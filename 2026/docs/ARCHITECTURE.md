# Architecture

```
                         ┌──────────────────────── premote serve ───────────────────────┐
 browser tab ──HTTP────▶ │ web.rs   axum router: page, /api/*, WebSocket per device      │
 (web/ page) ◀─WS events │    │ commands (set / preset / raw / firmware)                 │
                         │    ▼                                                          │
                         │ device.rs  Registry ── Device (one per IP) ── connection task │──TCP 58888──▶ PreMO
                         │    ▲            state + broadcast events    ▲  OCP.1 (ocp1.rs)│◀─notifications─
                         │    │                                        │                 │
                         │ discovery.rs  mDNS browse _rtsp._tcp ───────┘ adds devices    │
                         │ updater.rs    firmware upload (own TCP connection) ───────────│──TCP 58888──▶ boot loader
                         │ objects.rs    the object map every other module reads        │
                         └──────────────────────────────────────────────────────────────┘
```

## Modules (`src/`)

| File | Responsibility |
|---|---|
| `main.rs` | Command line (`serve`, `discover`, `set`, `preset`, `update`, `simulate`, `controls`) and wiring |
| `ocp1.rs` | Byte layouts: encode commands, decode notifications/responses, a `Framer` that splits the TCP stream into PDUs. Encoders for the device side, used by the simulator. Unit tests pin the exact legacy bytes. |
| `objects.rs` | The object map: every control's block, send/report object numbers, value type and range, preset byte; value↔byte conversion; preset decode/encode. **The single source of truth**: the page, CLI, device task and simulator all read it. |
| `device.rs` | `Device`: one task per unit that connects, waits for `START`, loads preset 0, reads notifications into `DeviceState`, sends queued commands and reconnects with back-off. `Registry`: all devices by IP. `Event`: what the page is told. |
| `discovery.rs` | mDNS browsing on a background thread; every IPv4 address found is added to the registry |
| `updater.rs` | The boot-loader protocol: packetising, REBOOT/DOWNLOAD handshake, acks, retries, progress callback |
| `web.rs` | HTTP API, WebSocket per device, page files compiled in with `include_dir!` |
| `sim.rs` | A simulated PreMO (control, meters, presets, boot loader) for testing |

## Concurrency

- Tokio multi-threaded runtime.
- **Per device:** one task owns the socket. Commands reach it through an `mpsc` queue (64
  deep). State is behind `RwLock<DeviceState>`, and changes go out on a `broadcast` channel
  (512 deep).
- **Per browser tab:** one WebSocket task subscribes to its device's broadcast. If the tab
  falls behind (`Lagged`), it is sent a fresh full state instead of the missed events.
- **Firmware update:** `Device::pause_for_update()` takes a mutex. The connection task sees
  it, closes the control socket and waits. The updater then opens its own connection.
  Dropping the guard lets the control link reconnect.
- **Discovery:** `mdns-sd` runs its own thread; results are handed back to the Tokio
  runtime.

## Data flow for one change

1. The user drags **Input 1 → Gain** to 40 and releases it.
2. `app.js` sends `{"cmd":"set","key":"ch1_gain","value":40}` on the WebSocket.
3. `web.rs` passes it to `Device::set`. `objects::to_wire` checks the range (0–63) and
   applies the legacy rule that 1–7 snaps to 0.
4. The command is queued. The connection task writes the 28-byte PDU (block 16, ONo 21,
   value 40).
5. The state is updated at once and a `value` event (`source: "host"`) is sent to every tab.
6. The device answers with a notification (block 16, emitter 21, value 40). It is mapped
   back through `objects::by_state_ono` and broadcast with `source: "device"`.

Changes made on the device itself (front panel, another controller) only go through step 6.

## Web API

See the table at the top of `src/web.rs`. In short:

- `GET /api/catalog`, `GET/POST /api/devices`, `GET /api/devices/{ip}`
- `POST /api/devices/{ip}/set | preset | raw | firmware`
- `GET /api/devices/{ip}/ws`: WebSocket

WebSocket messages from the server (JSON, field `event`):

| event | fields | when |
|---|---|---|
| `state` | `state` (whole `DeviceState`) | on connect, after a preset load, after lag |
| `value` | `key`, `value`, `source` (`host` / `device`) | a control changed |
| `meters` | `meters.channels` = 4 × `[level, peak]` dB | every meter notification |
| `link` | `link` (`connecting`, `connected`, `disconnected`, `updating`) | connection change |
| `log` | `text` | connection, unmapped objects, updater messages |
| `progress` | `percent`, `packet`, `packets` | firmware upload |
| `error` | `text` | a command from this tab was refused |

Messages to the server use the field `cmd`: `set {key,value}`, `preset {op,num}`,
`raw {block,ono,value}`.

## The page (`web/`)

- `index.html`: the layout, one page for every device. The device comes from the URL
  `/device/<ip>`.
- `app.js`: fetches `/api/catalog` and builds one card per group and one widget per
  control:
  - switch → toggle;
  - level or signed value → slider, sent on release as the legacy app did on MouseUp;
  - choice → numbered buttons, plus a routing diagram for the two routing controls.

  It also draws the 40-segment PPM meters with the legacy segment table.
- `style.css`: light and dark themes, and the PPM colours from `PPMMeter.cs`.
- `img/`: images extracted from PreMo.exe's resources:
  - `route_01..06.jpg`: headset routing 0–5;
  - `route_07..12.jpg`: upload routing 0–5;
  - the original button art (`btncheck*`, `MICbutton`, `LINEbutton`).

The page is compiled into the binary, so `premote` is a single file to deploy.

## Building

```sh
cargo build --release      # target/release/premote
cargo test                 # protocol, object map, preset and packetising tests
```

Built and tested with Rust 1.94. Dependencies: `tokio`, `axum` (HTTP + WebSocket), `serde`/`serde_json`,
`clap`, `mdns-sd`, `tracing`, `anyhow`, `include_dir`, `futures-util`.
