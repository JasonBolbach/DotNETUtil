# DotNETUtil

Small .NET utility libraries (VB.NET, `net10.0`).

| Project | What it is |
|---------|------------|
| **Assist** | General helpers: compression (GZip/Deflate), crypto wrappers (`CA` - RSA/DSA/AES/GZip/Deflate), `StreamTranslator` (copy through compress/encrypt stages with hashing and progress), queues, timers, OS helpers. |
| **Message** | `Message`: a tree of ID/Value nodes. Binary format is gzip'd, DSA-signed and AES-GCM encrypted; text format is JSON, optionally in a DSA-signed envelope. Depends on Assist. |
| **NA** | Networking helpers: `TCPServer`, `TCPSession`, buffer manager, synchronized queue/stack. |

## Notes

- **Placeholder keys.** The AES key and DSA key built into `Message` are placeholders: treat messages that use them as *unencrypted and unsigned*. Pass your own `Key()` / `DSACryptoServiceProvider` / `CA.DSA` for real use.
- **Windows.** `DSACryptoServiceProvider` is Windows-only on modern .NET.
- **Not compiled.** The WinForms/System.Drawing helpers in `Assist` (`DataGridView`, `Demo`, `Drawing`, `Invoke`) are kept on disk but excluded from the `net10.0` build.
- **Namespace.** `Message` has root namespace `Message`, so the class is `Message.Message`. Consumers can alias it (`Imports Msg = Message.Message`) to avoid the namespace/class name clash.

## Build

```
dotnet build DotNETUtil.sln
```
