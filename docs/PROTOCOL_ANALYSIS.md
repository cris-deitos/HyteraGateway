# DMR Protocol Analysis for Hytera MD785i

## Overview

This document contains notes and findings from analyzing the Hytera MD785i DMR protocol for implementing the radio communication layer.

## Status: TO BE COMPLETED

The protocol implementation is currently in placeholder state. This document will be updated as the protocol is reverse-engineered from HyteraProtocol.dll or through packet analysis.

## DMR Basics

**DMR (Digital Mobile Radio)** is a digital radio standard developed by ETSI.

- **Timeslot TDMA**: Two timeslots on a single 12.5 kHz channel
- **Codec**: AMBE+2 vocoder for voice encoding
- **Data**: Supports embedded data, text messaging, GPS
- **Call Types**: Group call, private call, all call, emergency

## Hytera MD785i Specifics

**Model**: MD785i (mobile transceiver)
**Connection**: USB NCM (Network Control Model)
**IP Address**: Typically 192.168.x.x (auto-assigned or configured)
**Ports**:
- Control: 50000 (TCP/UDP - TBD)
- Audio: 50001 (UDP - TBD)

## USB NCM Connection

The Hytera MD785i presents itself as a network adapter via USB NCM:

1. **Device Detection**: 
   - USB VID/PID: [TO BE DETERMINED]
   - Interface: CDC NCM class

2. **Network Configuration**:
   - DHCP client on radio side
   - Static IP or DHCP server on host side
   - Default subnet: 192.168.x.0/24

3. **Connection Establishment**:
   - [TO BE DOCUMENTED after DLL analysis]

## Protocol Packets

### Packet Structure (Preliminary)

```
[Header][Type][Length][Sequence][Data][Checksum]
```

### Packet Types

Based on `PacketType` enum in our implementation:

- `Unknown = 0` - Default/unidentified
- `Control = 1` - Control commands
- `Voice = 2` - Audio/voice data (AMBE+2)
- `Data = 3` - Generic data
- `Gps = 4` - GPS position reports
- `TextMessage = 5` - Short text messages
- `Emergency = 6` - Emergency alerts
- `Status = 7` - Radio status updates
- `Ptt = 8` - PTT control

### Control Packets

**PTT Command**:
```
[TO BE DOCUMENTED]
- Press PTT: [byte sequence]
- Release PTT: [byte sequence]
```

**GPS Request**:
```
[TO BE DOCUMENTED]
```

**Text Message**:
```
[TO BE DOCUMENTED]
- Format: DMR short data or proprietary
- Max length: [TBD]
- Encoding: [TBD]
```

### Audio Packets

**AMBE+2 Voice Frames**:
```
[TO BE DOCUMENTED]
- Frame size: [TBD] bytes
- Sample rate: 8000 Hz
- Frame duration: 60ms
```

**Audio Stream**:
- Transport: UDP on port 50001
- Format: AMBE+2 encoded frames
- Decoding: Requires AMBE vocoder (hardware or licensed software)

### GPS Packets

**Position Report Format**:
```
[TO BE DOCUMENTED]
- Latitude: [format TBD]
- Longitude: [format TBD]
- Altitude: [format TBD]
- Speed/Heading: [format TBD]
```

## Reverse Engineering Tools

### DllAnalyzer
Use the included `DllAnalyzer` tool to inspect HyteraProtocol.dll:

```bash
cd tools/DllAnalyzer
dotnet run -- /path/to/HyteraProtocol.dll > analysis.txt
```

### Wireshark
Capture USB NCM traffic:
- Filter: `ip.dst == 192.168.x.x || ip.src == 192.168.x.x`
- Protocol: TCP/UDP on ports 50000-50001

### Network Monitor
For Windows-specific USB network analysis.

## Implementation Notes

### Current Placeholder Status

All protocol-specific code is marked with:
```csharp
// TODO: Reverse-engineer from HyteraProtocol.dll
```

### Priority Implementation Order

1. **Connection Handshake** - Establish USB NCM connection
2. **Status Query** - Get radio status and DMR ID
3. **PTT Control** - Basic push-to-talk functionality
4. **Audio Stream** - Receive and decode voice
5. **GPS Request** - Get position data
6. **Text Messaging** - Send/receive SMS
7. **Emergency Handling** - Process emergency alerts

### Required Libraries

- **AMBE Vocoder**: 
  - Option 1: Hardware AMBE chip via USB
  - Option 2: DVSI software codec license
  - Option 3: Open-source vocoder approximation (lower quality)

### Testing Strategy

1. **Unit Tests**: Mock protocol packets and verify parsing
2. **Integration Tests**: Real radio connection in test environment
3. **Packet Validation**: Compare with known-good implementations

## Security Considerations

- **Authentication**: [TO BE DOCUMENTED]
- **Encryption**: DMR supports AES encryption (check if enabled)
- **Access Control**: Verify DMR ID and authorization

## References

- ETSI TS 102 361: DMR Air Interface Specification
- Hytera MD785i User Manual
- USB NCM Specification
- AMBE+2 Vocoder Documentation

## Next Steps

1. Obtain HyteraProtocol.dll for analysis
2. Run DllAnalyzer to extract type/method information
3. Capture network traffic during radio operations
4. Document packet formats and sequences
5. Implement protocol layer incrementally
6. Test with actual hardware

---

**Last Updated**: [TO BE UPDATED]
**Status**: Initial placeholder - awaiting protocol analysis
