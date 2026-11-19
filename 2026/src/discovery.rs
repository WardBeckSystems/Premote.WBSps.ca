//! # Device discovery (mDNS / DNS-SD)
//!
//! PreMo.exe used Apple's Bonjour for Windows through COM
//! (`Interop.Bonjour.dll`, CLSID 24CD4DE9-FF84-4701-9DC1-9B69E0D1090A):
//!
//! 1. it *registered* the PC itself as `<Windows user name>._rtsp._tcp`,
//! 2. *browsed* for `_rtsp._tcp` services,
//! 3. *resolved* each one to a host name, then queried that host's A record,
//! 4. listed it under "PreMo" by the first label of the host name, greyed out
//!    when the IP was outside the PC's own subnet (`BonjourBLL.FailSubNet`).
//!
//! (An earlier version in `PreMoBLL.BonjourBLL` browsed `_http._tcp`
//! instead; the shipped form uses `_rtsp._tcp`.)
//!
//! Here the pure-Rust `mdns-sd` crate does steps 2–3; no Bonjour install is
//! needed. We do not register the host (the device never looked for it).
//! Every resolved IPv4 address is added to the [`Registry`], which connects
//! to it on port 58888.

use crate::device::Registry;
use mdns_sd::{ServiceDaemon, ServiceEvent};
use std::net::IpAddr;

/// The service type PreMO units announce.
pub const SERVICE: &str = "_rtsp._tcp.local.";

/// A device found on the network.
#[derive(Debug, Clone, serde::Serialize)]
pub struct Found {
    /// Instance name (e.g. "PreMo-1234").
    pub name: String,
    /// Host name (e.g. "premo-1234.local.").
    pub host: String,
    pub addresses: Vec<IpAddr>,
    /// Port advertised in the SRV record (the control port is 58888
    /// regardless; PreMo.exe ignored this value too).
    pub port: u16,
}

/// Turn "premo-1234.local." / "PreMo 1234._rtsp._tcp.local." into a label.
fn short_name(full: &str) -> String {
    full.split('.').next().unwrap_or(full).to_string()
}

/// Browse forever, calling `on_found` for every resolved service.
/// Runs on a blocking thread because `mdns-sd` uses its own threads/channels.
pub fn browse(mut on_found: impl FnMut(Found) + Send + 'static) -> anyhow::Result<()> {
    let mdns = ServiceDaemon::new()?;
    let rx = mdns.browse(SERVICE)?;
    std::thread::spawn(move || {
        // Keep the daemon alive as long as the thread runs.
        let _mdns = mdns;
        while let Ok(event) = rx.recv() {
            if let ServiceEvent::ServiceResolved(info) = event {
                on_found(Found {
                    name: short_name(info.get_fullname()),
                    host: info.get_hostname().to_string(),
                    addresses: info.get_addresses().iter().copied().collect(),
                    port: info.get_port(),
                });
            }
        }
    });
    Ok(())
}

/// Start browsing and add every IPv4 device to the registry.
pub fn start(registry: Registry) -> anyhow::Result<()> {
    let handle = tokio::runtime::Handle::current();
    browse(move |f| {
        tracing::info!(name = %f.name, host = %f.host, addrs = ?f.addresses, "found {SERVICE}");
        for a in &f.addresses {
            if let IpAddr::V4(v4) = a {
                let (reg, v4, name) = (registry.clone(), *v4, short_name(&f.host));
                handle.spawn(async move {
                    reg.add(v4, Some(&name)).await;
                });
            }
        }
    })
}

/// One-shot scan for the `discover` command: print what answers in `secs`.
pub async fn scan(secs: u64) -> anyhow::Result<Vec<Found>> {
    let found = std::sync::Arc::new(std::sync::Mutex::new(Vec::<Found>::new()));
    let sink = found.clone();
    browse(move |f| sink.lock().unwrap().push(f))?;
    tokio::time::sleep(std::time::Duration::from_secs(secs)).await;
    let v = found.lock().unwrap().clone();
    Ok(v)
}
