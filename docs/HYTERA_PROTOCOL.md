# Hytera REAL Protocol Implementation

## Overview

This implementation integrates the Hytera REAL protocol using the vendor DLL libraries and XML configurations. The implementation provides a foundation for working with Hytera radios using their native protocol instead of custom implementations.

## Key Components

### 1. Configuration Management

#### HyteraProtocolConfig.cs
- Parses `Hytera_HyteraProtocol.xml` configuration file
- Provides access to audio settings, RTP parameters, and protocol configuration
- **Important**: Indicates that audio is in PCM format (no AMBE codec needed)

```csharp
var config = HyteraProtocolConfigLoader.LoadOrDefault("lib/vendor/Hytera_HyteraProtocol.xml");
if (config.IsPcmAudio)
{
    // Audio is PCM - no decoding needed
}
```

#### RadioControllerConfig.cs
- Parses `RadioController.xml` configuration file
- Provides UDP service port definitions (RRS, LP, TMP, RCP, TP, DTP, SDMP)
- Defines enabled RX events and TX commands
- Manages radio and slot configurations

```csharp
var config = RadioControllerConfigLoader.Load("lib/vendor/RadioController.xml");
foreach (var protocol in config.Protocols)
{
    foreach (var service in protocol.UDPNetworkServices)
    {
        // Access UDP service ports (3002-3009)
    }
}
```

### 2. Vendor DLL Wrapper

#### VendorDllWrapper.cs
- Dynamically loads vendor DLLs from `lib/vendor/` directory
- Uses reflection to access DLL types and methods
- Provides graceful fallback if DLLs are not available
- Supports:
  - HyteraProtocol.dll
  - BPGRadioController.dll
  - Alvas.Audio.dll
  - Other vendor libraries

```csharp
var wrapper = new VendorDllWrapper("lib/vendor/");
if (wrapper.LoadDlls())
{
    // DLLs loaded successfully
    // Can now use vendor protocol implementation
}
```

### 3. Protocol Constants

#### HyteraUdpPorts.cs
- Defines UDP service ports (3002-3009)
- Provides event type constants (RX_CALL, TX_GPS_QUERY, etc.)

```csharp
// Use port constants
int rrsPort = HyteraUdpPorts.RRS;  // 3002 - Radio Registration Service
int lpPort = HyteraUdpPorts.LP;    // 3003 - Location Protocol (GPS)
int tmpPort = HyteraUdpPorts.TMP;  // 3004 - Text Message Protocol

// Use event constants
string callEvent = HyteraEventType.RX_CALL;
string gpsQuery = HyteraEventType.TX_GPS_QUERY;
```

## UDP Service Ports

| Port | Service | Purpose |
|------|---------|---------|
| 3002 | RRS | Radio Registration Service - Keepalive and registration |
| 3003 | LP | Location Protocol - GPS data |
| 3004 | TMP | Text Message Protocol - Text messages |
| 3005 | RCP | Radio Control Protocol - PTT and control commands |
| 3006 | TP | Telemetry Protocol - Telemetry data |
| 3007 | DTP | Data Transfer Protocol - Data transfer |
| 3009 | SDMP | Status/Display Message Protocol - Status messages |

## Audio Format

**Critical**: The `RTPAudioOutputType` in the XML configuration is set to **PCM**.

This means:
- ✅ Audio is already decoded as PCM samples
- ✅ No AMBE/AMBE+ codec is required
- ✅ No mbelib dependency needed
- ✅ Can use standard PCM audio processing

Audio specifications:
- Format: 16-bit Linear PCM
- Sample Rate: 8 kHz
- Channels: Mono
- Rx Timeout: 55ms
- Tx Timeout: 55ms

## Project Structure

```
src/HyteraGateway.Radio/
├── Configuration/
│   ├── HyteraProtocolConfig.cs     # Hytera_HyteraProtocol.xml parser
│   └── RadioControllerConfig.cs    # RadioController.xml parser
├── Protocol/Hytera/
│   ├── HyteraUdpPorts.cs          # Port and event constants
│   ├── HyteraConnection.cs        # Existing TCP connection (IPSC)
│   └── HyteraIPSCPacket.cs        # Existing IPSC packet handling
├── Vendor/
│   └── VendorDllWrapper.cs        # DLL loading and reflection wrapper
└── Services/
    └── HyteraConnectionService.cs  # Main service (to be updated)

lib/vendor/
├── HyteraProtocol.dll              # Main protocol DLL
├── BPGRadioController.dll          # Radio controller
├── Alvas.Audio.dll                 # Audio handling
├── RadioController.xml             # Port and service configuration
└── Hytera_HyteraProtocol.xml      # Protocol settings
```

## Testing

All configuration tests pass (29/29):
- ✅ HyteraProtocolConfig loading and defaults
- ✅ RadioControllerConfig loading and defaults
- ✅ UDP service configuration
- ✅ PCM audio format detection
- ✅ XML serialization/deserialization

## Next Steps

To complete the integration:

1. **Update HyteraConnectionService**:
   - Add UDP listeners for ports 3002-3009
   - Route events to appropriate handlers
   - Use vendor DLL wrapper for protocol operations

2. **Implement Event Handlers**:
   - RX_CALL, RX_GPS_SENTENCE handlers
   - TX_VOICE_CALL, TX_GPS_QUERY handlers
   - Status and telemetry handlers

3. **Audio Integration**:
   - Use Alvas.Audio.dll or NAudio for PCM audio
   - Remove AMBE codec dependencies if present
   - Configure RTP audio streaming

4. **Test with Real Hardware**:
   - Connect to Hytera repeater/radio
   - Verify all UDP services
   - Test audio quality

## Build and Run

```bash
# Build the project
dotnet build src/HyteraGateway.Radio/HyteraGateway.Radio.csproj

# Run tests
dotnet test tests/HyteraGateway.Radio.Tests/

# Configuration files are copied to output directory automatically
# Look for them in bin/Debug/net8.0/vendor/
```

## Documentation

- [Audio Format Details](AUDIO_FORMAT.md)
- [UDP Protocol Details](UDP_PROTOCOL.md)

## Compatibility Notes

- Vendor DLLs are .NET Framework 4.0
- Current project targets .NET 8.0
- Runtime should provide compatibility layer
- Graceful fallback implemented if DLLs fail to load
- XML configuration uses capital-case booleans ("False"/"True") which is handled gracefully

## License

Vendor DLLs are proprietary Hytera software. Ensure proper licensing before deployment.
