# DMR Protocol Overview

## ETSI TS 102 361 Standard

Digital Mobile Radio (DMR) is an open digital radio standard developed by the European Telecommunications Standards Institute (ETSI) under specification TS 102 361.

## Key Features

### TDMA Dual Slot Architecture

DMR uses Time Division Multiple Access (TDMA) to provide two logical channels (slots) on a single 12.5 kHz frequency channel:

- **Slot 1**: Timeslot 0 (first 30ms of 60ms frame)
- **Slot 2**: Timeslot 1 (second 30ms of 60ms frame)
- **Frame Duration**: 60ms total
- **Voice Frame Rate**: 16.67 frames per second (per slot)

### AMBE+2 Codec

DMR uses the proprietary AMBE+2 (Advanced Multi-Band Excitation) vocoder:

- **Voice Bitrate**: 2450 bps
- **FEC (Forward Error Correction)**: 3150 bps
- **Total Bitrate**: 5600 bps per slot
- **Frame Size**: 33 bytes (264 bits)
- **Sample Rate**: 8000 Hz
- **Frame Duration**: 60ms
- **Samples per Frame**: 480

### Color Codes

DMR uses color codes (0-15) to prevent interference between nearby repeaters on the same frequency:

- **Purpose**: Network identification and interference rejection
- **Range**: 0-15 (4 bits)
- **Usage**: Radios ignore transmissions with different color codes

## Packet Structure

### Voice/Data Frame (Simplified)

```
Offset | Bytes | Field        | Description
-------|-------|--------------|------------------------------------------
0x00   | 6     | Sync         | Sync pattern (identifies slot and type)
0x06   | 1     | Slot         | Timeslot (0 = Slot 1, 1 = Slot 2)
0x07   | 1     | CC + Type    | Color Code (4 bits) + Packet Type (4 bits)
0x08   | 4     | Source ID    | Source DMR ID (32-bit, little-endian)
0x0C   | 4     | Dest ID      | Destination ID (32-bit, little-endian)
0x10   | 1     | Call Type    | 0=Private, 1=Group, 2=All
0x11   | 2     | Sequence     | Packet sequence number (little-endian)
0x13   | N     | Payload      | Voice/data payload
...    | 2     | CRC          | CRC-CCITT checksum
```

### Sync Patterns

Different sync patterns identify the slot and frame type:

- **Voice Slot 1**: `0x77 0x55 0xFD 0x7D 0xF7 0x5F`
- **Voice Slot 2**: `0xFF 0x57 0xD7 0x5D 0xF5 0xD5`
- **Data Slot 1**: `0xD5 0xDD 0x57 0xDF 0xD7 0x57`
- **Data Slot 2**: `0x77 0xD5 0x5F 0x7D 0xFF 0x77`

## Call Types

### Group Call (Talkgroup)

- **Most Common**: Used for dispatcher-to-group communications
- **Destination ID**: Talkgroup ID (typically 1-999999)
- **Example**: Dispatcher broadcasts to all units on Talkgroup 100

### Private Call

- **One-to-One**: Direct communication between two radios
- **Destination ID**: Target radio's DMR ID
- **Example**: Dispatcher calls specific unit 123456

### All Call

- **Broadcast**: Transmitted to all radios regardless of talkgroup
- **Destination ID**: Usually 16777215 (0xFFFFFF)
- **Use Case**: Emergency notifications

## DMR IDs

- **Format**: 32-bit unsigned integer
- **Range**: 1 to 16777215 (0x00000001 to 0x00FFFFFF)
- **Assignment**: Globally unique, managed by radio-id.net
- **Structure**:
  - Country Code: 3 digits (e.g., 310 for USA)
  - User/Radio ID: Up to 7 total digits

## CRC Validation

DMR packets use CRC-CCITT for error detection:

- **Polynomial**: 0x1021
- **Initial Value**: 0xFFFF
- **Width**: 16 bits
- **Position**: Last 2 bytes of packet (little-endian)

## Packet Types

| Type | Value | Description |
|------|-------|-------------|
| Voice | 0 | AMBE+2 encoded voice frame |
| Data | 1 | User data payload |
| CSBK | 2 | Control Signaling Block |
| Rate 1/2 Data | 3 | Lower rate data with more FEC |
| Rate 3/4 Data | 4 | Higher rate data with less FEC |
| Idle | 5 | Channel idle/filler |

## Voice Superframe Structure

Voice transmissions are organized into superframes:

- **Superframe**: 6 voice frames (A-F)
- **Frame A**: Contains embedded signaling (LC data)
- **Frame B-F**: Pure voice data
- **Duration**: 360ms (6 × 60ms)

## Implementation Notes

1. **Byte Order**: All multi-byte integers are little-endian
2. **CRC Calculation**: Computed over entire packet except CRC bytes
3. **Sync Detection**: Critical for slot identification and timing
4. **Color Code**: Must match repeater/network configuration
5. **Frame Timing**: Strict 60ms timing required for TDMA operation

## References

- ETSI TS 102 361-1: Air Interface Protocol
- ETSI TS 102 361-2: Voice and Generic Services
- ETSI TS 102 361-3: Data Protocol
- ETSI TS 102 361-4: Trunking Protocol

## See Also

- [Hytera IPSC Protocol](HYTERA_IPSC.md)
- [Packet Reference](PACKET_REFERENCE.md)
- [Implementation Guide](../IMPLEMENTATION_GUIDE.md)
