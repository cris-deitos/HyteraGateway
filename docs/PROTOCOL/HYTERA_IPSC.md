# Hytera IP Site Connect (IPSC) Protocol

## Overview

Hytera IPSC is a proprietary TCP/IP-based protocol used for communication between Hytera dispatch software and Hytera MD785i (and similar) radios. It wraps DMR packets in an IP transport layer.

## Connection Details

### TCP Connection

- **Protocol**: TCP/IP
- **Default Port**: 50000
- **Connection**: Persistent, maintained with keepalives
- **Authentication**: Optional authentication data in login packet

### Connection Flow

```
1. Dispatcher → Radio: TCP Connect (port 50000)
2. Dispatcher → Radio: LOGIN (0x7000)
3. Radio → Dispatcher: LOGIN_RESPONSE (0x7001)
4. Connection Established
5. Dispatcher ↔ Radio: Commands and Events
6. Every 10 seconds: KEEPALIVE (0x7100)
7. Radio → Dispatcher: KEEPALIVE_RESPONSE (0x7101)
8. Eventually: DISCONNECT (0x7200)
9. TCP Close
```

## Packet Format

### General Structure

```
Offset | Bytes | Field        | Type      | Description
-------|-------|--------------|-----------|---------------------------
0x00   | 2     | Signature    | uint16 BE | "PH" (0x50 0x48 = 0x5048)
0x02   | 4     | Sequence     | uint32 LE | Packet sequence number
0x06   | 2     | Length       | uint16 LE | Total packet length
0x08   | 2     | Command      | uint16 LE | Command code
0x0A   | 1     | Slot         | uint8     | Timeslot (0 or 1)
0x0B   | 4     | Source ID    | uint32 LE | Source DMR ID
0x0F   | 4     | Dest ID      | uint32 LE | Destination DMR ID
0x13   | N     | Payload      | bytes     | Command-specific data
...    | 2     | CRC          | uint16 LE | CRC-CCITT checksum
```

**Note**: BE = Big Endian, LE = Little Endian

### Signature

The packet always starts with "PH" (0x5048 in big-endian):
- **0x50**: ASCII 'P'
- **0x48**: ASCII 'H'

This helps identify valid IPSC packets in a stream.

## Command Reference

### Connection Commands

| Command | Code | Direction | Description |
|---------|------|-----------|-------------|
| LOGIN | 0x7000 | D→R | Initial login/authentication request |
| LOGIN_RESPONSE | 0x7001 | R→D | Login acknowledgment |
| KEEPALIVE | 0x7100 | D→R | Keepalive/heartbeat (every 10 sec) |
| KEEPALIVE_RESPONSE | 0x7101 | R→D | Keepalive acknowledgment |
| DISCONNECT | 0x7200 | D→R | Graceful disconnect request |

**D→R**: Dispatcher to Radio, **R→D**: Radio to Dispatcher

### Call Control Commands

| Command | Code | Direction | Description |
|---------|------|-----------|-------------|
| PTT_PRESS | 0x8001 | D↔R | Push-to-Talk pressed |
| PTT_RELEASE | 0x8002 | D↔R | Push-to-Talk released |
| CALL_START | 0x8010 | R→D | Call transmission started |
| CALL_END | 0x8011 | R→D | Call transmission ended |
| CALL_ACK | 0x8012 | D→R | Call acknowledgment |

### Voice Commands

| Command | Code | Direction | Description |
|---------|------|-----------|-------------|
| VOICE_FRAME | 0x8080 | D↔R | Voice frame with AMBE+2 data |
| VOICE_HEADER | 0x8081 | D↔R | Voice call header |
| VOICE_TERMINATOR | 0x8082 | D↔R | Voice call terminator |

### GPS Commands

| Command | Code | Direction | Description |
|---------|------|-----------|-------------|
| GPS_REQUEST | 0x9001 | D→R | Request GPS position |
| GPS_RESPONSE | 0x9002 | R→D | GPS position data |
| GPS_TRIGGER | 0x9003 | D→R | Trigger GPS update |

### Text Messaging Commands

| Command | Code | Direction | Description |
|---------|------|-----------|-------------|
| TEXT_MESSAGE_SEND | 0xA001 | D→R | Send text message to radio |
| TEXT_MESSAGE_RECEIVE | 0xA002 | R→D | Text message from radio |
| TEXT_MESSAGE_ACK | 0xA003 | R→D | Message delivery acknowledgment |

### Emergency Commands

| Command | Code | Direction | Description |
|---------|------|-----------|-------------|
| EMERGENCY_DECLARE | 0xF001 | R→D | Emergency button pressed |
| EMERGENCY_ACK | 0xF002 | D→R | Emergency acknowledged |
| EMERGENCY_CANCEL | 0xF003 | D→R | Cancel emergency state |

