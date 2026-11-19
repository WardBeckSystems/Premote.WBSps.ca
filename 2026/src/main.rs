//! # premote — PreMO host application
//!
//! A Rust rewrite of the legacy PreMO Windows tools (PreMo.exe 1.22.0 and
//! TCPRemote.exe), recovered from the binaries in
//! `../preMO remote-20261119T130500Z-1-001/`.
//!
//! ```text
//! premote serve      discover devices, keep them connected, host the web page
//! premote discover   list PreMO units announcing _rtsp._tcp on the network
//! premote set        change one control from the command line
//! premote preset     read / select / write a preset
//! premote update     firmware update (the TCPRemote.exe replacement)
//! premote simulate   run a fake PreMO for testing
//! premote controls   print the object map
//! ```
//!
//! Module map:
//! * [`ocp1`]      – byte layout of commands, notifications and responses
//! * [`objects`]   – every control: block, object numbers, range, preset byte
//! * [`device`]    – one live connection per device, state and events
//! * [`discovery`] – mDNS browsing (replaces Bonjour)
//! * [`updater`]   – firmware upload protocol
//! * [`web`]       – HTTP/WebSocket server and the HTML page
//! * [`sim`]       – device simulator

mod device;
mod discovery;
mod objects;
mod ocp1;
mod sim;
mod updater;
mod web;

use anyhow::{bail, Context, Result};
use clap::{Parser, Subcommand};
use std::net::{Ipv4Addr, SocketAddr};
use std::time::Duration;

#[derive(Parser)]
#[command(name = "premote", version, about = "PreMO host: device control, firmware update and a web control page")]
struct Cli {
    #[command(subcommand)]
    cmd: Command,
}

#[derive(Subcommand)]
enum Command {
    /// Run the host: discover devices, connect, and serve the web page.
    Serve {
        /// Address for the web page.
        #[arg(long, default_value = "0.0.0.0:8080")]
        bind: SocketAddr,
        /// Devices to connect to without waiting for discovery (repeatable).
        #[arg(long = "device", value_name = "IP")]
        devices: Vec<Ipv4Addr>,
        /// Do not browse mDNS (only use --device / devices added on the page).
        #[arg(long)]
        no_discovery: bool,
        /// Device control port (58888 on real units; change for the simulator).
        #[arg(long, default_value_t = device::CONTROL_PORT)]
        port: u16,
    },
    /// Browse the network for PreMO units (_rtsp._tcp) and print them.
    Discover {
        /// Seconds to listen.
        #[arg(long, default_value_t = 4)]
        secs: u64,
    },
    /// Set one control, e.g. `premote set 10.0.0.5 ch1_gain 40`.
    Set {
        ip: Ipv4Addr,
        /// Control key (see `premote controls`).
        key: String,
        value: i32,
        #[arg(long, default_value_t = device::CONTROL_PORT)]
        port: u16,
    },
    /// Preset operation: read (load), select, or write (store) preset 0–9.
    Preset {
        ip: Ipv4Addr,
        /// read | select | write
        op: String,
        num: u8,
        #[arg(long, default_value_t = device::CONTROL_PORT)]
        port: u16,
    },
    /// Firmware update (replaces TCPRemote.exe / "PreMoUpdate").
    Update {
        ip: Ipv4Addr,
        /// Firmware image, e.g. APP2.bin.
        file: std::path::PathBuf,
        /// Do not send REBOOT first (unit already in its boot loader;
        /// TCPRemote's UPDATE button).
        #[arg(long)]
        no_reboot: bool,
        /// Pause between packets, in ms (PreMo.exe's own updater used 120).
        #[arg(long, default_value_t = 0)]
        delay_ms: u64,
        #[arg(long, default_value_t = device::CONTROL_PORT)]
        port: u16,
    },
    /// Run a simulated PreMO (control, meters, presets, boot loader).
    Simulate {
        #[arg(long, default_value_t = device::CONTROL_PORT)]
        port: u16,
        /// Imitate the PreMO "222" firmware (no START, AES70 float notifications).
        #[arg(long)]
        fw222: bool,
    },
    /// Print every control: key, block, object numbers and range.
    Controls,
}

