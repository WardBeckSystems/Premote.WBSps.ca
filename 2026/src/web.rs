//! # Web server: the HTML page for each device
//!
//! `premote serve` runs this. Open `http://<host>:8080/` for the device list,
//! or `http://<host>:8080/device/<ip>` for one device's control page.
//! The page (in `web/`, compiled into the binary) is built at run time from
//! the object catalogue, so it always matches `objects::CONTROLS`.
//!
//! ## HTTP API (JSON)
//!
//! | Method | Path                               | Body / query                         | Does                                   |
//! |--------|------------------------------------|--------------------------------------|----------------------------------------|
//! | GET    | `/api/catalog`                     |                                      | controls, preset layout, meter names   |
//! | GET    | `/api/devices`                     |                                      | every known device and its state       |
//! | POST   | `/api/devices`                     | `{"ip":"10.0.0.5","name":"Desk"}`    | add a device by hand (connects)        |
//! | GET    | `/api/devices/{ip}`                |                                      | one device's state                     |
//! | POST   | `/api/devices/{ip}/set`            | `{"key":"ch1_gain","value":40}`      | change a control                       |
//! | POST   | `/api/devices/{ip}/preset`         | `{"op":"read","num":0}`              | preset read / select / write           |
//! | POST   | `/api/devices/{ip}/raw`            | `{"block":16,"ono":21,"value":40}`   | send any command (experiments)         |
//! | POST   | `/api/devices/{ip}/firmware`       | raw `.bin` body; `?reboot=true&delay_ms=0` | firmware update (progress on the WebSocket) |
//! | GET    | `/api/devices/{ip}/ws`             |                                      | WebSocket: state + live events         |
//!
//! The WebSocket first sends `{"event":"state",...}` and then every event
//! from [`crate::device::Event`]. It also accepts the same JSON commands as
//! the POST routes, tagged: `{"cmd":"set","key":"ch1_gain","value":40}`,
//! `{"cmd":"preset","op":"write","num":3}`, `{"cmd":"raw",...}`.

use crate::device::{Event, Registry};
use crate::objects;
use crate::updater::{self, UpdateOptions};
use axum::{
    body::Bytes,
    extract::{ws::Message, ws::WebSocket, Path, Query, State, WebSocketUpgrade},
    http::{header, StatusCode},
    response::{IntoResponse, Response},
    routing::{get, post},
    Json, Router,
};
use futures_util::{SinkExt, StreamExt};
use include_dir::{include_dir, Dir};
use serde::Deserialize;
use serde_json::json;
use std::net::{Ipv4Addr, SocketAddr};
use std::time::Duration;

/// The `web/` folder, compiled in.
static WEB: Dir = include_dir!("$CARGO_MANIFEST_DIR/web");

/// Shared server state.
#[derive(Clone)]
pub struct App {
    pub registry: Registry,
}

/// Build the router.
pub fn router(app: App) -> Router {
    Router::new()
        .route("/", get(index))
        .route("/device/:ip", get(index))
        .route("/assets/*path", get(asset))
        .route("/api/catalog", get(catalog))
        .route("/api/devices", get(list).post(add))
        .route("/api/devices/:ip", get(one))
        .route("/api/devices/:ip/set", post(set))
        .route("/api/devices/:ip/preset", post(preset))
        .route("/api/devices/:ip/raw", post(raw))
        .route("/api/devices/:ip/firmware", post(firmware))
        .route("/api/devices/:ip/ws", get(ws))
        // Firmware images are at most 512,000 bytes.
        .layer(axum::extract::DefaultBodyLimit::max(updater::MAX_IMAGE + 1024))
        .with_state(app)
}

/// Run the server until the process ends.
pub async fn serve(bind: SocketAddr, app: App) -> anyhow::Result<()> {
    let listener = tokio::net::TcpListener::bind(bind).await?;
    tracing::info!("web page on http://{bind}/");
    axum::serve(listener, router(app)).await?;
    Ok(())
}

/// Error helper: `(status, {"error": ...})`.
fn err(code: StatusCode, msg: impl Into<String>) -> Response {
    (code, Json(json!({ "error": msg.into() }))).into_response()
}

