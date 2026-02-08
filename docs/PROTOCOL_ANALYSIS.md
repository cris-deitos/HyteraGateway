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

Based on the RadioController.xml configuration, PTT control is handled through the `TX_PRESS_RELEASE_BUTTON` event.

Configuration:
- Service: RCP (Radio Control Protocol)
- Port: 3005 (UDP)
- Event: `TX_PRESS_RELEASE_BUTTON`
- Supported profiles: All radio profiles (HYT Display/GPS, HYT NO Display/GPS, MOTO Display/GPS, etc.)

Protocol details:
```
PTT Press: Initiates transmission
PTT Release: Ends transmission
Transport: UDP to 192.168.10.2:3005
Format: Proprietary Hytera protocol packets
```

The PTT state is also indicated through tone signaling:
- `EncoderLeadingEdgePTT`: Tone sent when PTT is pressed
- `EncoderTrailingEdgePTT`: Tone sent when PTT is released
- Tone duration: 70ms (configurable via `EncoderToneDuration`)
- Pre-time: 500ms (configurable via `EncoderToneDurationPretime`)

**GPS Request**:

Based on the RadioController.xml configuration, GPS functionality is handled through the Location Protocol (LP).

Configuration:
- Service: LP (Location Protocol)
- Port: 3003 (UDP)
- Host: 192.168.10.2
- Query Event: `TX_GPS_QUERY`
- Response Event: `RX_GPS_SENTENCE`

Protocol details:
```
GPS Query: Requests current position from radio
GPS Response: Returns position data in NMEA format
Transport: UDP to 192.168.10.2:3003
Format: NMEA sentences (standard GPS data format)
```

GPS Configuration Parameters (from Hytera_HyteraProtocol.xml):
- `BeaconBaseValue`: 40000 (base value for beacon reporting)
- `BeaconBatteryLowStatus`: 35 (battery level threshold)
- `BeaconBatteryLowOffset`: 1000 (offset when battery is low)
- `LocalizationType`: O (localization type indicator)

NMEA format typically includes:
- Latitude/Longitude in degrees and minutes
- Altitude in meters
- Speed and heading
- Fix quality and satellite count
- Timestamp (UTC)

**Text Message**:

Based on the RadioController.xml configuration, text messaging is handled through the Text Message Protocol (TMP).

Configuration:
- Service: TMP (Text Message Protocol)
- Port: 3004 (UDP)
- Host: 192.168.10.2
- Transmit Event: `TX_FREE_TEXT_MESSAGE`
- Receive Event: `RX_FREE_TEXT_MESSAGE`

Protocol details:
```
Message Send: TX_FREE_TEXT_MESSAGE
Message Receive: RX_FREE_TEXT_MESSAGE
Transport: UDP to 192.168.10.2:3004
Format: DMR short data or proprietary Hytera format
Encoding: Likely UTF-8 or ASCII
Max length: Limited by DMR short data specifications
```

Supported in profiles:
- HYT Display/GPS and HYT Display/NO GPS
- MOTO Display/GPS and MOTO Display/NO GPS
- HYT TALKGROUPS and MOTO TALKGROUPS

Status Message Configuration (from Hytera_HyteraProtocol.xml):
- Code 91: "MARCATURA" (voice=False, emergency=False)
- Code 95: "Stato Emergenza" (voice=False, emergency=True)
- Code 0: "Voce" (voice=True, emergency=False)

### Audio Packets

**Audio Configuration** (from Hytera_HyteraProtocol.xml):

RTP Settings:
- `RtpBufferSize`: 10 packets
- `RtpTimeoutMs`: 500ms (timeout for RTP packet reception)
- `RtpLogReOrder`: false (disable logging of packet reordering)

Audio Timing:
- `AudioRxTimeoutMs`: 55ms (receive timeout)
- `AudioTxTimeoutMs`: 55ms (transmit timeout)

Volume Settings:
- `RxVolume`: 500 (receive audio level)
- `TxVolume`: 100 (transmit audio level)

Audio Format:
- `RTPAudioOutputType`: PCM (uncompressed audio over RTP)

**AMBE+2 Voice Frames**:

DMR uses AMBE+2 vocoder for voice encoding:
```
Frame size: 33 bytes (264 bits)
Sample rate: 8000 Hz
Frame duration: 60ms
Samples per frame: 480 (60ms * 8000 Hz / 1000)
Voice bitrate: 2450 bps
FEC bitrate: 3150 bps
Total: 5600 bps (DMR specification)
```

