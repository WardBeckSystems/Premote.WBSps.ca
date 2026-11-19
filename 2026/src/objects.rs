//! # The PreMO object map
//!
//! Every control on the PreMO is an AES70 object addressed by a number
//! ("ONo"). Controls are grouped in *blocks*; the block's ONo goes in the
//! command `handle` field and the control's ONo in `targetONo`.
//!
//! This table is the single source of truth for the whole program: the
//! device connection uses it to build commands and to apply notifications,
//! the web page is generated from it (`GET /api/catalog`), and the
//! simulator uses it to know what a real device would hold.
//!
//! Sources in the legacy code (`preMO remote-.../decompiled/`):
//! * `PreMoBLL/GainBLL.cs`    – gain/level controls per block label
//! * `PreMoBLL/SwitchBLL.cs`  – on/off controls
//! * `PreMoBLL/DSPBLL.cs`     – HPF / treble / bass / routing
//! * `PreMoBLL/FileBLL.cs`    – preset select / read / write
//! * `PreMo/Home.cs`          – slider ranges (`InitializeComponent`), how
//!                              notifications are applied (`ControlValueChange`)
//!                              and how a preset maps onto the controls
//!                              (`WaitResponse`)
//!
//! ## Blocks
//!
//! | Block ONo | Name (legacy label) | What it is                                |
//! |-----------|---------------------|-------------------------------------------|
//! | 16        | CH1                 | microphone/line input 1                   |
//! | 32        | DOWNLOAD1           | return ("download") channel 1             |
//! | 48        | CH2                 | microphone/line input 2                   |
//! | 64        | DOWNLOAD2           | return ("download") channel 2             |
//! | 80        | FRONT               | front headphone output                    |
//! | 96        | REAR                | rear headphone output                     |
//! | 112       | STREAM              | stream routing, stream gain, stream mutes |
//! | 128       | (tone generator)    | 129–131 reported, never sent by the app   |
//! | 144       | SYSTEM              | display brightness/mode, mic mute, PPM    |
//! | 160       | FILE                | presets 0–9: select / read / write        |
//! | 176       | PPM                 | meter levels (notifications only)         |

use serde::Serialize;

/// Block numbers.
pub mod block {
    pub const CH1: u32 = 16;
    pub const DOWNLOAD1: u32 = 32;
    pub const CH2: u32 = 48;
    pub const DOWNLOAD2: u32 = 64;
    pub const FRONT: u32 = 80;
    pub const REAR: u32 = 96;
    pub const STREAM: u32 = 112;
    pub const TONE: u32 = 128;
    pub const SYSTEM: u32 = 144;
    pub const FILE: u32 = 160;
    pub const PPM: u32 = 176;
}

/// File (preset) operations in block 160; the parameter is the preset number.
pub mod file_op {
    /// 161: select preset N (FileBLL label "select"; unused by PreMo.exe's UI).
    pub const SELECT: u32 = 161;
    /// 162: read preset N; the device answers with a 97-byte response
    /// holding the preset contents. PreMo.exe sends this right after
    /// connecting to load the current settings.
    pub const READ: u32 = 162;
    /// 163: write (store) the current settings as preset N (label "write";
    /// unused by PreMo.exe's UI).
    pub const WRITE: u32 = 163;
}

