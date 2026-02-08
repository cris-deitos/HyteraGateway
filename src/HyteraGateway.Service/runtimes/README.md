# Native Libraries Directory

This directory is for platform-specific native libraries required by HyteraGateway.

## mbelib - AMBE Audio Codec

HyteraGateway uses [mbelib](https://github.com/szechyjs/mbelib) for decoding DMR AMBE+2 audio.

### Directory Structure

```
runtimes/
├── win-x64/native/        # Windows x64 libraries
│   └── mbe.dll (or libmbe.dll)
├── linux-x64/native/      # Linux x64 libraries  
│   └── libmbe.so
└── osx-x64/native/        # macOS x64 libraries
    └── libmbe.dylib
```

### How to Add mbelib

1. **Build or obtain** the mbelib library for your platform
   - See [MBELIB_SETUP.md](../../docs/MBELIB_SETUP.md) for build instructions

2. **Copy the library** to the appropriate platform folder:
   - **Windows:** Copy `mbe.dll` or `libmbe.dll` to `runtimes/win-x64/native/`
   - **Linux:** Copy `libmbe.so` to `runtimes/linux-x64/native/`
   - **macOS:** Copy `libmbe.dylib` to `runtimes/osx-x64/native/`

3. **Rebuild the project** - the native libraries will be copied to the output directory automatically

### .NET Runtime Library Loading

The .NET runtime automatically searches these directories for platform-specific native libraries:

1. `runtimes/{rid}/native/` in the application directory
2. System library paths (`/usr/local/lib`, `C:\Windows\System32`, etc.)

Where `{rid}` is the Runtime Identifier (e.g., `win-x64`, `linux-x64`, `osx-x64`)

### Alternative: System-Wide Installation

Instead of including libraries in the `runtimes/` folder, you can install mbelib system-wide:

- **Linux/macOS:** Run `sudo make install` after building mbelib
- **Windows:** Copy `mbe.dll` to the same directory as `HyteraGateway.Service.exe`

See [MBELIB_SETUP.md](../../docs/MBELIB_SETUP.md) for detailed instructions.

### Graceful Fallback

HyteraGateway works without mbelib installed:

- If mbelib is found → DMR audio is decoded and recordings have sound
- If mbelib is not found → Service still runs, but audio recordings are silent

A warning is logged if mbelib is not available.

### Library Verification

To verify libraries are correctly placed:

```bash
# Windows (PowerShell)
Get-ChildItem -Recurse runtimes\*\native\*.dll

# Linux/macOS
find runtimes -name "*.so" -o -name "*.dylib"
```

### Notes

- The `.gitkeep` files in empty folders ensure the directory structure is preserved in git
- Native libraries are not included in the repository due to size and licensing
- Each deployment needs to include the appropriate native library for the target platform
- Libraries must match the architecture of your .NET runtime (x64 for most modern systems)

### Getting Help

For build instructions and troubleshooting:
- See [MBELIB_SETUP.md](../../docs/MBELIB_SETUP.md)
- Check project logs for native library loading errors
- Verify library architecture matches your .NET runtime