**Audio Stream**:
- Transport: RTP over UDP (port determined by RRS service)
- Formats supported:
  - PCM: Uncompressed 16-bit audio at 8kHz (as configured)
  - AMBE+2: Encoded DMR voice frames
- Encoding: AMBE+2 encoded frames for radio transmission
- Decoding: Requires AMBE vocoder (hardware dongle or licensed software)

RTP Packet Structure:
- RTP Header: Standard 12 bytes
- Payload: Audio data (PCM or AMBE+2)
- Buffer management: 10 packet jitter buffer
- Packet loss handling: 500ms timeout before assuming loss

### GPS Packets

**Position Report Format** (from Hytera_HyteraProtocol.xml):

GPS Configuration:
- Service: LP (Location Protocol) on port 3003
- Beacon Base Value: 40000 (identifier for beacon reporting)
- Battery Low Status: 35% threshold
- Battery Low Offset: 1000 (adjustment when battery is low)
- Localization Type: "O" (type indicator)

Position Report Contents:
```
Format: NMEA sentences via RX_GPS_SENTENCE event
Standard NMEA Fields:
  - Latitude: Degrees and decimal minutes (DDMM.MMMM format)
  - Longitude: Degrees and decimal minutes (DDDMM.MMMM format)
  - Altitude: Meters above sea level
  - Speed: Knots or km/h
  - Heading: Degrees (0-359)
  - Timestamp: UTC time
  - Fix Quality: GPS fix status (0=invalid, 1=GPS fix, 2=DGPS fix)
  - Satellites: Number of satellites in use
  - HDOP: Horizontal dilution of precision
```

Common NMEA Sentence Types:
- `$GPGGA`: Global Positioning System Fix Data
- `$GPRMC`: Recommended Minimum Navigation Information
- `$GPGLL`: Geographic Position - Latitude/Longitude

Beacon Reporting:
- Base value for identification: 40000
- Battery status monitoring with threshold at 35%
- Offset adjustment when battery is low: +1000 to base value

The GPS data is transmitted via the LP service when queried using `TX_GPS_QUERY` and received through `RX_GPS_SENTENCE` events.

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

**Network Security**:
- IP Address Configuration:
  - Interface IP: 192.168.10.1 (host/gateway side)
  - Base IP: 12.0.0.100 (radio base address)
  - IP Prefix Single: 12 (for individual radios)
  - IP Prefix Group: 225 (for multicast groups)

**Connection Control**:
- Connection Type: "B" (Binary/Basic mode)
- Ping Interval: 60 seconds (keepalive mechanism)
- RRS Timer: 35 minutes first timer, 2 minutes second timer
- TTL (Time To Live): 70 seconds for most profiles

**Authentication**:
- DMR ID validation required for radio identification
- Radio Check command available: `TX_RADIO_CHECK`
- Radio Selection for Conversation: `TX_RADIO_SELECTION_TO_CONVERSATION`

**Access Control Features**:
- Remote Monitor: `TX_REMOTE_MONITOR` (surveillance capability)
- Radio Disable/Enable: `TX_RADIO_DISABLE_ON_OFF` (remote control)
- Channel Status Management: `TX_CHANGE_CHANNEL_STATUS`
- Busy Channel Management: Configurable per profile

**Encryption**:
- DMR supports AES encryption (status depends on radio configuration)
- Encryption status not explicitly defined in configuration files
- Voice and data can be encrypted at DMR layer

**Emergency Handling**:
- Emergency Status: Code 95 "Stato Emergenza"
- Emergency ACK encoding: `EncoderACKForEmergency`
- Emergency call alert: `EncoderCallAlert`
- Allow Send Emergency: false (default configuration)

**Transmission Control**:
- Allow Transmit Interrupt: false (prevents unauthorized interruption)
- RRS Revert: disabled (no automatic revert to repeater)

**Multicast Configuration**:
- Multicast TTL: 6 (hop limit for multicast packets)
- Multicast: Disabled for all UDP services (direct unicast only)

**Protocol Isolation**:
- Separate UDP ports for each service (RRS:3002, LP:3003, TMP:3004, RCP:3005, TP:3006, DTP:3007, SDMP:3009)
- ACK Software: Disabled (no software-level acknowledgment layer)
- Telemetry Query Status: Code 99

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

**Last Updated**: 2026-02-08
**Status**: Protocol documentation completed based on XML configuration analysis
