//! # OCP.1 wire format, as spoken by the PreMO
//!
//! OCP.1 is the TCP/IP transport of AES70 (Open Control Architecture).
//! The PreMO uses a small, fixed subset of it. Everything in this file was
//! recovered from the legacy Windows remote (PreMo.exe 1.22.0), mainly:
//!
//! * `Model.AES70.*`            – the packet structs (`[StructLayout(Sequential, Pack = 1)]`)
//! * `OCAAES70.OCP1.*`          – how a command packet is filled in
//! * `PreMoBLL.CommandBLL`      – `ByteMarshal()`, which byte-swaps the
//!                                 little-endian .NET struct into network order
//! * `PreMoBLL.OcaUnmarshalByte` – how notifications/responses are read back
//!
//! The field order below is the real metadata order taken from the IL of
//! `Model.dll` (the decompiled C# lists fields in a different order).
//!
//! ## Command PDU (host → device), always 28 bytes
//!
//! ```text
//! off size field                     value used by PreMo.exe
//! --- ---- ------------------------  ------------------------------------------
//!  0   1   syncVal                   0x3B
//!  1   2   protocolVersion  (BE)     1
//!  3   4   pduSize          (BE)     23   (5 + sizeof(Ocp1Command)=18; NOT the
//!                                           AES70 "length after sync" of 27 —
//!                                           kept exactly as the device expects)
//!  7   1   pduType                   0 = OcaCmd
//!  8   2   messageCount     (BE)     1
//! 10   4   commandSize      (BE)     18
//! 14   4   handle           (BE)     the *block* ONo (16, 32, 48, ...)
//! 18   4   targetONo        (BE)     the *control* ONo (e.g. 21 = CH1 gain)
//! 22   2   methodID.treeLevel (BE)   4
//! 24   2   methodID.methodIndex (BE) 1
//! 26   1   parameterCount            1
//! 27   1   parameter                 the value byte
//! ```
//!
//! Every PreMO command carries exactly one value byte (switch 0/1, a gain
//! step, an EQ value as a two's-complement signed byte, a routing number or
//! a preset number).
//!
//! ## Notification PDU (device → host), 43 bytes
//!
//! Read by `Marshal.PtrToStructure` *without* byte swapping, so the 32-bit
//! object numbers arrive little-endian (otherwise PreMo.exe's
//! `switch (block) { case 16: ... }` could never have matched).
//!
//! ```text
//!  0   1   syncVal 0x3B
//!  1   9   header (pduType at offset 7 = 2 = OcaNtf)
//! 10   4   notificationSize
//! 14   4   targetONo   (LE)  -> the block (16, 32, ... 176)
//! 18   4   methodID
//! 22   1   parameterCount
//! 23   4   context
//! 27   4   emitterONo  (LE)  -> the control that changed (e.g. 21)
//! 31   4   eventID
//! 35   8   eventParameters   -> value in byte 0; PPM meters use all 8
//! ```
//!
//! ## Response PDU (device → host), 97 bytes: a preset ("file") read
//!
//! ```text
//!  0  10   sync + header (pduType at offset 7 = 3 = OcaRsp)
//! 10   4   responseSize
//! 14   4   handle
//! 18   1   statusCode (OcaStatus)
//! 19   1   parameterCount
//! 20   1   fileNum        (preset number 0..9)
//! 21  76   fileParam      (the preset contents, see objects::PRESET_LAYOUT)
//! ```

use serde::Serialize;

/// First byte of every OCP.1 PDU.
pub const SYNC: u8 = 0x3B;

/// Total length of a command PDU as sent by PreMo.exe.
pub const COMMAND_LEN: usize = 28;
/// Total length of a notification PDU (header + fixed notification struct).
pub const NOTIFICATION_LEN: usize = 43;
/// Total length of a preset/file response PDU.
pub const RESPONSE_LEN: usize = 97;
/// Number of preset bytes in a response (`fileParam`).
pub const PRESET_LEN: usize = 76;

/// `Model.AES70.OcaMessageType`: byte 7 of every PDU.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize)]
pub enum MessageType {
    /// 0: command, no response wanted (all PreMO commands are this).
    Cmd,
    /// 1: command, response required (not used by the PreMO remote).
    CmdRrq,
    /// 2: notification: a control value changed, or meter levels.
    Ntf,
    /// 3: response: answer to a preset "read".
    Rsp,
    /// 4: keep-alive (never seen in the legacy code).
    KeepAlive,
    /// Anything else (bad or unknown packet).
    Unknown(u8),
}

