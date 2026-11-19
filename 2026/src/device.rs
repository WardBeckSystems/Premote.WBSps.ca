//! # Device connections
//!
//! One [`Device`] per PreMO. It owns a background task that:
//!
//! 1. opens TCP port 58888 and waits for the 5-byte ASCII banner `START`
//!    (`PreMoDAL.TCPHelp.GetConnectionAsync`),
//! 2. asks for preset 0 with a FILE "read" command (block 160, ONo 162),
//!    exactly what PreMo.exe did on connect, so the page shows the
//!    device's current settings (`Home.CallWithAsync`),
//! 3. then reads notifications forever and folds them into [`DeviceState`],
//! 4. reconnects with back-off if the link drops.
//!
//! Commands from the web page or CLI go through an mpsc queue to the same
//! socket. Every change (state, meters, connection, log lines) is published
//! on a broadcast channel that the WebSocket handlers subscribe to.
//!
//! Differences from PreMo.exe, on purpose:
//! * The stream is framed properly ([`crate::ocp1::Framer`]) instead of
//!   "read 200 bytes and parse from byte 0".
//! * A command updates our copy of the state immediately (optimistic), like
//!   the legacy UI did by moving its own slider; a later notification from
//!   the device overrides it.
//! * Reconnection is automatic.

use crate::objects::{self, block, file_op};
use crate::ocp1::{self, Incoming};
use serde::Serialize;
use std::collections::BTreeMap;
use std::net::{Ipv4Addr, SocketAddr};
use std::sync::Arc;
use std::time::Duration;
use tokio::io::{AsyncReadExt, AsyncWriteExt};
use tokio::net::TcpStream;
use tokio::sync::{broadcast, mpsc, RwLock};
use tokio::time::{sleep, timeout};

/// TCP port of the PreMO control (and update) service.
pub const CONTROL_PORT: u16 = 58888;

/// How long to wait for the `START` banner after connecting.
const BANNER_TIMEOUT: Duration = Duration::from_secs(5);

/// Connection state shown on the page.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize)]
#[serde(rename_all = "snake_case")]
pub enum Link {
    Disconnected,
    Connecting,
    Connected,
    /// The control link is paused while a firmware update runs.
    Updating,
}

/// Latest meter frame (block 176), in dB (0 = top, -60 = bottom).
#[derive(Debug, Clone, Default, Serialize)]
pub struct Meters {
    /// `[level, peak]` for Input 1, Input 2, Download L, Download R.
    pub channels: [[i8; 2]; 4],
}

/// Everything we know about one device.
#[derive(Debug, Clone, Serialize)]
pub struct DeviceState {
    pub ip: Ipv4Addr,
    /// Name from mDNS (or the IP when added by hand).
    pub name: String,
    pub link: Link,
    /// Control values by key (see `objects::CONTROLS`); missing = unknown yet.
    pub values: BTreeMap<String, i32>,
    pub meters: Meters,
    /// Preset number last loaded from the device.
    pub preset: Option<u8>,
    /// Notifications we could not map to a known control: "block/ono" -> byte.
    pub unknown: BTreeMap<String, u8>,
}

/// Events pushed to web clients.
#[derive(Debug, Clone, Serialize)]
#[serde(tag = "event", rename_all = "snake_case")]
pub enum Event {
    /// Full state (sent first, and after a preset load).
    State { state: DeviceState },
    /// One control changed; `source` is "device" or "host".
    Value { key: String, value: i32, source: &'static str },
    /// New meter frame.
    Meters { meters: Meters },
    /// Link state changed.
    Link { link: Link },
    /// Human-readable log line (connection, raw PDUs, updater progress).
    Log { text: String },
    /// Firmware update progress, 0–100.
    Progress { percent: u8, packet: usize, packets: usize },
}

/// Commands queued for the connection task.
#[derive(Debug)]
enum Outgoing {
    /// Raw command PDU fields.
    Command { block: u32, ono: u32, value: u8 },
}

/// A handle to one device; cheap to clone.
#[derive(Clone)]
pub struct Device {
    pub ip: Ipv4Addr,
    pub port: u16,
    state: Arc<RwLock<DeviceState>>,
    events: broadcast::Sender<Event>,
    tx: mpsc::Sender<Outgoing>,
    /// Set while a firmware update owns the device; the control task backs off.
    paused: Arc<tokio::sync::Mutex<()>>,
}

impl Device {
    /// Create the handle and start its connection task.
    pub fn spawn(ip: Ipv4Addr, port: u16, name: String) -> Self {
        let (events, _) = broadcast::channel(512);
        let (tx, rx) = mpsc::channel(64);
        let dev = Device {
            ip,
            port,
            state: Arc::new(RwLock::new(DeviceState {
                ip,
                name,
                link: Link::Disconnected,
                values: BTreeMap::new(),
                meters: Meters::default(),
                preset: None,
                unknown: BTreeMap::new(),
            })),
            events,
            tx,
            paused: Arc::new(tokio::sync::Mutex::new(())),
        };
        tokio::spawn(dev.clone().run(rx));
        dev
    }

