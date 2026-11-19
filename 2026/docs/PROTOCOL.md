# PreMO protocol

Everything here was recovered from the legacy Windows software in
`../preMO remote-20261119T130500Z-1-001/` (binaries in `preMO remote/`, decompiled C# in
`decompiled/`). Each section names its source. Paths below are relative to `decompiled/`.

The PreMO speaks a small subset of **AES70 / OCP.1** (Open Control Architecture over TCP),
and a separate plain-bytes **boot-loader protocol** for firmware, both on **TCP port 58888**.
It announces itself over **mDNS as `_rtsp._tcp`**.

---

## 1. Connection

Source: `PreMoDAL/PreMoDAL/TCPHelp.cs`, `PreMoBLL/PreMoBLL/ConnectionBLL.cs`, `PreMo/PreMo/Home.cs` (`trvDevice_AfterSelect`, `CallWithAsync`).

1. Connect to `<ip>:58888`.
2. The device sends the 5 ASCII bytes `START`. Anything else counts as failure ("Retry or check IP").
3. PreMo.exe then sent a **FILE read of preset N** (N from its preset box, normally 0) to
   load the current settings, and started reading notifications.

PreMo.exe kept one connection per IP in a dictionary and reused it when you clicked back
to a device. It sent no keep-alives and had no reconnect logic. `premote` reconnects on its
own, waiting 1 s at first and doubling up to 15 s.

## 2. Byte order

The packet classes in `Model.dll` are `[StructLayout(LayoutKind.Sequential, Pack = 1)]` and
were copied to and from bytes with `Marshal.StructureToPtr` / `PtrToStructure`.

- **Commands** are byte-swapped field by field into network order (big-endian) by
  `PreMoBLL/CommandBLL.ByteMarshal()`.
- **Notifications and responses are not swapped back**, so the 32-bit object numbers in
  them are read little-endian. PreMo.exe's `switch (block) { case 16: … }` only works if the
  device sends them little-endian. `premote` reads them little-endian, and falls back to
  big-endian if the little-endian value is implausible (> 65535).

The decompiled C# lists class fields in a different order from the binary layout. The order
below is the real metadata order, taken from the IL of `Model.dll` (`ilspycmd -il`), and it
agrees with the offsets hard-coded in `ByteMarshal()`.

## 3. Command PDU (host → device), 28 bytes

Source: `Model/Model.AES70/Ocp1CommandPdu.cs`, `Ocp1Header.cs`, `Ocp1Command.cs`,
`Ocp1Parameters.cs`, `OcaMethodID.cs`; `OCAAes70/OCAAES70.OCP1/*.cs`;
`PreMoBLL/{Gain,Switch,DSP,File}BLL.cs`.

| Offset | Size | Field | Value |
|---:|---:|---|---|
| 0 | 1 | syncVal | `0x3B` |
| 1 | 2 | protocolVersion (BE) | 1 |
| 3 | 4 | pduSize (BE) | **23** (`5 + sizeof(Ocp1Command)`; AES70 would say 27; we send what the device was always sent) |
| 7 | 1 | pduType | 0 = OcaCmd |
| 8 | 2 | messageCount (BE) | 1 |
| 10 | 4 | commandSize (BE) | 18 |
| 14 | 4 | handle (BE) | the **block** ONo |
| 18 | 4 | targetONo (BE) | the **control** ONo |
| 22 | 2 | methodID.treeLevel (BE) | 4 |
| 24 | 2 | methodID.methodIndex (BE) | 1 |
| 26 | 1 | parameterCount | 1 |
| 27 | 1 | parameter | the value |

Example: Input 1 gain = 40 dB:
`3B 00 01 00 00 00 17 00 00 01 00 00 00 12 00 00 00 10 00 00 00 15 00 04 00 01 01 28`
(this exact byte string is a unit test in `src/ocp1.rs`).

Values are always one byte:

- switches: 0 or 1;
- gains and levels: the step number;
- treble/bass: a signed value in two's complement (`(byte)(value & 0xFF)` in `Home.cs`, so -5 → `0xFB`);
- routing and HPF: the choice number;
- presets: the preset number.

## 4. Notification PDU (device → host), 43 bytes

Source: `Model/Model.AES70/Ocp1NotificationPdu.cs`, `Ocp1Notification.cs`, `Ocp1NtfParams.cs`,
`Ocp1EventData.cs`, `OcaEvent.cs`; `PreMoBLL/OcaUnmarshalByte.cs`; `Home.WaitACK`,
`Home.ControlValueChange`.

