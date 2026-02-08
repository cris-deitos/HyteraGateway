# Hytera UDP Protocol Ports

## Overview

The Hytera protocol uses multiple UDP services on different ports for different purposes. This configuration is defined in `RadioController.xml`.

## UDP Service Ports

| Service | Port | Description | Usage |
|---------|------|-------------|-------|
| **RRS** | 3002 | Radio Registration Service | Radio registration and keepalive messages |
| **LP** | 3003 | Location Protocol | GPS location data and position updates |
| **TMP** | 3004 | Text Message Protocol | Send and receive text messages |
| **RCP** | 3005 | Radio Control Protocol | Radio control commands (PTT, channel change, etc.) |
| **TP** | 3006 | Telemetry Protocol | Telemetry data from radios |
| **DTP** | 3007 | Data Transfer Protocol | Data transfer between radios |
| **SDMP** | 3009 | Status/Display Message Protocol | Status messages and display updates |

## Configuration

From `RadioController.xml`:

```xml
<UDPNetworkServices>
  <UDPService name="RRS" host="192.168.10.2" port="3002" multicast="False" />
  <UDPService name="LP" host="192.168.10.2" port="3003" multicast="False" />
  <UDPService name="TMP" host="192.168.10.2" port="3004" multicast="False" />
  <UDPService name="RCP" host="192.168.10.2" port="3005" multicast="False" />
  <UDPService name="TP" host="192.168.10.2" port="3006" multicast="False" />
  <UDPService name="DTP" host="192.168.10.2" port="3007" multicast="False" />
  <UDPService name="SDMP" host="192.168.10.2" port="3009" multicast="False" />
</UDPNetworkServices>
```

## Event Types

### RX Events (Reception)

Events that can be received from radios:

| Event | Description |
|-------|-------------|
| `RX_CALL` | Call received |
| `RX_CALL_ACK` | Call acknowledgment received |
| `RX_CALL_STARTED` | Call started notification |
| `RX_CALL_ENDED` | Call ended notification |
| `RX_GPS_SENTENCE` | GPS data received |
| `RX_FREE_TEXT_MESSAGE` | Free text message received |
| `RX_RADIO_ON` | Radio powered on |
| `RX_RADIO_OFF` | Radio powered off |
| `RX_CHANNEL_STATUS_CHANGED` | Channel status changed |
| `RX_END_PTT_ID` | End of PTT ID received |

### TX Events (Transmission)

Commands that can be sent to radios:

| Event | Description |
|-------|-------------|
| `TX_CALL` | Initiate a call |
| `TX_VOICE_CALL` | Initiate a voice call |
| `TX_GPS_QUERY` | Request GPS position |
| `TX_FREE_TEXT_MESSAGE` | Send text message |
| `TX_RADIO_CHECK` | Check radio status |
| `TX_REMOTE_MONITOR` | Activate remote monitoring |
| `TX_RADIO_DISABLE_ON_OFF` | Enable/disable radio |
| `TX_PRESS_RELEASE_BUTTON` | Simulate button press/release |
| `TX_CHANGE_CHANNEL_STATUS` | Change channel status |
| `TX_RADIO_SELECTION_TO_CONVERSATION` | Select radio for conversation |
| `TX_SEND_RULE` | Send rule to radio |

## Usage in Code

### Port Constants

```csharp
using HyteraGateway.Radio.Protocol.Hytera;

// Use port constants
int rrsPort = HyteraUdpPorts.RRS;      // 3002
int lpPort = HyteraUdpPorts.LP;        // 3003
int tmpPort = HyteraUdpPorts.TMP;      // 3004
int rcpPort = HyteraUdpPorts.RCP;      // 3005
```

### Event Type Constants

```csharp
// Check for event types
if (eventName == HyteraEventType.RX_CALL)
{
    // Handle incoming call
}

if (eventName == HyteraEventType.RX_GPS_SENTENCE)
{
    // Handle GPS data
}
```

### Loading Configuration

```csharp
var config = RadioControllerConfigLoader.Load("RadioController.xml");

// Get UDP services
foreach (var protocol in config.Protocols)
{
    foreach (var service in protocol.UDPNetworkServices)
    {
        Console.WriteLine($"{service.Name}: {service.Host}:{service.Port}");
    }
}
```

## Multi-Service Connection

To implement full Hytera protocol support, a connection service should:

1. **Listen on all required ports** (3002-3009)
2. **Handle events on appropriate ports**:
   - RRS (3002): Registration and keepalive
   - LP (3003): GPS data
   - TMP (3004): Text messages
   - RCP (3005): Control commands
   - TP (3006): Telemetry
   - DTP (3007): Data transfer
   - SDMP (3009): Status messages
3. **Route events to appropriate handlers** based on port/service type
4. **Maintain state across services** (e.g., radio registration affects all services)

## Previous Implementation

The previous implementation used:
- **Single TCP connection** on port 50000
- **IPSC protocol** for DMR communication

## New Implementation

The new implementation should:
- **Multiple UDP listeners** on ports 3002-3009
- **Hytera vendor protocol** using DLLs from lib/vendor/
- **Event-driven architecture** with handlers for each event type
- **Configuration from XML files** instead of hardcoded values
