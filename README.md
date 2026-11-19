# Premote.WBSps.ca

PreMO remote software: the legacy Windows tools, their recovered source, and a new Rust host application.

| Folder | What |
|---|---|
| [`2026/`](2026/) | **premote**: the Rust host. Discovers PreMO units, controls them over AES70/OCP.1, updates firmware, and hosts an HTML control page per device. Start with [`2026/README.md`](2026/README.md); the protocol is in [`2026/docs/PROTOCOL.md`](2026/docs/PROTOCOL.md). |
| `preMO remote-20261119T130500Z-1-001/` | The legacy software as received, and the C# recovered from it (below). |

## Legacy software

Legacy PreMO remote software, kept as received in `preMO remote-20261119T130500Z-1-001/` (Google Drive export, 2026-11-19). Compiled files only; no source code was included.

| File | Bytes | SHA-256 |
|---|---|---|
| `preMO remote-20261119T130500Z-1-001/preMO remote/Premo2,19/APP2.bin` | 129484 | `f79ef9ec44f3f3d66651e2b9ddb777fa488a44df68a45d15ae0d799b2ce873ed` |
| `preMO remote-20261119T130500Z-1-001/preMO remote/Premo2,19/Remote1.22.0/Common.dll` | 4096 | `d60f7b2ac29c258a07b97c82897b77abce6a52207b22bfe49cd742d9fedc8530` |
| `preMO remote-20261119T130500Z-1-001/preMO remote/Premo2,19/Remote1.22.0/DNSService.dll` | 4096 | `dec1fa7948ca6ff04ab8c0fc6982f8edc35070ccc89b7686302d1a602c1e715f` |
| `preMO remote-20261119T130500Z-1-001/preMO remote/Premo2,19/Remote1.22.0/Interop.Bonjour.dll` | 36864 | `fcb9477ec228d4fb63ae10da337b46f9d2a6a5edbdb3bf8e5e48f0b8f7ff7d4a` |
| `preMO remote-20261119T130500Z-1-001/preMO remote/Premo2,19/Remote1.22.0/Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll` | 98448 | `e65df22a4538f0d00d52ab6891f9b5e6b275f85f7638354dc3954366161f7383` |
| `preMO remote-20261119T130500Z-1-001/preMO remote/Premo2,19/Remote1.22.0/Model.dll` | 16384 | `7bd1f8ac58dad43797cad883183c58bc8b782d9f2be200103ae7f8dc9fa5df7b` |
| `preMO remote-20261119T130500Z-1-001/preMO remote/Premo2,19/Remote1.22.0/Newtonsoft.Json.dll` | 662528 | `f58c374ffcaae4e36d740d90fbf7fe70d0abb7328cd9af3a0a7b70803e994ba4` |
| `preMO remote-20261119T130500Z-1-001/preMO remote/Premo2,19/Remote1.22.0/OCAAes70.dll` | 12800 | `8839504abe12a63f0a00534cf60e6e9d83415edbf12ca3fee8a00bc62fba9e04` |
| `preMO remote-20261119T130500Z-1-001/preMO remote/Premo2,19/Remote1.22.0/PreMoBLL.dll` | 29184 | `4bb5fed8b0cf3d7427f12e73362b462f7c3bfd216dbc6f236e5fcfa6abdbb668` |
| `preMO remote-20261119T130500Z-1-001/preMO remote/Premo2,19/Remote1.22.0/PreMoDAL.dll` | 12800 | `babfe308e07543cf2dc7f251d748b9ae3eed3c38d22b6c3ec3d1412bb5468a03` |
| `preMO remote-20261119T130500Z-1-001/preMO remote/Premo2,19/Remote1.22.0/PreMo.exe` | 731136 | `6f783fbe6a87ac6f84d26d99ee92b5fe9030bc462686a37a5a30b8fb6b15e276` |
| `preMO remote-20261119T130500Z-1-001/preMO remote/Premo2,19/TCPRemote.exe` | 19456 | `00379c3ff44fc9587de419b6ddf814a7984e381a638ed66e9eeb350b73704c8d` |
| `preMO remote-20261119T130500Z-1-001/preMO remote/PreMO manual draft.pdf` | 11939102 | `2f7c4a5c49878518cec3bf5c5c22608d41a77644333f5c4282d8c3ba463a39cd` |

- `Premo2,19/Remote1.22.0/PreMo.exe`: PreMO Remote 1.22.0, a Windows .NET (x86) app, with its libraries (PreMoBLL, PreMoDAL, Model, Common, OCAAes70, DNSService / Interop.Bonjour, Newtonsoft.Json)
- `Premo2,19/TCPRemote.exe`: the "PreMoUpdate" firmware updater (.NET): enter the unit's IP, READFILE picks a `.bin` (e.g. `APP2.bin`), UPDATE sends it over TCP, REBOOT restarts the unit
- `Premo2,19/APP2.bin`: firmware image
- `PreMO manual draft.pdf`: draft manual

## Recovered source (`preMO remote-20261119T130500Z-1-001/decompiled/`)

C# recovered from the .NET binaries above with ILSpy (`ilspycmd` 11.2), one project per program or library, all in `preMO remote-20261119T130500Z-1-001/decompiled/PreMoRemote.sln`. The binaries above are the originals; this folder is a readable, rebuildable reconstruction, not the original source code: comments, original local-variable names and Visual Studio designer files are lost.

| Project | Built from | Kind |
|---|---|---|
| `PreMo` | `Remote1.22.0/PreMo.exe` | Windows Forms app: login, home screen, meters (PPMMeter, MICLINE), user management, update |
| `PreMoBLL` | `PreMoBLL.dll` | Business logic: device discovery over Bonjour, device control |
| `PreMoDAL` | `PreMoDAL.dll` | Data access |
| `Model` | `Model.dll` | Data model, AES70 and JSON types |
| `OCAAes70` | `OCAAes70.dll` | AES70 (OCA) control classes and the OCP.1 protocol |
| `Common`, `DNSService` | `Common.dll`, `DNSService.dll` | Small helpers |
| `TCPRemote` | `Premo2,19/TCPRemote.exe` | "PreMoUpdate": Windows Forms firmware updater over TCP |

Not decompiled (used as references from the original folder): `Newtonsoft.Json.dll`, `Interop.Bonjour.dll` (the Bonjour COM wrapper, needs Apple Bonjour installed to run) and `Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll`.

Target: .NET Framework 4.5.2. Corrections made to the decompiler output so it compiles:
- project references between the recovered projects instead of the original DLLs
- the embedded Bonjour interop types replaced by a reference to `Interop.Bonjour.dll`
- base-class calls the decompiler wrote as `((Control)this).X(...)` written as `base.X(...)`
- `Timer` and `ManagementObjectEnumerator` written with their full type names

Build status: every project compiles. On Windows (Visual Studio or `msbuild`), the whole solution builds. With the .NET SDK on Linux, everything except the final `PreMo` resource step builds; its forms embed images, which only the Windows build tools pack.