| Offset | Size | Field | Use |
|---:|---:|---|---|
| 0 | 1 | syncVal `0x3B` | |
| 1–9 | 9 | header | byte 7 = **2** (OcaNtf) |
| 10 | 4 | notificationSize | ignored |
| 14 | 4 | targetONo (LE) | **block** |
| 18 | 4 | methodID | ignored |
| 22 | 1 | parameterCount | ignored |
| 23 | 4 | context | ignored |
| 27 | 4 | emitterONo (LE) | **control** that changed |
| 31 | 4 | eventID | ignored |
| 35 | 8 | eventParameters | value in byte 0; meters use all 8 |

PreMo.exe read 200 bytes at a time and parsed one PDU from the start of each buffer, so the
device must send one PDU per TCP segment. `premote` frames by type instead
(notification 43 bytes, response 97, anything else by `pduSize`) and resynchronises on
`0x3B`. That copes with PDUs that are split across reads or arrive back to back.

## 5. Response PDU (device → host): preset contents, 97 bytes

Source: `Model/Model.AES70/Ocp1ResponsePdu.cs`, `Ocp1Response.cs`; `PreMoBLL/ResponseBLL.cs`; `Home.WaitResponse`.

| Offset | Size | Field |
|---:|---:|---|
| 0–9 | 10 | sync + header, byte 7 = **3** (OcaRsp) |
| 10 | 4 | responseSize |
| 14 | 4 | handle |
| 18 | 1 | status (`OcaStatus`: 0 OK, 1 protocol version error, 2 device error, 3 locked, 4 bad format, 5 bad ONo, 6 parameter error, 7 out of range, 8 not implemented, 9 invalid request, 10 processing failed, 11 bad method, 12 partially succeeded, 13 timeout, 14 buffer overflow) |
| 19 | 1 | parameterCount |
| 20 | 1 | fileNum (preset number) |
| 21 | 76 | fileParam (preset bytes, §7) |

## 6. Object map

Sources: `PreMoBLL/GainBLL.cs`, `SwitchBLL.cs`, `DSPBLL.cs`, `FileBLL.cs`;
ranges from `Home.InitializeComponent`; notification handling from `Home.*Value()`.
The same table is in `src/objects.rs` and is printed by `premote controls`.

| Block | Name | ONo | Control | Range / values | Notes |
|---:|---|---:|---|---|---|
| 16 | CH1 (Input 1) | 17 | phantom | 0/1 | |
| | | 18 | phase | 0/1 | |
| | | 19 | source | 0 = mic, 1 = line | legacy `micline1` |
| | | 20 | mute | 0/1 | |
| | | 21 | gain | 0–63 dB | slider snapped 1–7 to 0 |
| | | 22 | pad | 0/1 | |
| | | 23 | HPF | 0–2 | |
| | | 24 | treble | −12…+12 dB (signed) | |
| | | 25 | bass | −12…+12 dB (signed) | |
| 48 | CH2 (Input 2) | 49–57 | same as CH1, +32 | | |
| 32 | DOWNLOAD1 | 33 | phase | 0/1 | |
| | | 34 | mute | 0/1 | |
| | | 35 | gain | 0–30 dB | |
| | | 36 | HPF | 0–2 | |
| | | 37 | treble | −12…+12 | |
| | | 38 | bass | −12…+12 | |
| 64 | DOWNLOAD2 | 65–70 | same as DOWNLOAD1, +32 | | |
| 80 | FRONT headphones | 81 | mute (reported) | 0/1 | |
| | | 82 | volume | 0–255 | |
| | | 83 | "push" (sent for mute) | 0/1 | the mute checkbox **sends 83**, the device **reports 81** |
| 96 | REAR headphones | 97 / 98 / 99 | mute / volume / push | as FRONT | |
| 112 | STREAM | 113 | headset routing | 0–5 | diagrams `img/route_01..06` |
| | | 114 | upload routing | 0–5 | diagrams `img/route_07..12` |
| | | 115 | stream gain | 0–9 dB | shared by both paths; UI showed it only for routing 3 |
| | | 116 | headset mute ("hmute") | 0/1 | |
| | | 117 | upload mute ("umute") | 0/1 | |
| 128 | tone generator | 129–131 | reported only | | PreMo.exe ignored these |
| 144 | SYSTEM | 145 | display brightness | 0–127 | sent with the gain method |
| | | 146 | display mode | 0/1 | |
| | | 147 | mic mute | 0/1 | UI linked it to both input mutes (on = inputs un-muted) |
| | | 148 | PPM switch | 0/1 | PreMo.exe sent 1 before switching device and from a test button |
| 160 | FILE | 161 | select preset | 0–9 | in `FileBLL`, not wired to any button |
| | | 162 | read preset | 0–9 | device answers with a response (§5) |
| | | 163 | write preset | 0–9 | in `FileBLL`, not wired to any button |
| | | 164 | (reported) | | ignored by PreMo.exe |
| 176 | PPM | any | meter frame | 8 signed bytes | §8 |