fn parse_ip(s: &str) -> Result<Ipv4Addr, Response> {
    s.parse().map_err(|_| err(StatusCode::BAD_REQUEST, format!("'{s}' is not an IPv4 address")))
}

/// The single page (it reads the device IP from the URL itself).
async fn index() -> Response {
    asset_response("index.html")
}

async fn asset(Path(path): Path<String>) -> Response {
    asset_response(&path)
}

fn asset_response(path: &str) -> Response {
    match WEB.get_file(path) {
        Some(f) => {
            let mime = match path.rsplit('.').next() {
                Some("html") => "text/html; charset=utf-8",
                Some("css") => "text/css; charset=utf-8",
                Some("js") => "text/javascript; charset=utf-8",
                Some("png") => "image/png",
                Some("jpg") => "image/jpeg",
                Some("svg") => "image/svg+xml",
                _ => "application/octet-stream",
            };
            ([(header::CONTENT_TYPE, mime)], f.contents()).into_response()
        }
        None => err(StatusCode::NOT_FOUND, format!("no file {path}")),
    }
}

async fn catalog() -> Response {
    Json(json!({
        "controls": objects::CONTROLS,
        "preset_layout": objects::PRESET_LAYOUT,
        "meters": objects::METER_NAMES,
    }))
    .into_response()
}

async fn list(State(app): State<App>) -> Response {
    Json(app.registry.list().await).into_response()
}

#[derive(Deserialize)]
struct AddBody {
    ip: String,
    name: Option<String>,
}

async fn add(State(app): State<App>, Json(b): Json<AddBody>) -> Response {
    let ip = match parse_ip(&b.ip) {
        Ok(ip) => ip,
        Err(r) => return r,
    };
    let d = app.registry.add(ip, b.name.as_deref()).await;
    Json(d.snapshot().await).into_response()
}

async fn one(State(app): State<App>, Path(ip): Path<String>) -> Response {
    let ip = match parse_ip(&ip) {
        Ok(ip) => ip,
        Err(r) => return r,
    };
    match app.registry.get(ip).await {
        Some(d) => Json(d.snapshot().await).into_response(),
        None => err(StatusCode::NOT_FOUND, "unknown device"),
    }
}

/// Commands accepted by POST routes and the WebSocket.
#[derive(Deserialize)]
#[serde(tag = "cmd", rename_all = "snake_case")]
enum Cmd {
    Set { key: String, value: i32 },
    Preset { op: String, num: u8 },
    Raw { block: u32, ono: u32, value: u8 },
}

/// Run a command against a device (creating/connecting it if needed).
async fn run_cmd(app: &App, ip: Ipv4Addr, cmd: Cmd) -> Result<(), String> {
    let d = app.registry.add(ip, None).await;
    match cmd {
        Cmd::Set { key, value } => d.set(&key, value).await,
        Cmd::Preset { op, num } => d.preset(&op, num).await,
        Cmd::Raw { block, ono, value } => d.raw(block, ono, value).await,
    }
}

async fn cmd_route(app: App, ip: String, cmd: Cmd) -> Response {
    let ip = match parse_ip(&ip) {
        Ok(ip) => ip,
        Err(r) => return r,
    };
    match run_cmd(&app, ip, cmd).await {
        Ok(()) => Json(json!({ "ok": true })).into_response(),
        Err(e) => err(StatusCode::BAD_REQUEST, e),
    }
}

#[derive(Deserialize)]
struct SetBody {
    key: String,
    value: i32,
}
async fn set(State(app): State<App>, Path(ip): Path<String>, Json(b): Json<SetBody>) -> Response {
    cmd_route(app, ip, Cmd::Set { key: b.key, value: b.value }).await
}

#[derive(Deserialize)]
struct PresetBody {
    op: String,
    num: u8,
}
async fn preset(State(app): State<App>, Path(ip): Path<String>, Json(b): Json<PresetBody>) -> Response {
    cmd_route(app, ip, Cmd::Preset { op: b.op, num: b.num }).await
}

