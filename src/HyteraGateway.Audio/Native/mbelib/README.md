# mbelib - AMBE Codec Library

## License
mbelib is licensed under GPL. This means HyteraGateway must also be GPL if distributed.

## Building mbelib

### Windows (MSVC)
```bash
git clone https://github.com/szechyjs/mbelib.git
cd mbelib
mkdir build && cd build
cmake .. -G "Visual Studio 17 2022" -A x64
cmake --build . --config Release
# Output: mbelib.dll
```

### Linux (GCC)
```bash
git clone https://github.com/szechyjs/mbelib.git
cd mbelib
mkdir build && cd build
cmake ..
make
# Output: libmbe.so
```

## Precompiled Binaries
For convenience, precompiled binaries are included in this directory.
Source: https://github.com/szechyjs/mbelib

## Usage Notes
- Place `mbelib.dll` in `win-x64/` for Windows
- Place `libmbe.so` in `linux-x64/` for Linux
- The library will be loaded automatically at runtime via P/Invoke
