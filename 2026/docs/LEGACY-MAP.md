# Legacy code → premote

Where every part of the legacy software went. Paths on the left are under
`../preMO remote-20261119T130500Z-1-001/decompiled/`.

## PreMo.exe 1.22.0 (`PreMo/`)

| Legacy | What it did | Now |
|---|---|---|
| `Program.Main` | Opens `Home` (not `Login`) | `main.rs` `serve` |
| `Home` device tree + `timDevice` | Lists Bonjour devices under "PreMo", greys out other subnets | sidebar in `web/app.js`; `discovery.rs` (no subnet greying: the link state shows reachability) |
| `Home.trvDevice_AfterSelect`, `CallWithAsync` | Connect on 58888, reuse the connection, read preset N | `device.rs` `Device::session` |
| `Home.WaitACK`, `ControlValueChange`, `Channel1Value` … `PPMValue` | Read notifications, update widgets | `device.rs` `Device::handle` + `objects::by_state_ono`; `app.js` `widgets` |
| `Home.WaitResponse`, `Switch_Checked` | Apply a preset to the widgets | `objects::apply_preset`, `PRESET_LAYOUT`, `PRESET_GAINS` |
| `Home` slider/checkbox/number handlers (`trbInput1_MouseUp`, `micline1_Click`, `nUD1_HPF_ValueChanged`, …) | Send one command per change | `Device::set` via `objects::to_wire`; page widgets |
| `Home.SwitchOn/SwitchOff` | Switch commands | `Device::set` with 0/1 |
| `Home.viewMenu_Click` | Open `http://<ip>` in Internet Explorer | "device's own web page" link |
| `Home.editMenu_Click` → `Update` form | Firmware over the open connection | Firmware card → `POST /firmware` → `updater.rs` |
| `Home.btnUsrMng_Click` → `UserManagement`, `Login` | Users in `UserInfo.xml` (hard-coded developer path) | **dropped**: the login form was never shown in 1.22.0 |
| `PPMMeter` | 40-segment meter, `Bar_Number` table, colours | `drawMeters` in `app.js` + `--ppm-*` colours in `style.css` |
| `MICLINE`, `OnOffButton`, `ScrollBarEx`, `UserTrackBar`, `CustomControl1`, `CheckStyle`, `ChoseStyle` | Custom WinForms widgets | standard HTML controls styled in `style.css` |
| `Resources` `_1`…`_12`, `btncheck*`, `MICbutton`, `LINEbutton` | Routing diagrams, button art | extracted to `web/img/` |
| `AutoClosingMessageBox` | "Change Device" popup | dropped (switching device has no delay now) |

## Libraries

| Legacy | Now |
|---|---|
| `PreMoBLL/CommandBLL` (+ `ByteMarshal`, `ReverseBytes`) | `ocp1::encode_command` |
| `PreMoBLL/GainBLL`, `SwitchBLL`, `DSPBLL` | `objects::CONTROLS` (block + ONo) + `Device::set` |
| `PreMoBLL/FileBLL` | `objects::file_op` + `Device::preset` (select / write are now usable) |
| `PreMoBLL/NoticeBLL`, `ResponseBLL`, `OcaUnmarshalByte` | `ocp1::Framer`, `ocp1::decode_pdu` |
| `PreMoBLL/ConnectionBLL` (IP regex, START check) | `std::net::Ipv4Addr` parsing; banner check in `Device::session` |
| `PreMoBLL/BonjourBLL` (`GetNetNum`, `FailSubNet`) | `discovery.rs` (subnet test dropped) |
| `PreMoBLL/UpdateBLL`, `ReadFileBLL` | `updater.rs` |
| `PreMoBLL/JSONBLL` (`/device_list`) | not ported (never called); documented in PROTOCOL.md §11 |
| `PreMoDAL/TCPHelp`, `CommandDAL`, `ResponseDAL`, `NoticeDAL`, `UpdateDAL` | Tokio sockets in `device.rs` / `updater.rs` |
| `Model/Model.AES70/*` (PDU structs, `OcaStatus`, `OcaMessageType`) | `ocp1.rs` |
| `Model/Model/IPInfo` (connection dictionary) | `device::Registry` |
| `Model/Model/FileInfo` (firmware buffers) | `updater::packets` |
| `Model/Model/PeerData` | `discovery::Found` |
| `Model/Model.JSON/*` | not ported |
| `OCAAes70/OCAAES70.OCC.*` (`OcaRoot`, `OcaWorker`, `OcaGain`, `OcaBlock`, `OcaSubscriptionManager`) | object model folded into `objects.rs`; the subscription manager's event becomes the broadcast channel |
| `OCAAes70/OCAAES70.OCP1/*` (header, command, message builders) | `ocp1::encode_command` |
| `Common`, `DNSService` | empty helper assemblies; nothing to port |

## TCPRemote.exe ("PreMoUpdate", `TCPRemote/`)

| Legacy | Now |
|---|---|
| READFILE button (pick `.bin`, show length / packet count / remainder) | `premote update` prints these; the page shows them in the log |
| REBOOT button (`BootLoad`) | default behaviour of `premote update` / the Firmware card |
| UPDATE button (`UpdateBoot`) | `--no-reboot` / untick "Send REBOOT first" |
| `StartBoot` recursion + ack test | `updater::update` loop with retry limit |
| progress bar | `progress` events → `<progress>` on the page |
| `Connect()` / `timer1` | unused in the legacy code; not ported |