#[derive(Deserialize)]
struct RawBody {
    block: u32,
    ono: u32,
    value: u8,
}
async fn raw(State(app): State<App>, Path(ip): Path<String>, Json(b): Json<RawBody>) -> Response {
    cmd_route(app, ip, Cmd::Raw { block: b.block, ono: b.ono, value: b.value }).await
}

#[derive(Deserialize)]
struct FwQuery {
    /// Send REBOOT first (default true).
    reboot: Option<bool>,
    /// Pause between packets in ms (default 0).
    delay_ms: Option<u64>,
}

/// Start a firmware update; progress arrives on the device's WebSocket.
async fn firmware(State(app): State<App>, Path(ip): Path<String>, Query(q): Query<FwQuery>, body: Bytes) -> Response {
    let ip = match parse_ip(&ip) {
        Ok(ip) => ip,
        Err(r) => return r,
    };
    if body.is_empty() || body.len() > updater::MAX_IMAGE {
        return err(StatusCode::BAD_REQUEST, format!("image must be 1..={} bytes", updater::MAX_IMAGE));
    }
    let dev = app.registry.add(ip, None).await;
    let opts = UpdateOptions {
        reboot: q.reboot.unwrap_or(true),
        packet_delay: Duration::from_millis(q.delay_ms.unwrap_or(0)),
        ..Default::default()
    };
    let image = body.to_vec();
    tokio::spawn(async move {
        // Close the control link and keep it closed during the update.
        let guard = dev.pause_for_update().await;
        tokio::time::sleep(Duration::from_millis(500)).await;
        let d2 = dev.clone();
        let mut progress = move |n: usize, total: usize, msg: &str| {
            if !msg.is_empty() {
                d2.report(Event::Log { text: format!("update: {msg}") });
            }
            if total > 0 {
                d2.report(Event::Progress { percent: (n * 100 / total) as u8, packet: n, packets: total });
            }
        };
        let addr = SocketAddr::from((dev.ip, dev.port));
        let result = updater::update(addr, &image, &opts, &mut progress).await;
        dev.report(Event::Log {
            text: match result {
                Ok(()) => "update: finished".into(),
                Err(e) => format!("update: FAILED: {e:#}"),
            },
        });
        drop(guard); // control link reconnects
    });
    (StatusCode::ACCEPTED, Json(json!({ "ok": true, "bytes": body.len() }))).into_response()
}

async fn ws(State(app): State<App>, Path(ip): Path<String>, upgrade: WebSocketUpgrade) -> Response {
    let ip = match parse_ip(&ip) {
        Ok(ip) => ip,
        Err(r) => return r,
    };
    upgrade.on_upgrade(move |socket| ws_session(app, ip, socket))
}

/// One browser tab watching one device.
async fn ws_session(app: App, ip: Ipv4Addr, socket: WebSocket) {
    let dev = app.registry.add(ip, None).await;
    let mut events = dev.subscribe();
    let (mut tx, mut rx) = socket.split();

    // Initial full state.
    let first = Event::State { state: dev.snapshot().await };
    if tx.send(Message::Text(serde_json::to_string(&first).unwrap())).await.is_err() {
        return;
    }
    loop {
        tokio::select! {
            ev = events.recv() => match ev {
                Ok(ev) => {
                    if tx.send(Message::Text(serde_json::to_string(&ev).unwrap())).await.is_err() { break; }
                }
                // Slow tab: skip what it missed and resend the full state.
                Err(tokio::sync::broadcast::error::RecvError::Lagged(_)) => {
                    let s = Event::State { state: dev.snapshot().await };
                    if tx.send(Message::Text(serde_json::to_string(&s).unwrap())).await.is_err() { break; }
                }
                Err(_) => break,
            },
            msg = rx.next() => match msg {
                Some(Ok(Message::Text(t))) => {
                    let reply = match serde_json::from_str::<Cmd>(&t) {
                        Ok(cmd) => run_cmd(&app, ip, cmd).await.err(),
                        Err(e) => Some(format!("bad command: {e}")),
                    };
                    if let Some(e) = reply {
                        let _ = tx.send(Message::Text(json!({"event":"error","text":e}).to_string())).await;
                    }
                }
                Some(Ok(Message::Close(_))) | None | Some(Err(_)) => break,
                _ => {}
            }
        }
    }
}
