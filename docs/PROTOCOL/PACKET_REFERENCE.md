# Packet Reference Guide

Complete byte-level specifications for all packet types in the Hytera Gateway protocol implementation.

## Table of Contents

1. [Voice Packets](#voice-packets)
2. [GPS Packets](#gps-packets)
3. [Text Message Packets](#text-message-packets)
4. [Control Packets](#control-packets)
5. [Emergency Packets](#emergency-packets)

---

## Voice Packets

### VOICE_FRAME (0x8080)

Complete voice frame with AMBE+2 encoded audio data.

#### Packet Layout

```
Offset | Size | Field          | Type      | Description
-------|------|----------------|-----------|--------------------------------
0x00   | 2    | Signature      | uint16 BE | "PH" (0x5048)
0x02   | 4    | Sequence       | uint32 LE | Packet sequence number
0x06   | 2    | Length         | uint16 LE | Total packet length (typically 56)
0x08   | 2    | Command        | uint16 LE | 0x8080 (VOICE_FRAME)
0x0A   | 1    | Slot           | uint8     | Timeslot (0 or 1)
0x0B   | 4    | Source ID      | uint32 LE | Transmitting radio DMR ID
0x0F   | 4    | Dest ID        | uint32 LE | Destination talkgroup/radio ID
0x13   | 33   | AMBE Data      | bytes[33] | AMBE+2 encoded voice (33 bytes)
0x34   | 1    | Frame Number   | uint8     | Sequence in superframe (0-5)
0x35   | 1    | Last Frame     | uint8     | 1=last frame, 0=more frames
0x36   | 2    | CRC            | uint16 LE | CRC-CCITT checksum
```

#### Example (Hex Dump)

```
50 48                   // Signature "PH"
01 00 00 00             // Sequence: 1
38 00                   // Length: 56 bytes
80 80                   // Command: VOICE_FRAME
00                      // Slot: 0 (Slot 1)
39 30 00 00             // Source ID: 12345
64 00 00 00             // Dest ID: 100 (talkgroup)
[33 bytes AMBE data]    // Voice data
03                      // Frame number: 3
00                      // Not last frame
XX XX                   // CRC
```

### VOICE_HEADER (0x8081)

Call initiation header sent before voice frames.

```
Offset | Size | Field          | Type      | Description
-------|------|----------------|-----------|--------------------------------
0x00   | 2    | Signature      | uint16 BE | "PH" (0x5048)
0x02   | 4    | Sequence       | uint32 LE | Packet sequence number
0x06   | 2    | Length         | uint16 LE | Total packet length
0x08   | 2    | Command        | uint16 LE | 0x8081 (VOICE_HEADER)
0x0A   | 1    | Slot           | uint8     | Timeslot
0x0B   | 4    | Source ID      | uint32 LE | Source DMR ID
0x0F   | 4    | Dest ID        | uint32 LE | Destination ID
0x13   | 1    | Call Type      | uint8     | 0=Private, 1=Group, 2=All
0x14   | 1    | Color Code     | uint8     | Color code (0-15)
0x15   | 2    | CRC            | uint16 LE | CRC checksum
```

### VOICE_TERMINATOR (0x8082)

Call termination packet sent after last voice frame.

```
Offset | Size | Field          | Type      | Description
-------|------|----------------|-----------|--------------------------------
0x00   | 2    | Signature      | uint16 BE | "PH" (0x5048)
0x02   | 4    | Sequence       | uint32 LE | Packet sequence number
0x06   | 2    | Length         | uint16 LE | Total packet length (23)
0x08   | 2    | Command        | uint16 LE | 0x8082 (VOICE_TERMINATOR)
0x0A   | 1    | Slot           | uint8     | Timeslot
0x0B   | 4    | Source ID      | uint32 LE | Source DMR ID
0x0F   | 4    | Dest ID        | uint32 LE | Destination ID
0x13   | 2    | Duration       | uint16 LE | Call duration (seconds)
0x15   | 2    | CRC            | uint16 LE | CRC checksum
```

---

## GPS Packets

### GPS_REQUEST (0x9001)

Request GPS position from a radio.

```
Offset | Size | Field          | Type      | Description
-------|------|----------------|-----------|--------------------------------
0x00   | 2    | Signature      | uint16 BE | "PH" (0x5048)
0x02   | 4    | Sequence       | uint32 LE | Packet sequence number
0x06   | 2    | Length         | uint16 LE | Total packet length (21)
0x08   | 2    | Command        | uint16 LE | 0x9001 (GPS_REQUEST)
0x0A   | 1    | Slot           | uint8     | Usually 0
0x0B   | 4    | Source ID      | uint32 LE | Dispatcher DMR ID
0x0F   | 4    | Dest ID        | uint32 LE | Target radio DMR ID
0x13   | 2    | CRC            | uint16 LE | CRC checksum
```

#### Example

```
50 48                   // Signature
05 00 00 00             // Sequence: 5
15 00                   // Length: 21 bytes
01 90                   // Command: GPS_REQUEST
00                      // Slot: 0
01 C9 86 00             // Source ID: 9000001 (dispatcher)
39 30 00 00             // Dest ID: 12345 (target radio)
XX XX                   // CRC
```

### GPS_RESPONSE (0x9002)

GPS position data from radio.

```
Offset | Size | Field          | Type      | Description
-------|------|----------------|-----------|--------------------------------
0x00   | 2    | Signature      | uint16 BE | "PH" (0x5048)
0x02   | 4    | Sequence       | uint32 LE | Packet sequence number
0x06   | 2    | Length         | uint16 LE | Total packet length (49)
0x08   | 2    | Command        | uint16 LE | 0x9002 (GPS_RESPONSE)
0x0A   | 1    | Slot           | uint8     | Usually 0
0x0B   | 4    | Source ID      | uint32 LE | Reporting radio DMR ID
0x0F   | 4    | Dest ID        | uint32 LE | Dispatcher DMR ID
0x13   | 8    | Latitude       | double LE | Latitude in degrees
0x1B   | 8    | Longitude      | double LE | Longitude in degrees
0x23   | 4    | Altitude       | float LE  | Altitude in meters (optional)
0x27   | 4    | Speed          | float LE  | Speed in km/h (optional)
0x2B   | 2    | Heading        | uint16 LE | Heading 0-359° (optional)
0x2D   | 4    | Timestamp      | uint32 LE | Unix timestamp
0x31   | 2    | CRC            | uint16 LE | CRC checksum
```

#### GPS Data Formats

**Latitude/Longitude**: IEEE 754 double-precision (8 bytes)
- Range: -90 to +90 (latitude), -180 to +180 (longitude)
- Example: 37.7749° = `0x40 0x42 0xE7 0x8C 0x1A 0x5A 0x64 0x3B` (LE)

**Altitude**: IEEE 754 single-precision (4 bytes)
- Range: Typically -500 to 10000 meters
- Example: 123.45m = `0xF6 0xB0 0xF6 0x42` (LE)

**Speed**: IEEE 754 single-precision (4 bytes)
- Range: 0 to 300 km/h
- Example: 55.5 km/h = `0x00 0x00 0x5E 0x42` (LE)

**Heading**: Unsigned 16-bit integer
- Range: 0-359 degrees
- Example: 180° = `0xB4 0x00` (LE)

---

## Text Message Packets

### TEXT_MESSAGE_SEND (0xA001)

Send text message from dispatcher to radio.

```
Offset | Size | Field          | Type      | Description
-------|------|----------------|-----------|--------------------------------
0x00   | 2    | Signature      | uint16 BE | "PH" (0x5048)
0x02   | 4    | Sequence       | uint32 LE | Packet sequence number
0x06   | 2    | Length         | uint16 LE | Total packet length (variable)
0x08   | 2    | Command        | uint16 LE | 0xA001 (TEXT_MESSAGE_SEND)
0x0A   | 1    | Slot           | uint8     | Usually 0
0x0B   | 4    | Source ID      | uint32 LE | Dispatcher DMR ID
0x0F   | 4    | Dest ID        | uint32 LE | Target radio DMR ID
0x13   | N    | Message        | UTF-8     | Text message (variable length)
...    | 2    | CRC            | uint16 LE | CRC checksum
```

#### Character Encoding

- **Encoding**: UTF-8
- **Maximum Length**: Typically 144 characters
- **Null Termination**: Not required
- **Special Characters**: Supported via UTF-8

#### Example

Message: "Hello Radio"

```
50 48                   // Signature
0A 00 00 00             // Sequence: 10
20 00                   // Length: 32 bytes
01 A0                   // Command: TEXT_MESSAGE_SEND
00                      // Slot: 0
01 C9 86 00             // Source ID: 9000001
39 30 00 00             // Dest ID: 12345
48 65 6C 6C 6F 20       // "Hello "
52 61 64 69 6F          // "Radio"
XX XX                   // CRC
```

### TEXT_MESSAGE_RECEIVE (0xA002)

Text message from radio to dispatcher.

Structure is identical to TEXT_MESSAGE_SEND, but:
- Command code is 0xA002
- Source ID is the sending radio
- Dest ID is typically dispatcher or broadcast

---

## Control Packets

### PTT_PRESS (0x8001)

Push-to-Talk button pressed.

```
Offset | Size | Field          | Type      | Description
-------|------|----------------|-----------|--------------------------------
0x00   | 2    | Signature      | uint16 BE | "PH" (0x5048)
0x02   | 4    | Sequence       | uint32 LE | Packet sequence number
0x06   | 2    | Length         | uint16 LE | Total packet length (21)
0x08   | 2    | Command        | uint16 LE | 0x8001 (PTT_PRESS)
0x0A   | 1    | Slot           | uint8     | Timeslot (0 or 1)
0x0B   | 4    | Source ID      | uint32 LE | Transmitting radio DMR ID
0x0F   | 4    | Dest ID        | uint32 LE | Destination talkgroup/ID
0x13   | 2    | CRC            | uint16 LE | CRC checksum
```

### PTT_RELEASE (0x8002)

Push-to-Talk button released.

Structure identical to PTT_PRESS, with command code 0x8002.

### LOGIN (0x7000)

Initial connection authentication.

```
Offset | Size | Field          | Type      | Description
-------|------|----------------|-----------|--------------------------------
0x00   | 2    | Signature      | uint16 BE | "PH" (0x5048)
0x02   | 4    | Sequence       | uint32 LE | Packet sequence number (0 or 1)
0x06   | 2    | Length         | uint16 LE | Total packet length (variable)
0x08   | 2    | Command        | uint16 LE | 0x7000 (LOGIN)
0x0A   | 1    | Slot           | uint8     | Usually 0
0x0B   | 4    | Source ID      | uint32 LE | Dispatcher DMR ID
0x0F   | 4    | Dest ID        | uint32 LE | Usually 0
0x13   | N    | Auth Data      | bytes     | Optional authentication (if any)
...    | 2    | CRC            | uint16 LE | CRC checksum
```

### KEEPALIVE (0x7100)

Connection heartbeat.

```
Offset | Size | Field          | Type      | Description
-------|------|----------------|-----------|--------------------------------
0x00   | 2    | Signature      | uint16 BE | "PH" (0x5048)
0x02   | 4    | Sequence       | uint32 LE | Packet sequence number
0x06   | 2    | Length         | uint16 LE | Total packet length (21)
0x08   | 2    | Command        | uint16 LE | 0x7100 (KEEPALIVE)
0x0A   | 1    | Slot           | uint8     | Usually 0
0x0B   | 4    | Source ID      | uint32 LE | Dispatcher DMR ID
0x0F   | 4    | Dest ID        | uint32 LE | Usually 0
0x13   | 2    | CRC            | uint16 LE | CRC checksum
```

---

## Emergency Packets

### EMERGENCY_DECLARE (0xF001)

Emergency button pressed on radio.

```
Offset | Size | Field          | Type      | Description
-------|------|----------------|-----------|--------------------------------
0x00   | 2    | Signature      | uint16 BE | "PH" (0x5048)
0x02   | 4    | Sequence       | uint32 LE | Packet sequence number
0x06   | 2    | Length         | uint16 LE | Total packet length (21)
0x08   | 2    | Command        | uint16 LE | 0xF001 (EMERGENCY_DECLARE)
0x0A   | 1    | Slot           | uint8     | Usually 0
0x0B   | 4    | Source ID      | uint32 LE | Emergency radio DMR ID
0x0F   | 4    | Dest ID        | uint32 LE | Dispatcher/broadcast
0x13   | 2    | CRC            | uint16 LE | CRC checksum
```

### EMERGENCY_ACK (0xF002)

Acknowledge emergency from dispatcher.

```
Offset | Size | Field          | Type      | Description
-------|------|----------------|-----------|--------------------------------
0x00   | 2    | Signature      | uint16 BE | "PH" (0x5048)
0x02   | 4    | Sequence       | uint32 LE | Packet sequence number
0x06   | 2    | Length         | uint16 LE | Total packet length (21)
0x08   | 2    | Command        | uint16 LE | 0xF002 (EMERGENCY_ACK)
0x0A   | 1    | Slot           | uint8     | Usually 0
0x0B   | 4    | Source ID      | uint32 LE | Dispatcher DMR ID
0x0F   | 4    | Dest ID        | uint32 LE | Emergency radio DMR ID
0x13   | 2    | CRC            | uint16 LE | CRC checksum
```

---

## CRC Calculation Example

```python
def calculate_crc_ccitt(data):
    """Calculate CRC-CCITT for packet data (excluding CRC bytes)"""
    polynomial = 0x1021
    crc = 0xFFFF
    
    for byte in data:
        crc ^= (byte << 8)
        for _ in range(8):
            if crc & 0x8000:
                crc = (crc << 1) ^ polynomial
            else:
                crc <<= 1
            crc &= 0xFFFF
    
    return crc
```

## Common Field Values

### Call Types
- `0x00`: Private call
- `0x01`: Group call
- `0x02`: All call

### Timeslots
- `0x00`: Slot 1
- `0x01`: Slot 2

### Boolean Flags
- `0x00`: False/No
- `0x01`: True/Yes

## See Also

- [DMR Protocol Overview](DMR_PROTOCOL.md)
- [Hytera IPSC Protocol](HYTERA_IPSC.md)
- [Implementation Guide](../IMPLEMENTATION_GUIDE.md)
