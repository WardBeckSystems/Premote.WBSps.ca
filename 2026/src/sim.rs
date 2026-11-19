//! # PreMO simulator
//!
//! A stand-in device for development and tests (`premote simulate`). It
//! behaves the way the legacy remote *expected* a PreMO to behave; it is a
//! model of the protocol, not of the real firmware:
//!
//! * greets every TCP connection with `START`;
//! * accepts 28-byte OCP.1 commands, stores the value, and answers with a
//!   notification on the control's *state* ONo (so the headphone mutes come
//!   back on 81/97 after being sent on 83/99, like the real unit);
//! * FILE read (162) returns a 97-byte preset response; select (161) loads
//!   a preset and notifies every value; write (163) stores one;
//! * streams PPM meter notifications (block 176) ten times a second with
//!   moving test levels;
//! * firmware: `REBOOT` → `DOWNLOAD`, then acknowledges 201-byte packets
//!   with `[seq, 201]` and the final short packet with `[seq, len]`, and
//!   logs the image size and a checksum when done. A connection that sends a
//!   packet straight after `START` (TCPRemote's UPDATE button) is treated as
//!   a unit already in its boot loader.

use crate::objects::{self, block, file_op, CONTROLS};
use crate::ocp1::{self, PRESET_LEN};
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use std::time::Duration;
use tokio::io::{AsyncReadExt, AsyncWriteExt};
use tokio::net::{TcpListener, TcpStream};
use tokio::time::{sleep, timeout};

/// Shared device memory: current values and the 10 presets.
struct Memory {
    values: HashMap<&'static str, i32>,
    presets: Vec<[u8; PRESET_LEN]>,
}

impl Memory {
    fn new() -> Self {
        // Plausible power-on settings.
        let mut values: HashMap<&'static str, i32> = CONTROLS.iter().map(|c| (c.key, 0)).collect();
        for (k, v) in [
            ("ch1_gain", 30), ("ch2_gain", 24), ("dl1_gain", 10), ("dl2_gain", 10),
            ("front_volume", 128), ("rear_volume", 128), ("brightness", 90),
            ("stream_gain", 3), ("headset_route", 1), ("upload_route", 2), ("mic_mute", 1),
        ] {
            values.insert(k, v);
        }
        let first = objects::build_preset(|k| *values.get(k).unwrap_or(&0));
        Self { values, presets: vec![first; 10] }
    }
}

/// Run the simulator on `port` until the process ends.
pub async fn run(port: u16) -> anyhow::Result<()> {
    let listener = TcpListener::bind(("0.0.0.0", port)).await?;
    tracing::info!("simulated PreMO listening on 0.0.0.0:{port}");
    let mem = Arc::new(Mutex::new(Memory::new()));
    loop {
        let (sock, peer) = listener.accept().await?;
        tracing::info!(%peer, "simulator: client connected");
        let mem = mem.clone();
        tokio::spawn(async move {
            if let Err(e) = client(sock, mem).await {
                tracing::info!(%peer, "simulator: client gone: {e}");
            }
        });
    }
}

/// One client connection.
async fn client(mut sock: TcpStream, mem: Arc<Mutex<Memory>>) -> anyhow::Result<()> {
    sock.set_nodelay(true)?;
    sock.write_all(b"START").await?;

    // Peek at the first bytes to tell control, REBOOT and raw firmware apart.
    let mut first = vec![0u8; 512];
    let n = match timeout(Duration::from_secs(600), sock.read(&mut first)).await {
        Ok(r) => r?,
        Err(_) => return Ok(()),
    };
    if n == 0 {
        return Ok(());
    }
    first.truncate(n);
    if first.starts_with(b"REBOOT") {
        tracing::info!("simulator: REBOOT received, entering boot loader");
        sleep(Duration::from_millis(500)).await;
        sock.write_all(b"DOWNLOAD").await?;
        return boot_loader(sock, Vec::new()).await;
    }
    if first[0] != ocp1::SYNC {
        // TCPRemote "UPDATE": unit assumed to be in the boot loader already.
        return boot_loader(sock, first).await;
    }
    control(sock, mem, first).await
}

