//! # Firmware updater ("PreMoUpdate")
//!
//! Replaces `TCPRemote.exe` (window title "PreMoUpdate", "Welcome WBS Audio
//! Update System !") and the Update form inside PreMo.exe
//! (`PreMoBLL.UpdateBLL`, `PreMoDAL.UpdateDAL`, `PreMoBLL.ReadFileBLL`).
//! The sample image in the archive is `Premo2,19/APP2.bin` (129,484 bytes).
//!
//! ## Protocol (TCP 58888, plain ASCII + raw bytes, no OCP.1)
//!
//! ```text
//! host                                   device
//!  |---- TCP connect :58888 ------------->|
//!  |<--------------------- "START" (5 B) -|   same banner as the control link
//!  |---- "REBOOT" (6 B) ----------------->|   only for "REBOOT"; skipped by
//!  |                                      |   TCPRemote's "UPDATE" button,
//!  |                                      |   which assumes the unit is
//!  |                                      |   already in its boot loader
//!  |<------------------- "DOWNLOAD" (8 B)-|   boot loader ready
//!  |---- packet 0 ----------------------->|   [seq][200 data bytes]
//!  |<------------------- [seq][201] (2 B)-|   ack: echoes seq, length 201
//!  |---- packet 1 ----------------------->|
//!  |      ...                             |
//!  |---- last packet -------------------->|   [seq][remaining bytes]
//!  |<------------------- [seq][len] ------|
//!  |---- close -------------------------->|
//! ```
//!
//! * The file is cut into `len / 200` full packets plus one final packet of
//!   `len % 200` bytes. The final packet is sent even when that is 0 bytes
//!   (a 1-byte packet holding only `seq`), as the legacy code did.
//! * `seq` is the packet number modulo 200 (one byte; it wraps at 200, not
//!   256). The legacy check was `ack[0] + (n / 200) * 200 == n`.
//! * Images were read with a 512,000-byte limit; larger files were silently
//!   truncated. We refuse them instead.
//! * The legacy loop resent a packet whenever the ack did not match. We do
//!   the same, but give up after [`MAX_RETRIES`] attempts instead of
//!   looping forever.
//! * PreMo.exe's own updater slept 120 ms between packets; TCPRemote.exe did
//!   not. The delay is a parameter (default 0, use 120 if a unit is slow).

use anyhow::{bail, Context, Result};
use std::net::SocketAddr;
use std::time::Duration;
use tokio::io::{AsyncReadExt, AsyncWriteExt};
use tokio::net::TcpStream;
use tokio::time::{sleep, timeout};

/// Data bytes per packet.
pub const CHUNK: usize = 200;
/// Largest image the legacy tools accepted.
pub const MAX_IMAGE: usize = 512_000;
/// Resend attempts per packet before giving up.
pub const MAX_RETRIES: usize = 5;

/// Options for one update run.
#[derive(Debug, Clone)]
pub struct UpdateOptions {
    /// Send "REBOOT" first and wait for "DOWNLOAD" (TCPRemote's REBOOT
    /// button). When false, start sending right after "START" (its UPDATE
    /// button, for a unit already in the boot loader).
    pub reboot: bool,
    /// Pause between packets.
    pub packet_delay: Duration,
    /// How long to wait for "DOWNLOAD" after "REBOOT".
    pub download_timeout: Duration,
}

impl Default for UpdateOptions {
    fn default() -> Self {
        Self { reboot: true, packet_delay: Duration::ZERO, download_timeout: Duration::from_secs(60) }
    }
}

/// Progress callback: (packets sent, total packets, message).
pub type Progress<'a> = &'a mut (dyn FnMut(usize, usize, &str) + Send);

/// Cut an image into the packets the boot loader expects.
pub fn packets(image: &[u8]) -> Vec<Vec<u8>> {
    let full = image.len() / CHUNK;
    let rest = image.len() % CHUNK;
    let mut out = Vec::with_capacity(full + 1);
    for n in 0..full {
        let mut p = Vec::with_capacity(CHUNK + 1);
        p.push((n % 200) as u8);
        p.extend_from_slice(&image[n * CHUNK..(n + 1) * CHUNK]);
        out.push(p);
    }
    // Final packet: always sent, possibly with no data.
    let mut last = Vec::with_capacity(rest + 1);
    last.push((full % 200) as u8);
    last.extend_from_slice(&image[full * CHUNK..]);
    out.push(last);
    out
}

