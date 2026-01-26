# HyteraGateway - Detailed Architecture

## Overview

HyteraGateway is a comprehensive radio gateway system for Hytera DMR radios that provides:
- TCP/IP connection to Hytera radios using IPSC protocol
- Multi-client TCP server for dispatcher applications
- Voice call recording with metadata
- Dual timeslot management for simultaneous calls
- PTT timeout protection
- Radio activity monitoring
- Auto-reconnection with exponential backoff
- FTP integration for call recording upload

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         HyteraGateway                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │              HyteraGateway.Api (REST API)                  │  │
│  │  - HTTP endpoints for radio control                        │  │
│  │  - Swagger/OpenAPI documentation                           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              ▲                                   │
│                              │                                   │
│  ┌───────────────────────────┴───────────────────────────────┐  │
│  │           HyteraGateway.Service (Host Service)             │  │
│  │  - Background service host                                 │  │
│  │  - Dependency injection configuration                      │  │
│  │  - Logging and configuration management                    │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                   │
│              ┌───────────────┼───────────────┐                  │
│              ▼               ▼               ▼                  │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐            │
│  │   Radio      │ │    Audio     │ │    Data      │            │
│  │   Module     │ │   Module     │ │   Module     │            │
│  └──────────────┘ └──────────────┘ └──────────────┘            │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

## Core Modules

### 1. HyteraGateway.Radio

The radio communication module handles all interactions with Hytera radios.

#### Components:

##### HyteraConnection
- Manages TCP connection to Hytera radio (default port 50000)
- IPSC protocol packet handling
- Auto-reconnection with exponential backoff: 1s, 2s, 5s, 10s, 30s, 60s, 120s, 300s, 600s, 900s
- Keepalive mechanism (every 10 seconds)
- Events: `PacketReceived`, `ConnectionLost`, `Reconnecting`, `ReconnectFailed`

**Connection Flow:**
```
┌─────────┐              ┌─────────┐
│ Gateway │              │  Radio  │
└────┬────┘              └────┬────┘
     │                        │
     │  TCP Connect           │
     ├───────────────────────>│
     │                        │
     │  LOGIN Packet          │
     ├───────────────────────>│
     │                        │
     │  LOGIN_RESPONSE        │
     │<───────────────────────┤
     │                        │
     │  KEEPALIVE (10s)       │
     ├───────────────────────>│
     │                        │
     │  KEEPALIVE_RESPONSE    │
     │<───────────────────────┤
     │                        │
```

##### HyteraPacketValidator
- Validates IPSC packets with detailed error reporting
- Checks:
  - "PH" signature (0x50, 0x48)
  - Length bounds (19-65535 bytes)
  - CRC-CCITT checksum
  - Unknown command codes (warning)
- Returns `ValidationResult` with severity: None/Warning/Error

##### DualSlotManager
- Manages DMR timeslots (Slot 1 and Slot 2)
- Tracks active calls per slot
- State: IsActive, SourceId, DestinationId, CallType, StartTime
- Thread-safe slot allocation
- Events: `SlotStateChanged`

##### PttTimeoutService
- Monitors PTT duration (default: 180 seconds)
- Auto-releases stuck PTT
- Configurable timeout and warning thresholds
- Timer check interval: 1 second
- Events: `PttTimeout`

##### RadioMonitoringService
- Periodic activity checks (default: 60 minutes)
- Periodic GPS position polling (default: 30 minutes)
- Background service with configurable intervals
- Integrates with RadioControllerConfig for radio list

##### RadioServerService
- Multi-client TCP server (default port: 8000)
- Protocol: `[Command:uint16_le][Length:uint16_le][Payload]`
- Commands:
  - `GET_RADIOS (0x1000)` - List all radios
  - `GET_STATUS (0x1001)` - Get radio status
  - `SEND_PTT (0x2001)` - Send PTT command
  - `REQUEST_GPS (0x2002)` - Request GPS position
  - `SEND_TEXT (0x2003)` - Send text message
  - `EVENT (0x0001)` - Server to client events
- Broadcasts radio events to all connected clients
- JSON payload format

##### CallRecorder
- Records voice calls with AMBE frame buffering
- File naming: `yyyyMMdd_HHmmss_{dmrId}_{talkGroupId}_slot{N}.{format}`
- Converts to WAV/MP3 on call end
- Saves metadata JSON with call details
- Thread-safe recording sessions
- Optional FTP upload with auto-delete

##### FtpClient
- Uploads recordings to FTP server
- Supports passive/active mode
- Configurable credentials
- Connection testing
- Directory creation

### 2. HyteraGateway.Audio

Handles audio streaming and processing.

- Audio capture from radio
- AMBE codec integration
- PCM conversion
- Streaming support

### 3. HyteraGateway.Data

Database integration for call records, GPS positions, and events.

- Entity Framework Core integration
- MySQL/MariaDB support
- Connection pooling
- Call history
- GPS position tracking
- Radio activity logs

### 4. HyteraGateway.Core

Shared models, interfaces, and configuration.

#### Key Interfaces:
- `IRadioService` - Radio communication abstraction
- `IAudioService` - Audio handling abstraction
- `IDatabaseService` - Database operations abstraction

#### Models:
- `RadioEvent` - Radio event data
- `CallRecord` - Call record details
- `GpsPosition` - GPS coordinates
- `RadioStatus` - Radio state
- `EmergencyAlert` - Emergency events

## Data Flow

### Voice Call Flow

