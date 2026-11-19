# Verification

## Done (2026-11-19, against the simulator)

| Check | How | Result |
|---|---|---|
| Command bytes match PreMo.exe | unit test `command_matches_legacy_layout`: expected bytes worked out by hand from the struct layout + `ByteMarshal` | pass |
| Notification / response framing, split and glued PDUs, junk bytes | unit test `notification_round_trip_and_framing` | pass |
| Big-endian object numbers tolerated | unit test `big_endian_onos_are_tolerated` | pass |
| Object map: unique keys, no clashing block/ONo, value conversion, signed EQ, gain snap | unit test `keys_unique_and_wire_values` | pass |
| Preset encode/decode, mic vs line gain slot | unit test `preset_round_trip` | pass |
| Firmware packetising: 200-byte packets, empty final packet, wrap at 200 | unit test `packetising` | pass |
| Connect, `START`, preset 0 load | `premote serve --device 127.0.0.1 --port 59999` + `premote simulate --port 59999`; `GET /api/devices` | connected, values loaded |
| Set controls over HTTP, range checks | `POST /set` for `ch1_bass` −7, `front_mute` 1, `ch1_hpf` 9 | first two applied and confirmed by the simulator's notification (front mute sent on 83, reported on 81); third refused with `ch1_hpf must be 0..=2` |
| Set over the WebSocket, errors, preset recall | node WebSocket client | `value` events from host then device; `error` for an unknown key; recalling preset 4 replayed every stored value |
| Preset store | `POST /preset {"op":"write","num":4}` | stored |
| Meters | `GET /api/devices/{ip}` and the page | four channels updating ten times a second |
| Firmware via CLI | `premote update 127.0.0.1 APP2.bin --port 59999` | 129,484 bytes in 648 packets; simulator byte sum 11,503,363 = file byte sum |
| Firmware via the page's endpoint | `POST /firmware` with APP2.bin | same size and sum; control link closed during the update and reconnected afterwards |
| CLI `set` | `premote set 127.0.0.1 ch1_gain 44 --port 59999` | host and device both reported 44 |
| Discovery | `premote discover` on the office network | found `_rtsp._tcp` services; the one answering unit (a WBS 32ME) is not a PreMO and refused 58888, as expected |
| Web page | headless Chrome against the simulator, no console errors | screenshot in `docs/device-page.png` |
| Build | `cargo build`, `cargo test` | no warnings, 6/6 tests pass |

## Still to check on a real PreMO

These come from reading the legacy code; only a real unit can confirm them.

1. **Byte order of notification object numbers.** Assumed little-endian (the only way
   PreMo.exe's `switch` statements can work). Big-endian falls back automatically.
2. **Notification length 43 / response length 97.** Taken from the .NET structs. If the
   device sends longer PDUs, the framer resynchronises on `0x3B`, but values could be
   misread: check the Log card for "unmapped notification" lines.
3. **Does the device echo a notification after every command?** PreMo.exe didn't depend on
   it (it moved its own widget). `premote` updates its state immediately anyway.
4. **Meter start.** It is unclear whether meters stream by themselves or need the System →
   PPM switch (148). Try toggling it if the meters stay still.
5. **Mic mute (147) polarity.** The legacy UI showed "Mic mute" ON as both inputs *un*-muted.
6. **Firmware.** Try `--no-reboot` / REBOOT on a unit you can recover. If packets need
   pacing, use `--delay-ms 120` (what PreMo.exe's own updater used).
7. **Several clients at once.** PreMo.exe kept one connection per unit. Avoid running
   PreMo.exe and `premote` against the same unit until it is known that the device accepts
   two connections.
