# mbelib Setup Guide

This guide explains how to install and configure mbelib for AMBE audio decoding in HyteraGateway.

## What is mbelib?

mbelib is an open-source library that implements an approximate AMBE (Advanced Multi-Band Excitation) decoder. It provides ~80-90% audio quality compared to the proprietary DVSI codec.

**Repository:** https://github.com/szechyjs/mbelib

**License:** ISC License (permissive, GPL-compatible)

**Note:** mbelib is a decode-only library. For encoding AMBE audio, you need DVSI hardware or a licensed SDK.

## Graceful Fallback

HyteraGateway is designed to work with or without mbelib:

- **With mbelib:** DMR audio is decoded and can be heard in recordings
- **Without mbelib:** Service starts normally, but audio recordings contain silence

If mbelib is not available, you'll see a warning in the logs:
```
mbelib not found, falling back to placeholder codec (audio will be silent). 
To enable audio decoding, install mbelib. See docs/MBELIB_SETUP.md for instructions.
```

## Installation

### Linux (Debian/Ubuntu)

```bash
# Install build dependencies
sudo apt-get update
sudo apt-get install -y git cmake build-essential

# Clone and build mbelib
git clone https://github.com/szechyjs/mbelib.git
cd mbelib
mkdir build && cd build
cmake ..
make

# Install system-wide
sudo make install

# Update library cache
sudo ldconfig

# Verify installation
ldconfig -p | grep libmbe
```

The library will be installed to `/usr/local/lib/libmbe.so`.

### Linux (Other Distributions)

For Fedora/RHEL/CentOS:
```bash
sudo dnf install -y git cmake gcc-c++
# Then follow the same build steps as Ubuntu
```

For Arch Linux:
```bash
sudo pacman -S git cmake base-devel
# Then follow the same build steps as Ubuntu
```

### macOS

```bash
# Install Homebrew if not already installed
# /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install build dependencies
brew install cmake

# Clone and build mbelib
git clone https://github.com/szechyjs/mbelib.git
cd mbelib
mkdir build && cd build
cmake ..
make

# Install system-wide
sudo make install
```

The library will be installed to `/usr/local/lib/libmbe.dylib`.

### Windows

#### Option 1: Build with MSYS2/MinGW (Recommended)

1. **Install MSYS2** from https://www.msys2.org/

2. **Open MSYS2 MinGW64 terminal** and install dependencies:
```bash
pacman -S --needed base-devel mingw-w64-x86_64-toolchain mingw-w64-x86_64-cmake git
```

3. **Build mbelib:**
```bash
git clone https://github.com/szechyjs/mbelib.git
cd mbelib
mkdir build && cd build
cmake .. -G "MinGW Makefiles"
mingw32-make
```

4. **Copy the DLL:**
```bash
# Find the built DLL (usually libmbe.dll or mbe.dll)
cp libmbe.dll /path/to/HyteraGateway/Service/
```

#### Option 2: Build with Visual Studio

1. **Install Visual Studio** with C++ development tools

2. **Install CMake** from https://cmake.org/download/

3. **Build mbelib:**
```cmd
git clone https://github.com/szechyjs/mbelib.git
cd mbelib
mkdir build
cd build
cmake .. -G "Visual Studio 17 2022" -A x64
cmake --build . --config Release
```

4. **Copy the DLL:**
```cmd
copy Release\mbe.dll C:\path\to\HyteraGateway\Service\
```

#### Option 3: Use Pre-compiled Binaries

Some community members may provide pre-compiled Windows binaries. If using these:

1. Download `libmbe.dll` (or `mbe.dll`)
2. Copy to the HyteraGateway.Service executable directory
3. Verify the DLL architecture matches (x64/x86)

## Deployment with HyteraGateway

### Runtime-Specific Libraries

To deploy mbelib with your application, place the native library in the appropriate runtime folder:

```
HyteraGateway.Service/
├── runtimes/
│   ├── win-x64/native/
│   │   └── mbe.dll (or libmbe.dll)
│   ├── linux-x64/native/
│   │   └── libmbe.so
│   └── osx-x64/native/
│       └── libmbe.dylib
```

The .NET runtime will automatically load the correct library for your platform.

### Docker Deployment