```
Radio PTT Press
      │
      ▼
HyteraConnection
      │
      ├─> DualSlotManager (assign slot)
      │
      ├─> CallRecorder (start recording)
      │
      ├─> PttTimeoutService (start timer)
      │
      └─> RadioServerService (broadcast to clients)
      
Radio Voice Frames
      │
      ▼
HyteraConnection
      │
      ├─> CallRecorder (append AMBE frames)
      │
      └─> AudioService (decode & stream)

Radio PTT Release
      │
      ▼
HyteraConnection
      │
      ├─> DualSlotManager (release slot)
      │
      ├─> CallRecorder (stop, save WAV + metadata)
      │
      ├─> PttTimeoutService (clear timer)
      │
      ├─> FtpClient (optional upload)
      │
      └─> RadioServerService (broadcast to clients)
```

### Reconnection Flow

```
Connection Lost
      │
      ▼
ConnectionLost Event
      │
      ▼
If AutoReconnect = true
      │
      ▼
ReconnectAsync()
      │
      ├─> Wait 1s → Attempt 1
      │
      ├─> Wait 2s → Attempt 2
      │
      ├─> Wait 5s → Attempt 3
      │
      ├─> ... (exponential backoff)
      │
      ├─> Wait 900s → Attempt 10
      │
      └─> If all fail → ReconnectFailed Event
```

## Protocol Details

### IPSC Packet Structure

```
+----------+----------+-----------+---------+-------+-----------+
| Signature| Sequence | Length    | Command | Slot  | Source ID |
| (2 bytes)| (4 bytes)| (2 bytes) |(2 bytes)|(1 byte)|(4 bytes) |
+----------+----------+-----------+---------+-------+-----------+
| Dest ID  | Payload  | CRC       |
| (4 bytes)| (n bytes)| (2 bytes) |
+----------+----------+-----------+

Total: 19 bytes minimum (without payload)
```

Fields:
- **Signature**: 0x5048 ("PH") - big-endian
- **Sequence**: Packet sequence number - little-endian
- **Length**: Total packet length - little-endian
- **Command**: Command code (see HyteraCommand enum) - little-endian
- **Slot**: DMR timeslot (0 or 1)
- **Source ID**: Source DMR ID - little-endian
- **Dest ID**: Destination DMR ID - little-endian
- **Payload**: Command-specific data
- **CRC**: CRC-CCITT checksum (polynomial 0x1021) - little-endian

### RadioServer Protocol

```
Client Request:
+----------+-----------+-------------+
| Command  | Length    | Payload     |
| (2 bytes)| (2 bytes) | (n bytes)   |
+----------+-----------+-------------+

Server Response:
+----------+-----------+-------------+
| Command  | Length    | Payload     |
| (2 bytes)| (2 bytes) | (JSON data) |
+----------+-----------+-------------+
```

All fields are little-endian.

## Configuration

### RadioController.xml

```xml
<RadioController>
  <RadioIpAddress>192.168.1.1</RadioIpAddress>
  <RadioPort>50000</RadioPort>
  <DispatcherId>9000001</DispatcherId>
  <PttTimeoutSeconds>180</PttTimeoutSeconds>
  <VoipEnabled>false</VoipEnabled>
  <VrsEnabled>false</VrsEnabled>
  <ActivityCheckEnabled>true</ActivityCheckEnabled>
  <ActivityCheckMinutes>60</ActivityCheckMinutes>
  <PositionCheckEnabled>true</PositionCheckEnabled>
  <PositionCheckMinutes>30</PositionCheckMinutes>
  
  <Radios>
    <Radio DmrId="1234567" Name="Radio 1" IpAddress="192.168.1.100" Port="50000" Enabled="true" />
  </Radios>
  
  <Slots>
    <Slot Number="1" Name="Talkgroup 9" TalkGroupId="9" IsDefault="true" PttEnabled="true" Visible="true" />
  </Slots>
</RadioController>
```

### appsettings.json

See existing configuration in `/src/HyteraGateway.Service/appsettings.json`

## Threading and Concurrency

- **HyteraConnection**: Background receive loop on separate task
- **RadioServerService**: Each client handled on separate task
- **CallRecorder**: Thread-safe using `ConcurrentDictionary` and `SemaphoreSlim`
- **PttTimeoutService**: Timer-based checks on background thread
- **RadioMonitoringService**: Periodic tasks with configurable intervals

## Error Handling

- All async methods accept `CancellationToken`
- Comprehensive logging at all levels (Trace, Debug, Info, Warning, Error)
- Graceful reconnection on connection loss
- Packet validation before processing
- Database retry policies (via EF Core)

## Security Considerations

- FTP credentials stored in configuration (consider encryption)
- No authentication on RadioServer (consider adding)
- Packet validation prevents malformed data attacks
- CRC verification ensures data integrity

## Performance

- Connection pooling for database (5-50 connections)
- Efficient binary protocol (no text parsing)
- Minimal memory allocation (reuse buffers where possible)
- Background processing for non-critical operations
- Configurable timeouts and intervals

## Scalability

- Single radio connection per gateway instance
- Multiple dispatcher clients supported (configurable max)
- Recording files written to disk (consider object storage)
- Database can be clustered/replicated
- Horizontal scaling via multiple gateway instances (separate radios)

## Dependencies

### NuGet Packages
- Microsoft.Extensions.Logging
- Microsoft.Extensions.Hosting
- Microsoft.EntityFrameworkCore (MySQL)
- NAudio (audio processing)
- Swashbuckle.AspNetCore (API documentation)

### External Systems
- Hytera DMR Radio (IPSC protocol)
- MySQL/MariaDB Database
- FTP Server (optional, for recordings)
- Dispatcher Applications (via RadioServer)

## Future Enhancements

- AMBE codec integration for real audio playback
- Authentication/authorization for RadioServer
- WebSocket support for real-time events
- Web UI for monitoring and control
- Encryption for sensitive data
- Cluster support for high availability
- Rate limiting and DDoS protection