/// Read 8-byte messages until one is "DOWNLOAD" (`UpdateDAL.WaitNotice`).
async fn wait_download(sock: &mut TcpStream, progress: Progress<'_>) -> Result<()> {
    let mut window = Vec::new();
    let mut buf = [0u8; 64];
    loop {
        let n = sock.read(&mut buf).await?;
        if n == 0 {
            bail!("device closed the connection before DOWNLOAD");
        }
        window.extend_from_slice(&buf[..n]);
        let text = String::from_utf8_lossy(&window).to_string();
        progress(0, 0, &format!("device: {}", text.trim()));
        if text.contains("DOWNLOAD") {
            return Ok(());
        }
        if window.len() > 256 {
            window.drain(..window.len() - 16);
        }
    }
}

/// Run a firmware update against `addr` (normally `<ip>:58888`).
pub async fn update(addr: SocketAddr, image: &[u8], opts: &UpdateOptions, progress: Progress<'_>) -> Result<()> {
    if image.is_empty() {
        bail!("image is empty");
    }
    if image.len() > MAX_IMAGE {
        bail!("image is {} bytes; the boot loader takes at most {MAX_IMAGE}", image.len());
    }
    let pk = packets(image);
    progress(0, pk.len(), &format!("{} bytes, {} packets of {CHUNK} + 1 of {}", image.len(), pk.len() - 1, image.len() % CHUNK));

    let mut sock = timeout(Duration::from_secs(5), TcpStream::connect(addr)).await.context("connect timed out")??;
    sock.set_nodelay(true)?;
    let mut banner = [0u8; 5];
    timeout(Duration::from_secs(5), sock.read_exact(&mut banner)).await.context("no START banner")??;
    if &banner != b"START" {
        bail!("unexpected banner {:?}", String::from_utf8_lossy(&banner));
    }
    progress(0, pk.len(), "START");

    if opts.reboot {
        sock.write_all(b"REBOOT").await?;
        progress(0, pk.len(), "sent REBOOT, waiting for DOWNLOAD");
        timeout(opts.download_timeout, wait_download(&mut sock, progress)).await.context("no DOWNLOAD from the boot loader")??;
    }

    for (n, p) in pk.iter().enumerate() {
        let mut tries = 0;
        loop {
            sock.write_all(p).await?;
            let mut ack = [0u8; 2];
            timeout(Duration::from_secs(10), sock.read_exact(&mut ack)).await.context("no ack")??;
            // ack = [seq, length]; length is 201 for a full packet. The legacy
            // code never checked the ack of the final packet, so neither do we.
            let last = n + 1 == pk.len();
            if last || (ack[0] == p[0] && ack[1] as usize == p.len()) {
                break;
            }
            tries += 1;
            progress(n, pk.len(), &format!("packet {n}: ack {:?}, expected [{}, {}]; resending", ack, p[0], p.len()));
            if tries >= MAX_RETRIES {
                bail!("packet {n} not acknowledged after {MAX_RETRIES} tries");
            }
        }
        progress(n + 1, pk.len(), "");
        if !opts.packet_delay.is_zero() {
            sleep(opts.packet_delay).await;
        }
    }
    progress(pk.len(), pk.len(), "update sent; the unit restarts on its own");
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn packetising() {
        let img: Vec<u8> = (0..=255u8).cycle().take(450).collect();
        let p = packets(&img);
        assert_eq!(p.len(), 3);
        assert_eq!((p[0][0], p[0].len()), (0, 201));
        assert_eq!((p[1][0], p[1].len()), (1, 201));
        assert_eq!((p[2][0], p[2].len()), (2, 51));
        assert_eq!(&p[2][1..], &img[400..]);
        // Exact multiple: final 1-byte packet still sent.
        let p = packets(&[0u8; 400]);
        assert_eq!(p.last().unwrap().len(), 1);
        // Sequence wraps at 200.
        let p = packets(&vec![0u8; 201 * CHUNK]);
        assert_eq!(p[200][0], 0);
    }
}