/// What kind of value a control holds; drives both validation and the
/// widget drawn on the web page.
#[derive(Debug, Clone, Copy, Serialize)]
#[serde(tag = "type", rename_all = "snake_case")]
pub enum Kind {
    /// On/off checkbox; sent as 0 or 1.
    Switch,
    /// Unsigned step value, sent as one byte (gains, volumes, brightness).
    Level { min: i32, max: i32, unit: &'static str },
    /// Signed value sent as a two's-complement byte (treble/bass, ±12 dB).
    Signed { min: i32, max: i32, unit: &'static str },
    /// One of N numbered choices (HPF 0–2, routing 0–5).
    Choice { max: i32 },
}

/// One controllable (or reported) value on the device.
#[derive(Debug, Clone, Copy, Serialize)]
pub struct Control {
    /// Stable key used by the web API, the CLI and the state map.
    pub key: &'static str,
    /// Human label for the web page.
    pub label: &'static str,
    /// Group heading on the web page (one card per group).
    pub group: &'static str,
    /// Block ONo (command `handle`).
    pub block: u32,
    /// ONo the host *sends* to change this control.
    pub send_ono: u32,
    /// ONo the device *reports* in notifications. Equal to `send_ono` for
    /// every control except the headphone mutes, where PreMo.exe sends the
    /// "push" object (83/99) but the device reports "mute" (81/97).
    pub state_ono: u32,
    /// Value type and range.
    pub kind: Kind,
    /// Whether the host may change it (false = report only).
    pub writable: bool,
    /// Extra notes shown as a tooltip on the web page.
    pub note: &'static str,
}

/// Shorthand constructors so the table below stays readable.
const fn sw(key: &'static str, label: &'static str, group: &'static str, block: u32, ono: u32) -> Control {
    Control { key, label, group, block, send_ono: ono, state_ono: ono, kind: Kind::Switch, writable: true, note: "" }
}
const fn lvl(key: &'static str, label: &'static str, group: &'static str, block: u32, ono: u32, max: i32, unit: &'static str) -> Control {
    Control { key, label, group, block, send_ono: ono, state_ono: ono, kind: Kind::Level { min: 0, max, unit }, writable: true, note: "" }
}
const fn eq(key: &'static str, label: &'static str, group: &'static str, block: u32, ono: u32) -> Control {
    Control { key, label, group, block, send_ono: ono, state_ono: ono, kind: Kind::Signed { min: -12, max: 12, unit: "dB" }, writable: true, note: "" }
}
const fn choice(key: &'static str, label: &'static str, group: &'static str, block: u32, ono: u32, max: i32) -> Control {
    Control { key, label, group, block, send_ono: ono, state_ono: ono, kind: Kind::Choice { max }, writable: true, note: "" }
}
const fn with_note(mut c: Control, note: &'static str) -> Control {
    c.note = note;
    c
}

use block::*;

/// Every control, in the order the web page shows them.
pub const CONTROLS: &[Control] = &[
    // ---- Input 1 (block 16) ---------------------------------------------
    with_note(lvl("ch1_gain", "Gain", "Input 1", CH1, 21, 63, "dB"),
        "0–63 dB. PreMo.exe snapped 1–7 to 0 (no gain below 8 dB except off)."),
    with_note(sw("ch1_source", "Line (off = mic)", "Input 1", CH1, 19), "Mic/line selector (legacy 'micline1')."),
    sw("ch1_phantom", "Phantom +48 V", "Input 1", CH1, 17),
    sw("ch1_pad", "Pad", "Input 1", CH1, 22),
    sw("ch1_phase", "Phase invert", "Input 1", CH1, 18),
    sw("ch1_mute", "Mute", "Input 1", CH1, 20),
    with_note(choice("ch1_hpf", "High-pass filter", "Input 1", CH1, 23, 2), "0 = off, 1 and 2 = filter settings."),
    eq("ch1_treble", "Treble", "Input 1", CH1, 24),
    eq("ch1_bass", "Bass", "Input 1", CH1, 25),
    // ---- Input 2 (block 48) ---------------------------------------------
    with_note(lvl("ch2_gain", "Gain", "Input 2", CH2, 53, 63, "dB"),
        "0–63 dB. PreMo.exe snapped 1–7 to 0 (no gain below 8 dB except off)."),
    with_note(sw("ch2_source", "Line (off = mic)", "Input 2", CH2, 51), "Mic/line selector (legacy 'micline2')."),
    sw("ch2_phantom", "Phantom +48 V", "Input 2", CH2, 49),
    sw("ch2_pad", "Pad", "Input 2", CH2, 54),
    sw("ch2_phase", "Phase invert", "Input 2", CH2, 50),
    sw("ch2_mute", "Mute", "Input 2", CH2, 52),
    with_note(choice("ch2_hpf", "High-pass filter", "Input 2", CH2, 55, 2), "0 = off, 1 and 2 = filter settings."),
    eq("ch2_treble", "Treble", "Input 2", CH2, 56),
    eq("ch2_bass", "Bass", "Input 2", CH2, 57),
    // ---- Download (return) 1 (block 32) ---------------------------------
    lvl("dl1_gain", "Gain", "Download 1", DOWNLOAD1, 35, 30, "dB"),
    sw("dl1_phase", "Phase invert", "Download 1", DOWNLOAD1, 33),
    sw("dl1_mute", "Mute", "Download 1", DOWNLOAD1, 34),
    choice("dl1_hpf", "High-pass filter", "Download 1", DOWNLOAD1, 36, 2),
    eq("dl1_treble", "Treble", "Download 1", DOWNLOAD1, 37),
    eq("dl1_bass", "Bass", "Download 1", DOWNLOAD1, 38),
    // ---- Download (return) 2 (block 64) ---------------------------------
    lvl("dl2_gain", "Gain", "Download 2", DOWNLOAD2, 67, 30, "dB"),
    sw("dl2_phase", "Phase invert", "Download 2", DOWNLOAD2, 65),
    sw("dl2_mute", "Mute", "Download 2", DOWNLOAD2, 66),
    choice("dl2_hpf", "High-pass filter", "Download 2", DOWNLOAD2, 68, 2),
    eq("dl2_treble", "Treble", "Download 2", DOWNLOAD2, 69),
    eq("dl2_bass", "Bass", "Download 2", DOWNLOAD2, 70),
    // ---- Headphones (blocks 80 / 96) ------------------------------------
    with_note(lvl("front_volume", "Front volume", "Headphones", FRONT, 82, 255, ""), "0–255 steps."),
    Control { key: "front_mute", label: "Front mute", group: "Headphones", block: FRONT, send_ono: 83, state_ono: 81,
        kind: Kind::Switch, writable: true,
        note: "Sent as object 83 ('push'), reported back as object 81 ('mute'), exactly as PreMo.exe did." },
    with_note(lvl("rear_volume", "Rear volume", "Headphones", REAR, 98, 255, ""), "0–255 steps."),
    Control { key: "rear_mute", label: "Rear mute", group: "Headphones", block: REAR, send_ono: 99, state_ono: 97,
        kind: Kind::Switch, writable: true,
        note: "Sent as object 99 ('push'), reported back as object 97 ('mute'), exactly as PreMo.exe did." },
    // ---- Stream (block 112) ---------------------------------------------
    with_note(choice("headset_route", "Headset routing", "Stream", STREAM, 113, 5),
        "0–5: which sources feed the headset; see the routing diagrams."),
    with_note(choice("upload_route", "Upload routing", "Stream", STREAM, 114, 5),
        "0–5: which sources feed the upload (stream send); see the routing diagrams."),
    with_note(lvl("stream_gain", "Stream gain", "Stream", STREAM, 115, 9, "dB"),
        "0–9 dB. One value shared by the headset and upload paths; PreMo.exe showed it only for routing 3."),
    sw("headset_mute", "Headset stream mute", "Stream", STREAM, 116),
    sw("upload_mute", "Upload mute", "Stream", STREAM, 117),
    // ---- System (block 144) ---------------------------------------------
    with_note(lvl("brightness", "Display brightness", "System", SYSTEM, 145, 127, ""), "0–127."),
    sw("display_mode", "Display mode", "System", SYSTEM, 146),
    with_note(sw("mic_mute", "Mic mute (system)", "System", SYSTEM, 147),
        "PreMo.exe tied this to the two input mutes: when it is on, it showed both inputs un-muted, and the reverse."),
    with_note(sw("ppm", "PPM (meter) switch", "System", SYSTEM, 148),
        "PreMo.exe sent 1 here before switching to another device, and from a test button."),
    // ---- Tone generator (block 128): only ever reported -----------------
    Control { key: "tone_129", label: "Tone 129", group: "Tone generator", block: TONE, send_ono: 129, state_ono: 129,
        kind: Kind::Level { min: 0, max: 255, unit: "" }, writable: false, note: "Reported by the device; PreMo.exe ignored it." },
    Control { key: "tone_130", label: "Tone 130", group: "Tone generator", block: TONE, send_ono: 130, state_ono: 130,
        kind: Kind::Level { min: 0, max: 255, unit: "" }, writable: false, note: "Reported by the device; PreMo.exe ignored it." },
    Control { key: "tone_131", label: "Tone 131", group: "Tone generator", block: TONE, send_ono: 131, state_ono: 131,
        kind: Kind::Level { min: 0, max: 255, unit: "" }, writable: false, note: "Reported by the device; PreMo.exe ignored it." },
];

/// Look a control up by its key.
pub fn by_key(key: &str) -> Option<&'static Control> {
    CONTROLS.iter().find(|c| c.key == key)
}

/// Look a control up by the (block, ONo) pair a notification carries.
pub fn by_state_ono(block: u32, ono: u32) -> Option<&'static Control> {
    CONTROLS.iter().find(|c| c.block == block && c.state_ono == ono)
}

