# HyteraGateway

A modern .NET 8 gateway for interfacing with Hytera MD785i DMR radios via USB NCM connection.

## Features

- **USB NCM Connection**: Connect to Hytera MD785i radios via USB network interface
- **Real-Time Events**: Capture PTT, calls, GPS positions, emergency alerts, and text messages
- **Audio Recording**: Record and stream AMBE+2 codec audio from radio transmissions
- **MySQL Database**: Store transmissions, GPS positions, and events in EasyVol schema
- **REST API**: Comprehensive API for radio control and monitoring
- **WebSocket Events**: Real-time event streaming via SignalR
- **Windows Service**: Run as a background service for continuous monitoring

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
- Hytera MD785i radio with USB NCM cable

### Configuration

Edit `appsettings.json` in the API or Service project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=easyvol;User=root;Password=;"
  },
  "HyteraGateway": {
    "Radio": {
      "IpAddress": "192.168.1.1",
      "ControlPort": 50000,
      "AudioPort": 50001
    },
    "Database": {
      "Host": "localhost",
      "Database": "easyvol"
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

## License

[Specify your license here]

## Contributing

Contributions are welcome! Please submit pull requests or open issues for bugs and feature requests.

## Support

For support and questions, please open an issue on GitHub.