    /// Subscribe to this device's events.
    pub fn subscribe(&self) -> broadcast::Receiver<Event> {
        self.events.subscribe()
    }

    /// Snapshot of the current state.
    pub async fn snapshot(&self) -> DeviceState {
        self.state.read().await.clone()
    }

    /// Rename (when mDNS gives us a better name than the IP).
    pub async fn set_name(&self, name: &str) {
        self.state.write().await.name = name.to_string();
    }

    fn emit(&self, e: Event) {
        // No subscribers is fine.
        let _ = self.events.send(e);
    }

    fn log(&self, text: impl Into<String>) {
        let text = text.into();
        tracing::info!(ip = %self.ip, "{text}");
        self.emit(Event::Log { text });
    }

    /// Change a control by key, e.g. `set("ch1_gain", 40)`.
    pub async fn set(&self, key: &str, value: i32) -> Result<(), String> {
        let c = objects::by_key(key).ok_or_else(|| format!("unknown control '{key}'"))?;
        if !c.writable {
            return Err(format!("{key} is report-only"));
        }
        let wire = objects::to_wire(c, value)?;
        self.tx
            .send(Outgoing::Command { block: c.block, ono: c.send_ono, value: wire })
            .await
            .map_err(|_| "device task stopped".to_string())?;
        // Optimistic update, like PreMo.exe moving its own slider.
        let shown = objects::from_wire(c, wire);
        self.state.write().await.values.insert(c.key.to_string(), shown);
        self.emit(Event::Value { key: c.key.to_string(), value: shown, source: "host" });
        Ok(())
    }

    /// Preset operation: "read" (load from device), "select", "write" (store).
    pub async fn preset(&self, op: &str, num: u8) -> Result<(), String> {
        if num > 9 {
            return Err("preset number must be 0..=9".into());
        }
        let ono = match op {
            "read" => file_op::READ,
            "select" => file_op::SELECT,
            "write" => file_op::WRITE,
            _ => return Err(format!("unknown preset operation '{op}'")),
        };
        self.raw(block::FILE, ono, num).await
    }

    /// Send any command (for experiments; logged).
    pub async fn raw(&self, block: u32, ono: u32, value: u8) -> Result<(), String> {
        self.tx
            .send(Outgoing::Command { block, ono, value })
            .await
            .map_err(|_| "device task stopped".to_string())
    }

    /// Hold the device for a firmware update: the control link is closed
    /// and stays closed until the returned guard is dropped.
    pub async fn pause_for_update(&self) -> tokio::sync::OwnedMutexGuard<()> {
        let g = self.paused.clone().lock_owned().await;
        self.set_link(Link::Updating).await;
        g
    }

    /// Forward firmware-update progress to the page.
    pub fn report(&self, e: Event) {
        self.emit(e);
    }

    async fn set_link(&self, link: Link) {
        self.state.write().await.link = link;
        self.emit(Event::Link { link });
    }

    /// The connection task: connect, run, reconnect.
    async fn run(self, mut rx: mpsc::Receiver<Outgoing>) {
        let mut backoff = Duration::from_secs(1);
        loop {
            // Wait here while a firmware update owns the device.
            drop(self.paused.lock().await);
            self.set_link(Link::Connecting).await;
            match self.session(&mut rx).await {
                Ok(()) => backoff = Duration::from_secs(1),
                Err(e) => self.log(format!("link: {e}")),
            }
            if self.paused.try_lock().is_ok() {
                self.set_link(Link::Disconnected).await;
            }
            sleep(backoff).await;
            backoff = (backoff * 2).min(Duration::from_secs(15));
        }
    }

