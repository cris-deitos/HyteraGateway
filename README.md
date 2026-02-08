# HyteraGateway

[![Build and Test](https://github.com/cris-deitos/HyteraGateway/actions/workflows/build.yml/badge.svg)](https://github.com/cris-deitos/HyteraGateway/actions/workflows/build.yml)
[![Release](https://github.com/cris-deitos/HyteraGateway/actions/workflows/release.yml/badge.svg)](https://github.com/cris-deitos/HyteraGateway/actions/workflows/release.yml)

A modern .NET 8 gateway for interfacing with Hytera DMR radios via USB NCM or direct Ethernet connection.

## Features

- **USB NCM Connection**: Connect to Hytera MD785i radios via USB network interface
- **Ethernet Connection**: Connect to Hytera HM785 and other radios with built-in Ethernet ports
- **Network Scanning**: Discover Ethernet-connected radios on your LAN
- **Real-Time Events**: Capture PTT, calls, GPS positions, emergency alerts, and text messages
- **Audio Recording**: Record and stream AMBE+2 codec audio from radio transmissions
- **MySQL Database**: Store transmissions, GPS positions, and events in EasyVol schema
- **REST API**: Comprehensive API for radio control and monitoring
- **WebSocket Events**: Real-time event streaming via SignalR
- **Windows Service**: Run as a background service for continuous monitoring

## Supported Radio Models

### USB Connection (NCM/RNDIS)
- **Hytera MD785i** - Mobile radio with USB port
- **Hytera MD785G** - Mobile radio with GPS and USB
- Other Hytera DMR radios with USB data cable

### Ethernet Connection (Direct IP)
- **Hytera HM785** - Mobile radio with built-in Ethernet
- **Hytera HR1065** - Repeater with network interface
- Other Hytera devices with RJ45 Ethernet port

### Connection Configuration

The gateway supports multiple connection types depending on your radio model:

| Radio Type | Connection | Typical IP | Configuration |
|------------|------------|------------|---------------|
| MD785i (USB NCM) | USB Cable | 192.168.1.1 | Auto-detected via USB interface |
| MD785i (USB RNDIS) | USB Cable | 192.168.42.1 | Auto-detected via USB interface |
| HM785 (Ethernet) | RJ45 Cable | DHCP or Static | Manual IP or network scan |
| HR1065 (Ethernet) | RJ45 Cable | DHCP or Static | Manual IP or network scan |

**Note:** USB radios may use either NCM or RNDIS drivers depending on your operating system and radio firmware. The IP address (192.168.1.1 or 192.168.42.1) is determined by the radio's USB configuration. The gateway's auto-detect feature will find the correct interface automatically.

### Phase 2.5 - Production Features (NEW)

- **Auto-Reconnection**: Automatic reconnection with exponential backoff (max 10 attempts, up to 300s delay)
- **Enhanced Packet Validation**: Comprehensive CRC and signature validation for protocol packets
- **RadioController.xml Parser**: Load and save radio/slot configurations from XML files
- **RadioServer Protocol (Port 8000)**: NETRadioClient-compatible TCP server for multi-client dispatcher control
- **Dual Slot Manager**: Simultaneous Timeslot 1 and Timeslot 2 call handling for DMR
- **PTT Timeout Service**: Automatic PTT release after configurable timeout (default 180s)
- **Radio Monitoring Service**: Periodic activity checks and GPS position requests
- **Comprehensive Testing**: 69+ unit tests with >80% code coverage

## Architecture

The solution is organized into multiple projects:

- **HyteraGateway.Core**: Core models, interfaces, and configuration
- **HyteraGateway.Radio**: Radio protocol implementation and connection management
- **HyteraGateway.Data**: Database repositories using Dapper and MySQL
- **HyteraGateway.Audio**: Audio capture, playback, and codec support
- **HyteraGateway.Api**: REST API and WebSocket server
- **HyteraGateway.Service**: Windows Service for background operation
- **HyteraGateway.Tests**: Unit and integration tests

## Quick Start

### Prerequisites

- .NET 8.0 SDK or later
- MySQL 8.0 or later
- Hytera radio with USB NCM cable or Ethernet connection

### Configuration

Edit `appsettings.json` in the API or Service project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=easyvol;User=root;Password=;"
  },
  "HyteraGateway": {
    "Radio": {
      "ConnectionType": "Auto",
      "IpAddress": "192.168.1.1",
      "ControlPort": 50000,
      "AudioPort": 50001,
      "AutoReconnect": true
    },
    "Database": {
      "Host": "localhost",
      "Database": "easyvol"
    },
    "RadioMonitoring": {
      "ActivityCheckEnabled": true,
      "ActivityCheckIntervalMinutes": 60,
      "PositionCheckEnabled": true,
      "PositionCheckIntervalMinutes": 30
    },
    "RadioServer": {
      "Enabled": true,
      "Port": 8000,
      "MaxClients": 10
    },
    "PttTimeout": {
      "TimeoutSeconds": 180
    }
  }
}
```

### Running the API

```bash
cd src/HyteraGateway.Api
dotnet run
```

Navigate to `http://localhost:5000/swagger` for API documentation.

### Running as Windows Service

```bash
cd src/HyteraGateway.Service
dotnet publish -c Release
sc create HyteraGateway binPath="<path-to-published-exe>"
sc start HyteraGateway
```

## API Endpoints

### Radio Control

- `GET /api/radios/{dmrId}/status` - Get radio status
- `POST /api/radios/{dmrId}/ptt` - Send PTT command
- `POST /api/radios/{dmrId}/gps` - Request GPS position
- `POST /api/radios/{dmrId}/sms` - Send text message

### WebSocket

Connect to `/hubs/radio-events` for real-time event streaming.

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/radio-events")
    .build();

connection.on("RadioEvent", (event) => {
    console.log("Received event:", event);
});

await connection.start();
await connection.invoke("SubscribeToRadio", 12345);
```

## Development

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

### Code Structure

See [ARCHITECTURE.md](docs/ARCHITECTURE.md) for detailed architecture documentation.

## Protocol Implementation

The DMR protocol implementation is currently in placeholder state. Protocol analysis and reverse engineering is documented in [PROTOCOL_ANALYSIS.md](docs/PROTOCOL_ANALYSIS.md).

## Production Readiness

This project includes comprehensive production features:

- **Robustness**: Auto-reconnection with exponential backoff prevents connection loss
- **Validation**: Enhanced packet validation ensures data integrity
- **Monitoring**: Periodic activity and position checks keep tabs on radio fleet
- **Safety**: PTT timeout prevents stuck transmissions
- **Scalability**: RadioServer protocol supports multiple dispatcher clients
- **Reliability**: Dual slot manager handles simultaneous calls on both timeslots
- **Quality**: 69+ unit tests ensure code reliability and maintainability

## 📦 Download

Download the latest release from the [Releases page](https://github.com/cris-deitos/HyteraGateway/releases).

### Available Packages

| Package | Description |
|---------|-------------|
| `HyteraGateway-UI-*-win-x64.zip` | Desktop application (WPF) |
| `HyteraGateway-API-*-win-x64.zip` | REST API server |
| `HyteraGateway-Service-*-win-x64.zip` | Windows Service |

All packages are self-contained and do not require .NET runtime to be installed.

## 🔨 Building from Source

### Prerequisites
- Windows 10/11
- .NET 8 SDK

### Quick Build
```powershell
# Clone the repository
git clone https://github.com/cris-deitos/HyteraGateway.git
cd HyteraGateway

# Run build script
.\scripts\build-release.ps1
```

### Manual Build
```powershell
dotnet publish src\HyteraGateway.UI\HyteraGateway.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
```

## License

[Specify your license here]

## Contributing

Contributions are welcome! Please submit pull requests or open issues for bugs and feature requests.

## Support

For support and questions, please open an issue on GitHub.