impl From<u8> for MessageType {
    fn from(b: u8) -> Self {
        match b {
            0 => Self::Cmd,
            1 => Self::CmdRrq,
            2 => Self::Ntf,
            3 => Self::Rsp,
            4 => Self::KeepAlive,
            other => Self::Unknown(other),
        }
    }
}

impl MessageType {
    /// The byte value written at offset 7.
    pub fn as_byte(self) -> u8 {
        match self {
            Self::Cmd => 0,
            Self::CmdRrq => 1,
            Self::Ntf => 2,
            Self::Rsp => 3,
            Self::KeepAlive => 4,
            Self::Unknown(b) => b,
        }
    }
}

/// `Model.AES70.OcaStatus`: status byte of a response.
pub fn status_name(code: u8) -> &'static str {
    match code {
        0 => "OK",
        1 => "PROTOCOL_VERSION_ERROR",
        2 => "DEVICE_ERROR",
        3 => "LOCKED",
        4 => "BAD_FORMAT",
        5 => "BAD_ONO",
        6 => "PARAMETER_ERROR",
        7 => "PARAMETER_OUT_OF_RANGE",
        8 => "NOT_IMPLEMENTED",
        9 => "INVALID_REQUEST",
        10 => "PROCESSING_FAILED",
        11 => "BAD_METHOD",
        12 => "PARTIALLY_SUCCEEDED",
        13 => "TIMEOUT",
        14 => "BUFFER_OVERFLOW",
        _ => "UNKNOWN",
    }
}

/// Build the 28-byte command PDU exactly as `GainBLL` / `SwitchBLL` /
/// `DSPBLL` / `FileBLL` did.
///
/// * `block`  – goes in the `handle` field (PreMo.exe put the block ONo there)
/// * `ono`    – the control's object number (`targetONo`)
/// * `value`  – the single parameter byte
pub fn encode_command(block: u32, ono: u32, value: u8) -> [u8; COMMAND_LEN] {
    let mut b = [0u8; COMMAND_LEN];
    b[0] = SYNC;
    // --- Ocp1Header -------------------------------------------------------
    b[1..3].copy_from_slice(&1u16.to_be_bytes()); // protocolVersion = 1
    b[3..7].copy_from_slice(&23u32.to_be_bytes()); // pduSize = 5 + 18 (sic)
    b[7] = MessageType::Cmd.as_byte(); // pduType = OcaCmd
    b[8..10].copy_from_slice(&1u16.to_be_bytes()); // messageCount = 1
    // --- Ocp1Command ------------------------------------------------------
    b[10..14].copy_from_slice(&18u32.to_be_bytes()); // commandSize = 18
    b[14..18].copy_from_slice(&block.to_be_bytes()); // handle = block ONo
    b[18..22].copy_from_slice(&ono.to_be_bytes()); // targetONo = control
    b[22..24].copy_from_slice(&4u16.to_be_bytes()); // methodID.treeLevel = 4
    b[24..26].copy_from_slice(&1u16.to_be_bytes()); // methodID.methodIndex = 1
    b[26] = 1; // parameterCount
    b[27] = value; // the parameter byte
    b
}

/// A decoded command (used by the simulator and by tests).
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct Command {
    pub block: u32,
    pub ono: u32,
    pub value: u8,
}

/// Decode a command PDU (the inverse of [`encode_command`]).
pub fn decode_command(b: &[u8]) -> Option<Command> {
    if b.len() < COMMAND_LEN || b[0] != SYNC || b[7] != MessageType::Cmd.as_byte() {
        return None;
    }
    Some(Command {
        block: u32::from_be_bytes(b[14..18].try_into().ok()?),
        ono: u32::from_be_bytes(b[18..22].try_into().ok()?),
        value: b[27],
    })
}

/// A decoded notification: "control `ono` in block `block` now has `params`".
#[derive(Debug, Clone, PartialEq, Eq, Serialize)]
pub struct Notification {
    /// Block ONo (targetONo field): 16 = CH1, ..., 176 = PPM meters.
    pub block: u32,
    /// Control ONo (emitterONo field): e.g. 21 = CH1 gain.
    pub ono: u32,
    /// The 8 event parameter bytes; single-value controls use byte 0.
    pub params: [u8; 8],
}