    /// One connected session; returns when the link drops.
    async fn session(&self, rx: &mut mpsc::Receiver<Outgoing>) -> anyhow::Result<()> {
        let addr = SocketAddr::from((self.ip, self.port));
        let mut sock = timeout(Duration::from_secs(5), TcpStream::connect(addr)).await??;
        sock.set_nodelay(true)?;

        // 1. Banner: the device greets every new connection with "START".
        let mut banner = [0u8; 5];
        timeout(BANNER_TIMEOUT, sock.read_exact(&mut banner)).await??;
        if &banner != b"START" {
            anyhow::bail!("unexpected banner {:?}", String::from_utf8_lossy(&banner));
        }
        self.set_link(Link::Connected).await;
        self.log(format!("connected to {addr}"));

        // 2. Load the current settings: FILE read of preset 0.
        sock.write_all(&ocp1::encode_command(block::FILE, file_op::READ, 0)).await?;

        // 3. Pump both directions until something fails.
        let (mut rd, mut wr) = sock.into_split();
        let mut framer = ocp1::Framer::new();
        let mut buf = [0u8; 1024];
        loop {
            tokio::select! {
                n = rd.read(&mut buf) => {
                    let n = n?;
                    if n == 0 { anyhow::bail!("device closed the connection"); }
                    for pdu in framer.push(&buf[..n]) {
                        self.handle(pdu).await;
                    }
                }
                cmd = rx.recv() => {
                    let Some(Outgoing::Command { block, ono, value }) = cmd else { return Ok(()) };
                    wr.write_all(&ocp1::encode_command(block, ono, value)).await?;
                    tracing::debug!(ip = %self.ip, block, ono, value, "sent");
                }
                // A firmware update wants the device: drop the link.
                _ = async { loop { if self.paused.try_lock().is_err() { break } sleep(Duration::from_millis(200)).await } } => {
                    self.log("control link closed for firmware update");
                    return Ok(());
                }
            }
        }
    }

    /// Apply one PDU from the device.
    async fn handle(&self, pdu: Incoming) {
        match pdu {
            Incoming::Notification(n) if n.block == block::PPM => {
                // Meter frame: 8 signed dB bytes, level/peak pairs.
                let mut m = Meters::default();
                for ch in 0..4 {
                    m.channels[ch] = [n.params[ch * 2] as i8, n.params[ch * 2 + 1] as i8];
                }
                self.state.write().await.meters = m.clone();
                self.emit(Event::Meters { meters: m });
            }
            Incoming::Notification(n) if n.block == block::FILE => {
                self.log(format!("file event: object {} value {}", n.ono, n.params[0]));
            }
            Incoming::Notification(n) => match objects::by_state_ono(n.block, n.ono) {
                Some(c) => {
                    let v = objects::from_wire(c, n.params[0]);
                    self.state.write().await.values.insert(c.key.to_string(), v);
                    self.emit(Event::Value { key: c.key.to_string(), value: v, source: "device" });
                }
                None => {
                    let k = format!("{}/{}", n.block, n.ono);
                    self.state.write().await.unknown.insert(k.clone(), n.params[0]);
                    self.log(format!("unmapped notification {k} = {:?}", n.params));
                }
            },
            Incoming::Preset(p) => {
                if p.status != 0 {
                    self.log(format!("preset {} read failed: {}", p.file_num, ocp1::status_name(p.status)));
                    return;
                }
                let snapshot = {
                    let mut st = self.state.write().await;
                    for (k, v) in objects::apply_preset(&p.data) {
                        st.values.insert(k.to_string(), v);
                    }
                    st.preset = Some(p.file_num);
                    st.clone()
                };
                self.log(format!("loaded preset {} from the device", p.file_num));
                self.emit(Event::State { state: snapshot });
            }
            Incoming::Other { pdu_type, bytes } => {
                self.log(format!("PDU type {pdu_type}: {}", hex(&bytes)));
            }
        }
    }
}

/// Bytes as hex, for log lines.
pub fn hex(b: &[u8]) -> String {
    b.iter().map(|x| format!("{x:02X}")).collect::<Vec<_>>().join(" ")
}

/// All devices the host knows about, by IP.
#[derive(Clone, Default)]
pub struct Registry {
    inner: Arc<RwLock<BTreeMap<Ipv4Addr, Device>>>,
    /// Port used for new devices (58888, or the simulator's port in tests).
    port: u16,
}

impl Registry {
    pub fn new(port: u16) -> Self {
        Self { inner: Default::default(), port }
    }

    /// Get or create (and start connecting to) a device.
    pub async fn add(&self, ip: Ipv4Addr, name: Option<&str>) -> Device {
        let mut map = self.inner.write().await;
        if let Some(d) = map.get(&ip) {
            if let Some(n) = name {
                d.set_name(n).await;
            }
            return d.clone();
        }
        let d = Device::spawn(ip, self.port, name.map(str::to_string).unwrap_or_else(|| ip.to_string()));
        map.insert(ip, d.clone());
        d
    }

    pub async fn get(&self, ip: Ipv4Addr) -> Option<Device> {
        self.inner.read().await.get(&ip).cloned()
    }

    pub async fn list(&self) -> Vec<DeviceState> {
        let devs: Vec<Device> = self.inner.read().await.values().cloned().collect();
        let mut out = Vec::new();
        for d in devs {
            out.push(d.snapshot().await);
        }
        out
    }
}