/// Turn a user value into the byte that goes on the wire, checking range.
///
/// * switches: any non-zero is 1
/// * signed EQ values: two's complement (`(byte)(value & 0xFF)` in Home.cs)
/// * input gains 1–7 are snapped to 0, as PreMo.exe's sliders did
pub fn to_wire(c: &Control, value: i32) -> Result<u8, String> {
    let check = |min: i32, max: i32| {
        if value < min || value > max {
            Err(format!("{} must be {min}..={max}, got {value}", c.key))
        } else {
            Ok(())
        }
    };
    match c.kind {
        Kind::Switch => Ok((value != 0) as u8),
        Kind::Level { min, max, .. } => {
            check(min, max)?;
            let v = if (c.key == "ch1_gain" || c.key == "ch2_gain") && (1..8).contains(&value) { 0 } else { value };
            Ok(v as u8)
        }
        Kind::Signed { min, max, .. } => {
            check(min, max)?;
            Ok((value as i8) as u8)
        }
        Kind::Choice { max } => {
            check(0, max)?;
            Ok(value as u8)
        }
    }
}

/// Turn a byte from the device back into the user value.
pub fn from_wire(c: &Control, b: u8) -> i32 {
    match c.kind {
        Kind::Switch => (b != 0) as i32,
        Kind::Signed { .. } => (b as i8) as i32,
        _ => b as i32,
    }
}

