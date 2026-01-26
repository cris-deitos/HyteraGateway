# Packet Inspector

A CLI tool for analyzing raw Hytera IPSC and DMR protocol packets.

## Usage

```bash
PacketInspector <command> <input>
```

## Commands

- **hex** `<hex_string>` - Analyze hex string (spaces/dashes optional)
- **file** `<file_path>` - Analyze binary file containing packet data

## Features

- Hex dump display with ASCII representation
- Hytera IPSC packet parsing and validation
- DMR packet parsing and validation
- CRC verification
- Payload interpretation (text messages, GPS data, etc.)
- Detailed field breakdown

## Examples

### Analyze Hex String
```bash
dotnet run --project tools/PacketInspector -- hex "50 48 01 00 00 00 15 00 01 80 00 39 30 00 00 64 00 00 00 12 34"
```

Without spaces:
```bash
dotnet run --project tools/PacketInspector -- hex 504801000000150001800039300000640000001234
```

### Analyze Binary File
```bash
dotnet run --project tools/PacketInspector -- file captured_packet.bin
```

## Sample Output

```
Packet Inspector
================

Total bytes: 23

Hex Dump:
--------
0000:  50 48 01 00 00 00 15 00  01 80 00 39 30 00 00 64   PH.........90..d
0010:  00 00 00 12 34 56 78                               ....4Vx

Hytera IPSC Packet Analysis:
----------------------------
✓ Valid Hytera IPSC packet
  Signature:    PH (0x5048)
  Sequence:     1
  Command:      PTT_PRESS (0x8001)
  Slot:         0
  Source ID:    12345
  Dest ID:      100
  Payload:      2 bytes
  CRC:          0x7856

DMR Packet Analysis:
-------------------
✗ Not a valid DMR packet: Packet too small
```

## Packet Types Supported

### Hytera IPSC Packets
- Connection commands (LOGIN, KEEPALIVE, DISCONNECT)
- Call control (PTT_PRESS, PTT_RELEASE, CALL_START, CALL_END)
- Voice frames (VOICE_FRAME, VOICE_HEADER, VOICE_TERMINATOR)
- GPS (GPS_REQUEST, GPS_RESPONSE)
- Text messaging (TEXT_MESSAGE_SEND, TEXT_MESSAGE_RECEIVE)
- Emergency (EMERGENCY_DECLARE, EMERGENCY_ACK)
- Status commands

### DMR Packets
- ETSI TS 102 361 standard packets
- Sync pattern recognition
- Color code extraction
- Call type identification (Private, Group, All)
- CRC-CCITT validation

## Building

```bash
dotnet build tools/PacketInspector/PacketInspector.csproj
```

## Publishing

```bash
dotnet publish tools/PacketInspector/PacketInspector.csproj -c Release -o ./publish
```

Then run with:
```bash
./publish/PacketInspector hex "50480100..."
```

## Capturing Packets

You can use Wireshark or tcpdump to capture packets from the radio:

### Using tcpdump
```bash
# Capture to file
sudo tcpdump -i eth0 -w packets.pcap host 192.168.1.100 and port 50000

# Extract specific packet to hex
tcpdump -r packets.pcap -X | grep "0x0000:"
```

### Using Wireshark
1. Start capture on network interface
2. Filter: `tcp.port == 50000`
3. Right-click packet → Copy → Bytes → Hex Stream
4. Paste into PacketInspector

## Use Cases

- **Protocol debugging** - Verify packet structure and contents
- **Reverse engineering** - Analyze unknown packet formats
- **Testing** - Validate packet generation
- **Education** - Learn protocol structure
- **Troubleshooting** - Diagnose communication issues

## Notes

- The tool attempts to parse data as both Hytera IPSC and DMR formats
- Invalid packets will show error messages with reasons
- CRC validation helps identify corrupted or malformed packets
- Payload interpretation is automatic for known command types