### Status Commands

| Command | Code | Direction | Description |
|---------|------|-----------|-------------|
| STATUS_REQUEST | 0xB001 | D→R | Request radio status |
| STATUS_RESPONSE | 0xB002 | R→D | Radio status data |
| RADIO_CHECK | 0xB010 | D→R | Check if radio is alive |
| RADIO_CHECK_ACK | 0xB011 | R→D | Radio check response |

## Payload Structures

### LOGIN (0x7000)

```
Offset | Bytes | Field | Description
-------|-------|-------|---------------------------
0x13   | N     | Auth  | Optional authentication data
```

### GPS_RESPONSE (0x9002)

```
Offset | Bytes | Field     | Type       | Description
-------|-------|-----------|------------|----------------------
0x13   | 8     | Latitude  | double LE  | Latitude (degrees)
0x1B   | 8     | Longitude | double LE  | Longitude (degrees)
0x23   | 4     | Altitude  | float LE   | Altitude (meters)
0x27   | 4     | Speed     | float LE   | Speed (km/h)
0x2B   | 2     | Heading   | uint16 LE  | Heading (0-359°)
0x2D   | 4     | Timestamp | uint32 LE  | Unix timestamp
```

### TEXT_MESSAGE_SEND/RECEIVE (0xA001/0xA002)

```
Offset | Bytes | Field   | Type  | Description
-------|-------|---------|-------|---------------------------
0x13   | N     | Message | UTF-8 | Text message (variable length)
```

### VOICE_FRAME (0x8080)

```
Offset | Bytes | Field      | Description
-------|-------|------------|--------------------------------
0x13   | 33    | AMBE Data  | AMBE+2 encoded voice (33 bytes)
0x34   | 1     | Frame Num  | Frame number in superframe (0-5)
0x35   | 1     | Last Frame | 1 if last frame, 0 otherwise
```

## CRC Calculation

Same as DMR: CRC-CCITT with polynomial 0x1021, initial value 0xFFFF.

**Python Example**:

```python
def calculate_crc(data):
    crc = 0xFFFF
    for byte in data:
        crc ^= (byte << 8)
        for _ in range(8):
            if crc & 0x8000:
                crc = (crc << 1) ^ 0x1021
            else:
                crc <<= 1
            crc &= 0xFFFF
    return crc
```

## Keepalive Mechanism

To maintain the connection:

1. **Interval**: Send KEEPALIVE every 10 seconds
2. **Response**: Radio responds with KEEPALIVE_RESPONSE
3. **Timeout**: If no response within 30 seconds, consider connection lost
4. **Reconnection**: Close and reopen TCP connection, perform LOGIN again

## Event Processing

The dispatcher should process events asynchronously:

1. **Background Receive Thread**: Continuously read packets from TCP stream
2. **Parse Packets**: Validate signature and CRC
3. **Dispatch Events**: Route to appropriate handlers based on command code
4. **Send Responses**: Acknowledge as needed (e.g., EMERGENCY_ACK)

## Error Handling

### Connection Errors

- **Login Failure**: Retry with exponential backoff
- **Connection Lost**: Attempt automatic reconnection
- **Keepalive Timeout**: Close and reconnect

### Packet Errors

- **Invalid Signature**: Discard packet, log error
- **CRC Mismatch**: Discard packet, log error
- **Unknown Command**: Log warning, continue processing

## Implementation Best Practices

1. **Sequence Numbers**: Increment for each sent packet, track received sequences
2. **Thread Safety**: Use locks when accessing connection from multiple threads
3. **Logging**: Log all packets at DEBUG level, connections at INFO level
4. **Buffer Management**: Use fixed-size buffers, handle partial reads
5. **Reconnection**: Implement exponential backoff with maximum retry limit

## Security Considerations

1. **Authentication**: Optional in LOGIN packet, typically not encrypted
2. **Encryption**: Protocol does not provide built-in encryption
3. **Network Security**: Use VPN or firewall to restrict access to port 50000
4. **Credentials**: Store authentication data securely

## Example Usage

### C# Connection Example

```csharp
var connection = new HyteraConnection("192.168.1.100", dispatcherId: 9000001);
connection.PacketReceived += (sender, packet) => {
    Console.WriteLine($"Received: {packet.Command}");
};

await connection.ConnectAsync();
await connection.SendPttAsync(targetId: 123456, press: true);
// ... wait for PTT release
await connection.SendPttAsync(targetId: 123456, press: false);
await connection.DisconnectAsync();
```

## See Also

- [DMR Protocol](DMR_PROTOCOL.md)
- [Packet Reference](PACKET_REFERENCE.md)
- [Implementation Guide](../IMPLEMENTATION_GUIDE.md)