/// Where each control lives inside the 76-byte preset (`fileParam`),
/// from `Home.WaitResponse`. Bytes not listed were never read by PreMo.exe.
///
/// Input gains are special: the preset holds two gains per input, one for
/// mic (bytes 0/1) and one for line (bytes 18/19); the one shown depends on
/// the source switch (bytes 20/21). See [`apply_preset`].
pub const PRESET_LAYOUT: &[(usize, &str)] = &[
    (2, "brightness"),
    (3, "dl1_gain"),
    (4, "dl2_gain"),
    (5, "stream_gain"),
    (6, "headset_route"),
    (7, "upload_route"),
    (8, "front_volume"),
    (9, "rear_volume"),
    (10, "ch1_hpf"),
    (11, "ch2_hpf"),
    (12, "dl1_hpf"),
    (13, "dl2_hpf"),
    (14, "ch1_phase"),
    (15, "ch2_phase"),
    (16, "dl1_phase"),
    (17, "dl2_phase"),
    (20, "ch1_source"),
    (21, "ch2_source"),
    (22, "ch1_pad"),
    (23, "ch2_pad"),
    (24, "headset_mute"),
    (25, "ch1_mute"),
    (26, "ch2_mute"),
    (27, "upload_mute"),
    (28, "dl1_mute"),
    (29, "dl2_mute"),
    (30, "display_mode"),
    (31, "ch1_phantom"),
    (32, "ch2_phantom"),
    (33, "ch1_bass"),
    (34, "ch2_bass"),
    (35, "dl1_bass"),
    (36, "dl2_bass"),
    (37, "ch1_treble"),
    (38, "ch2_treble"),
    (39, "dl1_treble"),
    (40, "dl2_treble"),
    (41, "mic_mute"),
];