/// Read a u32 that the device sent in native (little-endian) order.
///
/// Safety net: object numbers are all < 256, so if the little-endian reading
/// is absurd but the big-endian one is small, the device is evidently using
/// network order and we take that instead. (The legacy app only ever handled
/// little-endian; this keeps us working if a firmware differs.)
fn read_ono(b: &[u8]) -> u32 {
    let le = u32::from_le_bytes([b[0], b[1], b[2], b[3]]);
    let be = u32::from_be_bytes([b[0], b[1], b[2], b[3]]);
    if le > 0xFFFF && be <= 0xFFFF {
        be
    } else {
        le
    }
}

/// Encode a notification as the device does (used by the simulator).
pub fn encode_notification(block: u32, ono: u32, params: [u8; 8]) -> [u8; NOTIFICATION_LEN] {
    let mut b = [0u8; NOTIFICATION_LEN];
    b[0] = SYNC;
    b[1..3].copy_from_slice(&1u16.to_be_bytes());
    b[3..7].copy_from_slice(&((NOTIFICATION_LEN - 1) as u32).to_be_bytes());
    b[7] = MessageType::Ntf.as_byte();
    b[8..10].copy_from_slice(&1u16.to_be_bytes());
    b[10..14].copy_from_slice(&((NOTIFICATION_LEN - 10) as u32).to_le_bytes());
    b[14..18].copy_from_slice(&block.to_le_bytes()); // targetONo (LE, see top)
    b[18..20].copy_from_slice(&4u16.to_le_bytes()); // methodID.treeLevel
    b[20..22].copy_from_slice(&1u16.to_le_bytes()); // methodID.methodIndex
    b[22] = 1; // parameterCount
    // 23..27 context = 0
    b[27..31].copy_from_slice(&ono.to_le_bytes()); // emitterONo (LE)
    b[31..33].copy_from_slice(&1u16.to_le_bytes()); // eventID.treeLevel
    b[33..35].copy_from_slice(&1u16.to_le_bytes()); // eventID.eventIndex
    b[35..43].copy_from_slice(&params);
    b
}

/// A decoded preset read response.
#[derive(Debug, Clone, PartialEq, Eq, Serialize)]
pub struct PresetResponse {
    /// OcaStatus byte (0 = OK).
    pub status: u8,
    /// Preset number that was read (0..9).
    pub file_num: u8,
    /// The 76 preset bytes (`fileParam`).
    pub data: Vec<u8>,
}

/// Encode a preset response as the device does (used by the simulator).
pub fn encode_preset_response(file_num: u8, data: &[u8; PRESET_LEN]) -> [u8; RESPONSE_LEN] {
    let mut b = [0u8; RESPONSE_LEN];
    b[0] = SYNC;
    b[1..3].copy_from_slice(&1u16.to_be_bytes());
    b[3..7].copy_from_slice(&((RESPONSE_LEN - 1) as u32).to_be_bytes());
    b[7] = MessageType::Rsp.as_byte();
    b[8..10].copy_from_slice(&1u16.to_be_bytes());
    b[10..14].copy_from_slice(&((RESPONSE_LEN - 10) as u32).to_le_bytes());
    // 14..18 handle = 0, 18 status = OK
    b[19] = 2; // parameterCount: fileNum + blob
    b[20] = file_num;
    b[21..21 + PRESET_LEN].copy_from_slice(data);
    b
}

/// Anything the device can send on the control connection.
#[derive(Debug, Clone, PartialEq, Eq, Serialize)]
pub enum Incoming {
    Notification(Notification),
    Preset(PresetResponse),
    /// A PDU of another type (kept for the log).
    Other { pdu_type: u8, bytes: Vec<u8> },
}

/// Splits the TCP byte stream into PDUs.
///
/// PreMo.exe never framed the stream: it read up to 200 bytes and parsed one
/// PDU from the start of the buffer, so the device evidently sends one PDU
/// per TCP segment. The `pduSize` field is not trustworthy (the host itself
/// sends a non-standard value), so we frame by *type*, using the fixed
/// lengths above, and resynchronise on the 0x3B sync byte if anything looks
/// wrong. This copes with PDUs that are split across reads or glued together.
#[derive(Default)]
pub struct Framer {
    buf: Vec<u8>,
}

impl Framer {
    pub fn new() -> Self {
        Self::default()
    }