If deploying with Docker, add the mbelib installation to your Dockerfile:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Install mbelib
RUN apt-get update && apt-get install -y git cmake build-essential \
    && git clone https://github.com/szechyjs/mbelib.git /tmp/mbelib \
    && cd /tmp/mbelib \
    && mkdir build && cd build \
    && cmake .. && make && make install \
    && ldconfig \
    && rm -rf /tmp/mbelib \
    && apt-get remove -y git cmake build-essential \
    && apt-get autoremove -y

# Copy application files
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HyteraGateway.Service.dll"]
```

## Verification

To verify mbelib is working:

1. **Start HyteraGateway Service**

2. **Check logs for:**
```
Using MbelibAmbeCodec for AMBE audio decoding
MbelibAmbeCodec initialized (GPL licensed)
```

3. **Make a test DMR call** and check the recording has audio (not silent)

4. **If you see the fallback warning,** mbelib was not found on the system path

## Troubleshooting

### Library Not Found (Linux/macOS)

**Error:** `DllNotFoundException: Unable to load shared library 'mbe'`

**Solution:**
```bash
# Check if libmbe is installed
ldconfig -p | grep libmbe  # Linux
ls -la /usr/local/lib/libmbe* # Linux/macOS

# If not found, ensure you ran 'sudo make install' and 'sudo ldconfig'

# Check library path
echo $LD_LIBRARY_PATH  # Linux
echo $DYLD_LIBRARY_PATH  # macOS

# Add /usr/local/lib to library path if needed
export LD_LIBRARY_PATH=/usr/local/lib:$LD_LIBRARY_PATH  # Linux
export DYLD_LIBRARY_PATH=/usr/local/lib:$DYLD_LIBRARY_PATH  # macOS
```

### Library Not Found (Windows)

**Error:** `DllNotFoundException: Unable to load shared library 'mbe'`

**Solution:**
1. Verify `mbe.dll` or `libmbe.dll` is in the same directory as `HyteraGateway.Service.exe`
2. Verify the DLL architecture matches (x64 for 64-bit .NET apps)
3. Install Visual C++ Redistributables if needed
4. Use [Dependencies.exe](https://github.com/lucasg/Dependencies) to check DLL dependencies

### Wrong Architecture

**Error:** `BadImageFormatException` or similar

**Solution:** Rebuild mbelib for the correct architecture (x64 vs x86) matching your .NET runtime

### Audio Still Silent

If mbelib loads but audio is still silent:

1. **Check frame size:** Hytera AMBE frames should be 33 bytes
2. **Check logs:** Look for AMBE decode errors
3. **Verify DMR transmission:** Ensure radio is actually transmitting voice (not data/signaling)
4. **Frame structure:** The AMBE data extraction in `MbelibAmbeCodec.ExtractAmbeData()` may need adjustment for your specific Hytera model

## Legal Note

mbelib is licensed under the ISC License (permissive, GPL-compatible). HyteraGateway includes GPL-licensed code when using mbelib.

AMBE and AMBE+2 are proprietary codecs owned by Digital Voice Systems, Inc. (DVSI). mbelib is a reverse-engineered implementation and may have patent implications depending on your jurisdiction and use case. For commercial use, consider obtaining proper licensing from DVSI.

## Performance

mbelib decoding is relatively lightweight:
- ~0.5-1ms per frame on modern CPUs
- Minimal memory overhead
- No GPU acceleration required
- Can handle multiple concurrent streams

## Alternative: DVSI Hardware

For higher quality audio and legal certainty:

- **DVSI USB-3000:** USB dongle with hardware AMBE codec
- **DVSI AMBE-3003:** Chip-level integration
- **Licensed SDK:** Software encoder/decoder from DVSI

Contact DVSI at https://www.dvsinc.com/ for licensing information.

## Additional Resources

- mbelib GitHub: https://github.com/szechyjs/mbelib
- AMBE Specification: DVSI, Inc. proprietary
- DMR Standard: ETSI TS 102 361
- HyteraGateway Documentation: See docs/ folder

## Support

If you continue to have issues:

1. Check HyteraGateway logs for detailed error messages
2. Verify mbelib version compatibility
3. Open an issue on the HyteraGateway GitHub repository with:
   - Operating system and version
   - mbelib version
   - Full error logs
   - Steps to reproduce