#[tokio::main]
async fn main() -> Result<()> {
    tracing_subscriber::fmt()
        .with_env_filter(tracing_subscriber::EnvFilter::try_from_default_env().unwrap_or_else(|_| "premote=info".into()))
        .init();
    let cli = Cli::parse();
    match cli.cmd {
        Command::Serve { bind, devices, no_discovery, port } => {
            let registry = device::Registry::new(port);
            for ip in devices {
                registry.add(ip, None).await;
            }
            if !no_discovery {
                if let Err(e) = discovery::start(registry.clone()) {
                    tracing::warn!("mDNS discovery unavailable: {e}; add devices by IP");
                }
            }
            web::serve(bind, web::App { registry }).await
        }
        Command::Discover { secs } => {
            println!("browsing {} for {secs} s ...", discovery::SERVICE);
            let found = discovery::scan(secs).await?;
            if found.is_empty() {
                println!("nothing answered");
            }
            for f in found {
                println!("{:<24} {:<32} {:?} port {}", f.name, f.host, f.addresses, f.port);
            }
            Ok(())
        }
        Command::Set { ip, key, value, port } => one_shot(ip, port, |d| async move { d.set(&key, value).await }).await,
        Command::Preset { ip, op, num, port } => one_shot(ip, port, |d| async move { d.preset(&op, num).await }).await,
        Command::Update { ip, file, no_reboot, delay_ms, port } => {
            let image = std::fs::read(&file).with_context(|| format!("reading {}", file.display()))?;
            let opts = updater::UpdateOptions {
                reboot: !no_reboot,
                packet_delay: Duration::from_millis(delay_ms),
                ..Default::default()
            };
            let mut progress = |n: usize, total: usize, msg: &str| {
                if !msg.is_empty() {
                    println!("{msg}");
                } else if total > 0 && (n % 50 == 0 || n == total) {
                    println!("packet {n}/{total} ({}%)", n * 100 / total);
                }
            };
            updater::update(SocketAddr::from((ip, port)), &image, &opts, &mut progress).await
        }
        Command::Simulate { port, fw222 } => sim::run(port, fw222).await,
        Command::Controls => {
            println!("{:<16} {:<15} {:>5} {:>5} {:>5}  range", "key", "group", "block", "send", "state");
            for c in objects::CONTROLS {
                let range = match c.kind {
                    objects::Kind::Switch => "0/1".to_string(),
                    objects::Kind::Level { min, max, unit } | objects::Kind::Signed { min, max, unit } => format!("{min}..{max} {unit}"),
                    objects::Kind::Choice { max } => format!("0..{max}"),
                };
                println!("{:<16} {:<15} {:>5} {:>5} {:>5}  {range}{}", c.key, c.group, c.block, c.send_ono, c.state_ono,
                    if c.writable { "" } else { "  (report only)" });
            }
            Ok(())
        }
    }
}

/// Connect, run one action, wait for it to be sent, print what came back.
async fn one_shot<F, Fut>(ip: Ipv4Addr, port: u16, act: F) -> Result<()>
where
    F: FnOnce(device::Device) -> Fut,
    Fut: std::future::Future<Output = Result<(), String>>,
{
    let d = device::Device::spawn(ip, port, ip.to_string());
    let mut ev = d.subscribe();
    // Wait for the link.
    let ok = tokio::time::timeout(Duration::from_secs(8), async {
        loop {
            if d.snapshot().await.link == device::Link::Connected {
                return;
            }
            tokio::time::sleep(Duration::from_millis(50)).await;
        }
    })
    .await;
    if ok.is_err() {
        bail!("could not connect to {ip}:{port}");
    }
    act(d.clone()).await.map_err(anyhow::Error::msg)?;
    // Show the device's answers for a moment.
    let _ = tokio::time::timeout(Duration::from_millis(800), async {
        while let Ok(e) = ev.recv().await {
            match e {
                device::Event::Value { key, value, source } => println!("{key} = {value} ({source})"),
                device::Event::Log { text } => println!("{text}"),
                _ => {}
            }
        }
    })
    .await;
    Ok(())
}
