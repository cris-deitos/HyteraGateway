# HyteraGateway Architecture

## Overview

HyteraGateway is designed as a multi-layered application following clean architecture principles. The solution separates concerns into distinct projects, each with specific responsibilities.

## System Layers

### 1. Core Layer (HyteraGateway.Core)

**Purpose**: Contains the domain models, interfaces, and configuration that are shared across all layers.

**Components**:
- **Models**: Domain entities like `RadioEvent`, `CallRecord`, `GpsPosition`, `EmergencyAlert`, `RadioStatus`
- **Interfaces**: Service contracts (`IRadioService`, `IAudioService`, `IDatabaseService`)
- **Configuration**: Application configuration classes (`HyteraGatewayConfig`)

**Dependencies**: None (pure domain layer)

### 2. Infrastructure Layer

#### HyteraGateway.Radio
**Purpose**: Implements radio communication protocol and connection management.

**Components**:
- **Services**: `HyteraConnectionService` - Manages USB NCM connection and protocol communication
- **Protocol**: `DMRPacket` - Represents DMR protocol packets
- **Enums**: `PacketType` - DMR packet types

**Key Responsibilities**:
- USB NCM network interface detection and connection
- DMR protocol packet encoding/decoding
- Radio command execution (PTT, GPS, SMS)
- Real-time event monitoring

**Dependencies**: Core

#### HyteraGateway.Data
**Purpose**: Handles all database operations using Dapper ORM.

**Components**:
- **Repositories**: `TransmissionRepository`, `PositionRepository`

**Key Responsibilities**:
- CRUD operations for transmissions and positions
- Database connection management
- Query optimization and data mapping

**Dependencies**: Core, Dapper, MySqlConnector

#### HyteraGateway.Audio
**Purpose**: Manages audio capture, playback, and codec operations.

**Components**:
- **Services**: `AudioCaptureService`, `AudioPlaybackService`
- **Codecs**: `AmbeCodec`, `OpusCodec`

**Key Responsibilities**:
- Audio stream capture from radio
- AMBE+2 codec encoding/decoding
- Audio file recording
- Real-time audio streaming via WebSocket

**Dependencies**: Core, NAudio

### 3. Presentation Layer

#### HyteraGateway.Api
**Purpose**: Provides REST API and WebSocket endpoints for external clients.

**Components**:
- **Controllers**: `RadiosController` - REST API endpoints
- **Hubs**: `RadioEventsHub` - SignalR hub for real-time events
- **Models**: Request/response DTOs

**Key Responsibilities**:
- HTTP request handling
- WebSocket connection management
- API documentation (Swagger)
- CORS and authentication

**Dependencies**: Core, Radio, Data, ASP.NET Core, SignalR

#### HyteraGateway.Service
**Purpose**: Windows Service for continuous background operation.

**Components**:
- **Worker**: `Worker` - Background service implementation

**Key Responsibilities**:
- Radio connection maintenance
- Event monitoring loop
- Service lifecycle management
- Windows Service integration

**Dependencies**: Core, Radio, Microsoft.Extensions.Hosting.WindowsServices

### 4. Test Layer (HyteraGateway.Tests)

**Purpose**: Contains unit and integration tests.

**Dependencies**: All application projects, xUnit

## Data Flow

### Radio Event Processing

```
Radio Hardware (MD785i)
    ↓ (USB NCM)
HyteraConnectionService
    ↓ (Packet parsing)
RadioEvent Model
    ↓ (SignalR broadcast)
Connected Clients
    ↓ (Database storage)
TransmissionRepository
    ↓ (MySQL)
Database (easyvol)
```

### API Request Flow

```
HTTP Client
    ↓ (REST API call)
RadiosController
    ↓ (Service call)
HyteraConnectionService
    ↓ (DMR packet)
Radio Hardware (MD785i)
```

### Audio Flow

```
Radio Audio Stream
    ↓ (UDP packets, AMBE+2)
AudioCaptureService
    ↓ (Decode)
AmbeCodec
    ↓ (PCM audio)
File System (recordings/)
    or
    ↓ (Encode)
OpusCodec
    ↓ (WebSocket)
Web Clients
```

## Design Patterns

### Dependency Injection
All services are registered in the DI container and injected via constructor injection.

### Repository Pattern
Data access is abstracted through repository classes that encapsulate database operations.

### Service Pattern
Business logic is encapsulated in service classes implementing interfaces from the Core layer.

### Background Service
The Worker class uses .NET's `BackgroundService` for long-running background tasks.

## Configuration

Configuration is hierarchical and loaded from `appsettings.json`:

```
HyteraGatewayConfig
  ├── RadioConfig (connection settings)
  ├── DatabaseConfig (MySQL settings)
  ├── FtpConfig (optional FTP upload)
  ├── AudioConfig (recording settings)
  └── ApiConfig (API/WebSocket settings)
```

## Database Schema (EasyVol)

### dispatch_transmissions
Stores call/transmission records with audio file references.

### dispatch_positions
Stores GPS position reports from radios.

### dispatch_events (future)
Stores all radio events for audit trail.

## Security Considerations

1. **API Authentication**: Optional API key authentication can be enabled
2. **CORS**: Configurable CORS policies for web client access
3. **Database**: Connection strings stored in configuration (use secrets in production)
4. **USB Access**: Requires appropriate device permissions

## Scalability

- **Horizontal**: Multiple API instances can share the same database
- **Vertical**: Service can handle multiple radio connections simultaneously
- **Caching**: Future implementation for frequently accessed data

## Monitoring and Logging

- Structured logging using `ILogger<T>`
- Log levels: Debug, Information, Warning, Error
- Log targets: Console, File, Event Log (Windows Service)

## Future Enhancements

1. **Clustering**: Support for multiple radios across different gateways
2. **Analytics**: Dashboard for transmission statistics and radio health
3. **Encryption**: End-to-end encryption for sensitive communications
4. **Mobile App**: Companion mobile app using the REST API