/// Preset byte offsets of the mic-mode and line-mode gain for each input.
pub const PRESET_GAINS: &[(&str, usize, usize, usize)] = &[
    // (control, mic-gain byte, line-gain byte, source-switch byte)
    ("ch1_gain", 0, 18, 20),
    ("ch2_gain", 1, 19, 21),
];

/// Decode a preset into `(control key, value)` pairs.
pub fn apply_preset(data: &[u8]) -> Vec<(&'static str, i32)> {
    let mut out = Vec::new();
    for &(idx, key) in PRESET_LAYOUT {
        if let (Some(&b), Some(c)) = (data.get(idx), by_key(key)) {
            out.push((c.key, from_wire(c, b)));
        }
    }
    for &(key, mic, line, src) in PRESET_GAINS {
        let is_line = data.get(src).copied().unwrap_or(0) != 0;
        if let Some(&b) = data.get(if is_line { line } else { mic }) {
            out.push((key, b as i32));
        }
    }
    out
}

/// Build a preset from values (the simulator's stored settings).
pub fn build_preset(get: impl Fn(&str) -> i32) -> [u8; crate::ocp1::PRESET_LEN] {
    let mut p = [0u8; crate::ocp1::PRESET_LEN];
    for &(idx, key) in PRESET_LAYOUT {
        if let Some(c) = by_key(key) {
            p[idx] = to_wire(c, get(key)).unwrap_or(0);
        }
    }
    for &(key, mic, line, src) in PRESET_GAINS {
        let g = get(key) as u8;
        // Keep the gain in the slot that matches the current source.
        if p[src] != 0 { p[line] = g } else { p[mic] = g }
    }
    p
}

/// PPM meter notifications (block 176): 8 signed dB values, from
/// `Home.PPMValue`:
/// `[ch1, ch1 peak, ch2, ch2 peak, download L, download L peak, download R, download R peak]`.
/// 0 is the top of the scale, -60 the bottom.
pub const METER_NAMES: [&str; 4] = ["Input 1", "Input 2", "Download L", "Download R"];

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn keys_unique_and_wire_values() {
        for (i, a) in CONTROLS.iter().enumerate() {
            for b in &CONTROLS[i + 1..] {
                assert_ne!(a.key, b.key);
                assert!(!(a.block == b.block && a.state_ono == b.state_ono), "{} / {}", a.key, b.key);
            }
        }
        let treble = by_key("ch1_treble").unwrap();
        assert_eq!(to_wire(treble, -5).unwrap(), 0xFB);
        assert_eq!(from_wire(treble, 0xFB), -5);
        assert_eq!(to_wire(by_key("ch1_gain").unwrap(), 5).unwrap(), 0);
        assert!(to_wire(by_key("ch1_hpf").unwrap(), 3).is_err());
    }

    #[test]
    fn preset_round_trip() {
        let vals = |k: &str| match k {
            "ch1_gain" => 40,
            "ch1_source" => 1,
            "ch2_gain" => 22,
            "ch1_bass" => -6,
            "upload_route" => 4,
            _ => 0,
        };
        let p = build_preset(vals);
        assert_eq!(p[18], 40, "line gain slot used when source = line");
        let got: std::collections::HashMap<_, _> = apply_preset(&p).into_iter().collect();
        assert_eq!(got["ch1_gain"], 40);
        assert_eq!(got["ch2_gain"], 22);
        assert_eq!(got["ch1_bass"], -6);
        assert_eq!(got["upload_route"], 4);
    }
}