**Handle values.** `GainBLL` put the block number in `handle` for every block (CH1 16,
DOWNLOAD1 32, CH2 48, DOWNLOAD2 64, FRONT 80, REAR 96, STREAM 112, SYSTEM 144), and so did
the other BLLs. `premote` does the same.

## 7. Preset layout (76 bytes, `fileParam`)

Source: `Home.WaitResponse`. Bytes not listed were never read.

| Byte | Control | Byte | Control |
|---:|---|---:|---|
| 0 | Input 1 gain, when source = mic | 21 | Input 2 source |
| 1 | Input 2 gain, when source = mic | 22 | Input 1 pad |
| 2 | brightness | 23 | Input 2 pad |
| 3 | Download 1 gain | 24 | headset stream mute |
| 4 | Download 2 gain | 25 | Input 1 mute |
| 5 | stream gain | 26 | Input 2 mute |
| 6 | headset routing | 27 | upload mute |
| 7 | upload routing | 28 | Download 1 mute |
| 8 | front volume | 29 | Download 2 mute |
| 9 | rear volume | 30 | display mode |
| 10–13 | HPF: In 1, In 2, DL 1, DL 2 | 31 | Input 1 phantom |
| 14–17 | phase: In 1, In 2, DL 1, DL 2 | 32 | Input 2 phantom |
| 18 | Input 1 gain, when source = line | 33–36 | bass: In 1, In 2, DL 1, DL 2 (signed) |
| 19 | Input 2 gain, when source = line | 37–40 | treble: In 1, In 2, DL 1, DL 2 (signed) |
| 20 | Input 1 source (0 mic / 1 line) | 41 | mic mute |

The headphone mutes are not part of a preset.

## 8. PPM meters (block 176)

Source: `Home.PPMValue`, `PreMo/PPMMeter.cs`.

The 8 parameter bytes are signed dB values:
`[In1, In1 peak, In2, In2 peak, DL-L, DL-L peak, DL-R, DL-R peak]`.
0 is the top of the scale and −60 the bottom. The meter has 40 segments. The segment for a
value comes from this table, indexed by |dB|:

```
40 39 38 37 36 35 34 33 32 31 30 29 28 27 26 25 24 23 22 21
20 19 18 17 16 15 14 13 12 12 11 11 10 10  9  9  8  8  7  7
 6  6  6  5  5  5  4  4  4  4  3  3  3  3  2  2  2  2  1  1  0
```

Segments below 16 are green, 16–28 orange and 29 and up red. The scale reads "+3" at
segment 40 and `segment − 37` from segment 12 up (so 0 dB on the scale is a value of −3),
plus −30, −35, −45 and −55 at segments 10, 7, 4 and 1.

## 9. Firmware update (boot loader)

Source: `TCPRemote/TCPRemote/Form1.cs`, `PreMoDAL/UpdateDAL.cs`, `PreMoBLL/UpdateBLL.cs`,
`PreMoBLL/ReadFileBLL.cs`, `Model/Model/FileInfo.cs`.

Same port, plain bytes:

```
host → connect :58888
dev  → "START"
host → "REBOOT"                (6 bytes; TCPRemote's UPDATE button skips this when the
                                unit is already in its boot loader)
dev  → "DOWNLOAD"              (8 bytes, may be preceded by other text)
repeat for each packet n:
  host → [n mod 200][200 bytes of image]        (201 bytes)
  dev  → [n mod 200][201]                       (2-byte ack)
final:
  host → [n mod 200][len mod 200 bytes]         (sent even if 0 data bytes)
  dev  → 2-byte ack (never checked by the legacy code)
host → close
```

- The sequence number wraps at **200**, not 256. The legacy acceptance test was
  `ack[1] == 201 && ack[0] + (n / 200) * 200 == n`. If the ack did not match, it resent the
  same packet. `premote` gives up after 5 tries.
- Image limit: 512,000 bytes (the read buffer size). Larger files were silently truncated;
  `premote` refuses them.
- PreMo.exe's in-app updater slept 120 ms after each packet, TCPRemote.exe did not:
  `--delay-ms`.
- The sample image `Premo2,19/APP2.bin` is 129,484 bytes, so 647 full packets plus a final
  packet of 84 bytes: 648 packets in all.
- PreMo.exe's own Update form sent REBOOT on the *existing* control connection. `premote`
  closes the control link and opens a fresh one, the way TCPRemote.exe did.

## 10. Discovery