    /// Feed bytes from the socket; returns every complete PDU found.
    pub fn push(&mut self, data: &[u8]) -> Vec<Incoming> {
        self.buf.extend_from_slice(data);
        let mut out = Vec::new();
        loop {
            // Drop garbage before the next sync byte.
            match self.buf.iter().position(|&b| b == SYNC) {
                Some(0) => {}
                Some(n) => {
                    self.buf.drain(..n);
                }
                None => {
                    self.buf.clear();
                    break;
                }
            }
            // Need at least the header to know the type.
            if self.buf.len() < 10 {
                break;
            }
            let pdu_type = self.buf[7];
            let need = match MessageType::from(pdu_type) {
                MessageType::Ntf => NOTIFICATION_LEN,
                MessageType::Rsp => RESPONSE_LEN,
                // Unknown/other: trust pduSize if sane, else skip the sync byte.
                _ => {
                    let size = u32::from_be_bytes(self.buf[3..7].try_into().unwrap()) as usize;
                    if (9..=512).contains(&size) {
                        size + 1
                    } else {
                        self.buf.drain(..1);
                        continue;
                    }
                }
            };
            if self.buf.len() < need {
                break; // wait for the rest
            }
            let pdu: Vec<u8> = self.buf.drain(..need).collect();
            out.push(decode_pdu(&pdu));
        }
        out
    }
}

/// Decode one complete PDU (already framed).
pub fn decode_pdu(b: &[u8]) -> Incoming {
    match MessageType::from(b[7]) {
        MessageType::Ntf if b.len() >= NOTIFICATION_LEN => {
            let mut params = [0u8; 8];
            params.copy_from_slice(&b[35..43]);
            Incoming::Notification(Notification {
                block: read_ono(&b[14..18]),
                ono: read_ono(&b[27..31]),
                params,
            })
        }
        MessageType::Rsp if b.len() >= RESPONSE_LEN => Incoming::Preset(PresetResponse {
            status: b[18],
            file_num: b[20],
            data: b[21..21 + PRESET_LEN].to_vec(),
        }),
        _ => Incoming::Other { pdu_type: b[7], bytes: b.to_vec() },
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// The CH1 gain command must match byte-for-byte what PreMo.exe sent
    /// (worked out by hand from Ocp1CommandPdu + CommandBLL.ByteMarshal).
    #[test]
    fn command_matches_legacy_layout() {
        let b = encode_command(16, 21, 40);
        assert_eq!(
            b,
            [
                0x3B, 0x00, 0x01, 0x00, 0x00, 0x00, 0x17, 0x00, 0x00, 0x01, // sync + header
                0x00, 0x00, 0x00, 0x12, // commandSize 18
                0x00, 0x00, 0x00, 0x10, // handle = block 16
                0x00, 0x00, 0x00, 0x15, // targetONo = 21
                0x00, 0x04, 0x00, 0x01, // methodID 4.1
                0x01, 40, // 1 parameter: 40
            ]
        );
        assert_eq!(decode_command(&b), Some(Command { block: 16, ono: 21, value: 40 }));
    }

    #[test]
    fn notification_round_trip_and_framing() {
        let n = encode_notification(176, 177, [0xF6, 0xFE, 1, 2, 3, 4, 5, 6]);
        let r = encode_preset_response(3, &[7u8; PRESET_LEN]);
        // Glue two PDUs together, add junk, split at an awkward point.
        let mut stream = vec![0x00, 0x11];
        stream.extend_from_slice(&n);
        stream.extend_from_slice(&r);
        let mut f = Framer::new();
        let mut got = f.push(&stream[..20]);
        got.extend(f.push(&stream[20..]));
        assert_eq!(got.len(), 2);
        assert_eq!(
            got[0],
            Incoming::Notification(Notification { block: 176, ono: 177, params: [0xF6, 0xFE, 1, 2, 3, 4, 5, 6] })
        );
        match &got[1] {
            Incoming::Preset(p) => {
                assert_eq!(p.file_num, 3);
                assert_eq!(p.data, vec![7u8; PRESET_LEN]);
            }
            other => panic!("expected preset, got {other:?}"),
        }
    }

    #[test]
    fn big_endian_onos_are_tolerated() {
        let mut n = encode_notification(16, 21, [5, 0, 0, 0, 0, 0, 0, 0]);
        n[14..18].copy_from_slice(&16u32.to_be_bytes());
        n[27..31].copy_from_slice(&21u32.to_be_bytes());
        match decode_pdu(&n) {
            Incoming::Notification(x) => assert_eq!((x.block, x.ono), (16, 21)),
            other => panic!("{other:?}"),
        }
    }
}