/// The OCP.1 control session.
async fn control(sock: TcpStream, mem: Arc<Mutex<Memory>>, first: Vec<u8>) -> anyhow::Result<()> {
    let (mut rd, wr) = sock.into_split();
    let wr = Arc::new(tokio::sync::Mutex::new(wr));

    // Meter stream: 10 frames/s, a slow sweep with peak hold a bit above it.
    let meter_wr = wr.clone();
    let meters = tokio::spawn(async move {
        let mut t = 0f32;
        loop {
            t += 0.1;
            let mut p = [0u8; 8];
            for ch in 0..4 {
                let level = (-30.0 + 24.0 * (t * (0.7 + ch as f32 * 0.31)).sin()).clamp(-60.0, 0.0);
                let peak = (level + 4.0).min(0.0);
                p[ch * 2] = (level as i8) as u8;
                p[ch * 2 + 1] = (peak as i8) as u8;
            }
            let pdu = ocp1::encode_notification(block::PPM, block::PPM + 1, p);
            if meter_wr.lock().await.write_all(&pdu).await.is_err() {
                break;
            }
            sleep(Duration::from_millis(100)).await;
        }
    });

    let mut pending = first;
    let mut buf = [0u8; 1024];
    let result = async {
        loop {
            // Commands are fixed 28-byte PDUs; resync on 0x3B.
            while pending.len() >= ocp1::COMMAND_LEN {
                if pending[0] != ocp1::SYNC {
                    pending.remove(0);
                    continue;
                }
                let pdu: Vec<u8> = pending.drain(..ocp1::COMMAND_LEN).collect();
                if let Some(cmd) = ocp1::decode_command(&pdu) {
                    for out in apply(&mem, cmd) {
                        wr.lock().await.write_all(&out).await?;
                    }
                }
            }
            let n = rd.read(&mut buf).await?;
            if n == 0 {
                return anyhow::Ok(());
            }
            pending.extend_from_slice(&buf[..n]);
        }
    }
    .await;
    meters.abort();
    result
}

/// Apply one command to the simulated memory; return the PDUs to send back.
fn apply(mem: &Arc<Mutex<Memory>>, cmd: ocp1::Command) -> Vec<Vec<u8>> {
    let mut m = mem.lock().unwrap();
    let mut out = Vec::new();
    if cmd.block == block::FILE {
        let n = (cmd.value as usize).min(9);
        match cmd.ono {
            file_op::READ => {
                // Report the live settings when preset 0 is read on connect,
                // otherwise the stored preset.
                let p = if n == 0 { objects::build_preset(|k| *m.values.get(k).unwrap_or(&0)) } else { m.presets[n] };
                out.push(ocp1::encode_preset_response(n as u8, &p).to_vec());
            }
            file_op::WRITE => {
                let p = objects::build_preset(|k| *m.values.get(k).unwrap_or(&0));
                m.presets[n] = p;
                tracing::info!("simulator: stored preset {n}");
                out.push(ocp1::encode_notification(block::FILE, file_op::WRITE, [n as u8, 0, 0, 0, 0, 0, 0, 0]).to_vec());
            }
            file_op::SELECT => {
                let p = m.presets[n];
                for (k, v) in objects::apply_preset(&p) {
                    m.values.insert(k, v);
                    let c = objects::by_key(k).unwrap();
                    let b = objects::to_wire(c, v).unwrap_or(0);
                    out.push(ocp1::encode_notification(c.block, c.state_ono, [b, 0, 0, 0, 0, 0, 0, 0]).to_vec());
                }
                tracing::info!("simulator: selected preset {n}");
            }
            _ => {}
        }
        return out;
    }
    match CONTROLS.iter().find(|c| c.block == cmd.block && c.send_ono == cmd.ono) {
        Some(c) => {
            let v = objects::from_wire(c, cmd.value);
            m.values.insert(c.key, v);
            tracing::info!("simulator: {} = {v}", c.key);
            out.push(ocp1::encode_notification(c.block, c.state_ono, [cmd.value, 0, 0, 0, 0, 0, 0, 0]).to_vec());
        }
        None => tracing::warn!("simulator: unknown object {}/{} = {}", cmd.block, cmd.ono, cmd.value),
    }
    out
}

/// Boot loader: receive packets, acknowledge each, report the image.
async fn boot_loader(mut sock: TcpStream, mut pending: Vec<u8>) -> anyhow::Result<()> {
    let mut image = Vec::new();
    let mut buf = [0u8; 1024];
    let mut packets = 0usize;
    loop {
        // Gather one packet: up to 201 bytes, or fewer if the sender pauses
        // (that is the final packet).
        while pending.len() < 201 {
            match timeout(Duration::from_millis(300), sock.read(&mut buf)).await {
                Ok(Ok(0)) | Ok(Err(_)) => break,
                Ok(Ok(n)) => pending.extend_from_slice(&buf[..n]),
                Err(_) => break, // pause: short final packet
            }
        }
        if pending.is_empty() {
            break;
        }
        let take = pending.len().min(201);
        let pkt: Vec<u8> = pending.drain(..take).collect();
        image.extend_from_slice(&pkt[1..]);
        packets += 1;
        sock.write_all(&[pkt[0], pkt.len() as u8]).await?;
        if pkt.len() < 201 {
            break; // final packet
        }
    }
    let sum: u32 = image.iter().map(|&b| b as u32).sum();
    tracing::info!("simulator: firmware received: {} bytes in {packets} packets, byte sum {sum}", image.len());
    Ok(())
}