Source: `Home.Bonjour_Init`, `SimpleChat`, `ServiceFound`, `ServiceResolved`, `QueryAnswered`; `PreMoBLL/BonjourBLL.cs`.

- Bonjour COM objects: `DNSSDService` CLSID `24CD4DE9-FF84-4701-9DC1-9B69E0D1090A`,
  `DNSSDEventManager` CLSID `BEEB932A-8D4A-4619-AEFE-A836F988B221`. Without Bonjour,
  PreMo.exe showed "Bonjour Service is not available" and quit.
- It registered the PC as `<Windows user name>._rtsp._tcp` on a random UDP port (never
  used), browsed `_rtsp._tcp`, resolved each service, queried the A record and listed the
  device by the first label of its host name. Devices outside the PC's subnet
  (`BonjourBLL.FailSubNet`) were shown in grey.
- `BonjourBLL` (unused) browsed `_http._tcp` instead.
- `premote` browses `_rtsp._tcp.local.` with the pure-Rust `mdns-sd` crate and does not
  register itself.

## 11. Other things in the legacy code

- `PreMoBLL/JSONBLL.Device_List(host)` fetched `http://<host>.local./device_list`, which
  returns `{"ArchwaveAES67":{"version":…,"devices":[{"hostname":…,"IPaddress":…}]}}` (an
  Archwave AES67 module's device list). Nothing called it.
- The "View" menu opened `http://<device ip>` in Internet Explorer: the device has its own
  web page. The `premote` page links to it.
- `Login.cs` / `UserManagement.cs` read users from `UserInfo.xml` at a hard-coded path on
  the developer's PC. `Program.Main` opens `Home` directly, so the login was never shown.

---

## 12. Real hardware: PreMO "222" firmware (first capture, 2026-11-19)

Unit: `WBS-preMO-222` ("Desk preMO"), 44.44.44.150. Its web page on port 80 belongs to the
Archwave AES67 module inside the unit; the module's `/device_list` names the unit.
Capture: [`captures/premo-222-notifications.txt`](captures/premo-222-notifications.txt).

What differs from what PreMo.exe 1.22.0 expected:

- **No `START` banner.** TCP 58888 accepts the connection and stays silent, even straight
  after a reboot. A legacy FILE read (block 160, ONo 162) got no answer.
- **Notifications are standard AES70, big-endian, 42 bytes**, sent on their own when
  settings are changed on the front panel (no subscription was made):

```
off  bytes          field
 0   3b             sync
 1   00 01          protocolVersion 1
 3   00 00 00 25    pduSize 37  (= total length − 5, the same convention as the 1.22.0 commands)
 7   02             pduType: notification
 8   00 01          messageCount 1
10   00 00 00 1c    notificationSize 28
14   00 00 00 04    targetONo 4
18   00 03 00 01    methodID 3.1
22   02             parameterCount 2
23   00 01 00       context: blob, length 1, one 0x00 byte
26   00 00 36 b2    emitterONo (14002)
30   00 01 00 01    eventID 1.1 (PropertyChanged)
34   41 a8 00 00    value: float32, 21.0
38   00 00 00 00    (zero)
```

- **Values are float32 dB**, not single bytes. Objects seen:
  - **14002**: stepped 21.0 → 16.5 → 21.0 in 0.5 dB steps;
  - **11005**: 43.0 → 49.0 in 1 dB steps.

  Which front-panel controls these are still has to be matched up.
- **Caution:** at +50 s the unit sent 31 identical copies of one notification and dropped
  off the network. An over-fast port scan earlier had also taken it off the network, and a
  reboot brought it back. Its network stack is fragile: avoid bursts and keep to one
  connection.

So 1.22.0 matches an older PreMO firmware. `premote` now handles both:

- **Dialect detection.** After connecting it waits 2 s for `START`. If the banner comes, the
  unit is treated as **legacy** (1.22.0 protocol, preset 0 is read). If nothing comes, or
  protocol data arrives first, it is treated as **aes70** (222 firmware), and no legacy
  commands are sent.
- **Decoding.** The framer recognises the standard form by its big-endian sizes. These
  notifications are decoded as `PropertyChange { ono, event, value: f32 }`. Repeated
  identical values are dropped before they reach the page.
- **Page.** For aes70 units it shows a **Live objects** table (object number, value, time).
  The legacy controls, meters, presets, firmware and raw cards are hidden, because their
  object numbers and formats don't apply.
- **Testing.** `premote simulate --fw222` imitates this firmware, byte for byte from the
  capture. The unit test `decodes_real_222_capture` decodes two captured PDUs.

Still to do for 222: map object numbers to controls, then learn the setter (AES70 would be
`SetGain` 4.2 with a float32; to be confirmed carefully on a unit that is not on air).
