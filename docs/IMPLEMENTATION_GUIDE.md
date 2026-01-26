# Implementation Guide

This guide shows how to use the Hytera Gateway protocol implementation to communicate with Hytera MD785i radios.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Connection Management](#connection-management)
3. [Sending Commands](#sending-commands)
4. [Receiving Events](#receiving-events)
5. [Error Handling](#error-handling)
6. [Best Practices](#best-practices)

---

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- Hytera MD785i (or compatible) radio
- Network connection to radio (USB NCM or Ethernet)

### Basic Usage Example

```csharp
using HyteraGateway.Radio.Protocol.Hytera;
using Microsoft.Extensions.Logging;

// Create logger
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});
var logger = loggerFactory.CreateLogger<HyteraConnection>();

// Create connection
var connection = new HyteraConnection(
    radioIp: "192.168.1.100",
    dispatcherId: 9000001,
    port: 50000
);

// Subscribe to events
connection.PacketReceived += (sender, packet) =>
{
    Console.WriteLine($"Received: {packet.Command} from {packet.SourceId}");
};

connection.ConnectionLost += (sender, args) =>
{
    Console.WriteLine("Connection lost!");
};

// Connect
if (await connection.ConnectAsync())
{
    Console.WriteLine("Connected successfully");
    
    // Send PTT command
    await connection.SendPttAsync(
        destinationId: 100,  // Talkgroup 100
        press: true,
        slot: 0
    );
    
    await Task.Delay(2000); // Hold PTT for 2 seconds
    
    await connection.SendPttAsync(
        destinationId: 100,
        press: false,
        slot: 0
    );
    
    // Disconnect
    await connection.DisconnectAsync();
}
```

---

## Connection Management

### Establishing Connection

```csharp
var connection = new HyteraConnection(
    radioIp: "192.168.1.100",     // Radio IP address
    dispatcherId: 9000001,         // Your dispatcher DMR ID
    port: 50000                    // Default IPSC port
);

// Optional: Pass authentication data
byte[] authData = Encoding.UTF8.GetBytes("password123");
bool success = await connection.ConnectAsync(authData);

if (success)
{
    Console.WriteLine("Connected and authenticated");
}
else
{
    Console.WriteLine("Connection failed");
}
```

### Connection States

The `IsConnected` property indicates connection status:

```csharp
if (connection.IsConnected)
{
    // Connection is active
    await connection.SendPttAsync(100, true);
}
else
{
    // Not connected - reconnect or handle error
    await connection.ConnectAsync();
}
```

### Keepalive Mechanism

The connection automatically sends keepalive packets every 10 seconds. If no response is received within 30 seconds, the `ConnectionLost` event is raised.

```csharp
connection.ConnectionLost += async (sender, args) =>
{
    Console.WriteLine("Connection lost, attempting reconnect...");
    
    await Task.Delay(5000); // Wait 5 seconds
    
    if (await connection.ConnectAsync())
    {
        Console.WriteLine("Reconnected successfully");
    }
};
```

### Graceful Disconnection

```csharp
// Always disconnect when done
await connection.DisconnectAsync();

// Or use Dispose pattern
using var connection = new HyteraConnection("192.168.1.100", 9000001);
// ... use connection ...
// Automatically disconnects on disposal
```

---

## Sending Commands

### PTT (Push-to-Talk)

Control radio transmission:

```csharp
// Press PTT (start transmission)
await connection.SendPttAsync(
    destinationId: 100,    // Talkgroup 100
    press: true,
    slot: 0                // Timeslot 1 (0-indexed)
);

// Transmit voice frames here...

// Release PTT (end transmission)
await connection.SendPttAsync(
    destinationId: 100,
    press: false,
    slot: 0
);
```

### GPS Request

Request GPS position from a radio:

```csharp
uint targetRadioId = 123456;

// Send GPS request
bool sent = await connection.RequestGpsAsync(targetRadioId);

if (sent)
{
    Console.WriteLine($"GPS request sent to radio {targetRadioId}");
    
    // Response will come via PacketReceived event
    // with Command = GPS_RESPONSE
}
```

### Text Messaging

Send text messages to radios:

```csharp
uint targetRadioId = 123456;
string message = "Hello from dispatcher!";

bool sent = await connection.SendTextMessageAsync(targetRadioId, message);

if (sent)
{
    Console.WriteLine($"Message sent to {targetRadioId}");
}
```

**Note**: Text messages are limited to approximately 144 characters. Longer messages may be truncated or rejected.

### Custom Packets

For advanced use cases, create custom packets:

```csharp
var packet = new HyteraIPSCPacket
{
    Command = HyteraCommand.STATUS_REQUEST,
    SourceId = 9000001,
    DestinationId = 123456,
    Slot = 0,
    Payload = new byte[] { 0x01, 0x02, 0x03 }
};

byte[] data = packet.ToBytes();
// Send via connection's internal stream
```

---

## Receiving Events

### Packet Received Event

All incoming packets trigger the `PacketReceived` event:

```csharp
connection.PacketReceived += (sender, packet) =>
{
    Console.WriteLine($"Command: {packet.Command}");
    Console.WriteLine($"Source: {packet.SourceId}");
    Console.WriteLine($"Destination: {packet.DestinationId}");
    Console.WriteLine($"Slot: {packet.Slot}");
    Console.WriteLine($"Payload Length: {packet.Payload.Length}");
};
```

### Handling Specific Events

```csharp
connection.PacketReceived += (sender, packet) =>
{
    switch (packet.Command)
    {
        case HyteraCommand.PTT_PRESS:
            Console.WriteLine($"Radio {packet.SourceId} pressed PTT");
            break;
            
        case HyteraCommand.PTT_RELEASE:
            Console.WriteLine($"Radio {packet.SourceId} released PTT");
            break;
            
        case HyteraCommand.GPS_RESPONSE:
            HandleGpsResponse(packet);
            break;
            
        case HyteraCommand.TEXT_MESSAGE_RECEIVE:
            string msg = Encoding.UTF8.GetString(packet.Payload);
            Console.WriteLine($"Message from {packet.SourceId}: {msg}");
            break;
            
        case HyteraCommand.EMERGENCY_DECLARE:
            Console.WriteLine($"EMERGENCY from radio {packet.SourceId}!");
            // Send acknowledgment
            await SendEmergencyAck(packet.SourceId);
            break;
            
        case HyteraCommand.VOICE_FRAME:
            ProcessVoiceFrame(packet);
            break;
    }
};
```

### GPS Response Parsing

```csharp
void HandleGpsResponse(HyteraIPSCPacket packet)
{
    if (packet.Payload.Length < 16)
    {
        Console.WriteLine("Invalid GPS data");
        return;
    }
    
    double latitude = BitConverter.ToDouble(packet.Payload, 0);
    double longitude = BitConverter.ToDouble(packet.Payload, 8);
    
    Console.WriteLine($"Radio {packet.SourceId} GPS:");
    Console.WriteLine($"  Latitude: {latitude}°");
    Console.WriteLine($"  Longitude: {longitude}°");
    
    // Optional fields
    if (packet.Payload.Length >= 24)
    {
        float altitude = BitConverter.ToSingle(packet.Payload, 16);
        Console.WriteLine($"  Altitude: {altitude}m");
    }
    
    if (packet.Payload.Length >= 28)
    {
        float speed = BitConverter.ToSingle(packet.Payload, 20);
        Console.WriteLine($"  Speed: {speed} km/h");
    }
    
    if (packet.Payload.Length >= 30)
    {
        ushort heading = BitConverter.ToUInt16(packet.Payload, 24);
        Console.WriteLine($"  Heading: {heading}°");
    }
}
```

### Voice Frame Processing

```csharp
void ProcessVoiceFrame(HyteraIPSCPacket packet)
{
    if (packet.Payload.Length < 33)
    {
        return; // Invalid voice frame
    }
    
    // Extract AMBE+2 data (first 33 bytes)
    byte[] ambeData = packet.Payload[0..33];
    
    // Extract metadata (if present)
    byte frameNumber = packet.Payload.Length > 33 ? packet.Payload[33] : (byte)0;
    bool isLastFrame = packet.Payload.Length > 34 && packet.Payload[34] == 1;
    
    Console.WriteLine($"Voice frame {frameNumber} from {packet.SourceId}");
    
    if (isLastFrame)
    {
        Console.WriteLine("End of transmission");
    }
    
    // Decode AMBE data (requires codec implementation)
    // var codec = new AmbeCodec();
    // byte[] pcmData = codec.DecodeToPcm(ambeData);
    // PlayAudio(pcmData);
}
```

---

## Error Handling

### Connection Errors

```csharp
try
{
    await connection.ConnectAsync();
}
catch (SocketException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
    // Check network connectivity, radio IP, firewall
}
catch (TimeoutException ex)
{
    Console.WriteLine($"Connection timeout: {ex.Message}");
    // Radio may be offline or unreachable
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

### Command Failures

```csharp
bool success = await connection.SendPttAsync(100, true);

if (!success)
{
    Console.WriteLine("PTT command failed");
    
    // Check connection status
    if (!connection.IsConnected)
    {
        Console.WriteLine("Not connected - reconnecting...");
        await connection.ConnectAsync();
    }
}
```

### Packet Parsing Errors

```csharp
connection.PacketReceived += (sender, packet) =>
{
    try
    {
        // Parse packet data
        ProcessPacket(packet);
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Invalid packet data: {ex.Message}");
        // Log packet for debugging
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing packet: {ex.Message}");
    }
};
```

---

## Best Practices

### 1. Use Logging

Enable comprehensive logging for debugging:

```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .AddDebug()
        .SetMinimumLevel(LogLevel.Debug); // Or LogLevel.Trace for packets
});

var logger = loggerFactory.CreateLogger<HyteraConnection>();
```

### 2. Handle Connection Loss

Always implement reconnection logic:

```csharp
private async Task MaintainConnection(HyteraConnection connection)
{
    int retryCount = 0;
    const int maxRetries = 5;
    
    connection.ConnectionLost += async (s, e) =>
    {
        while (retryCount < maxRetries && !connection.IsConnected)
        {
            retryCount++;
            int delay = Math.Min(1000 * (int)Math.Pow(2, retryCount), 30000);
            
            Console.WriteLine($"Reconnect attempt {retryCount}/{maxRetries} in {delay}ms");
            await Task.Delay(delay);
            
            if (await connection.ConnectAsync())
            {
                Console.WriteLine("Reconnected successfully");
                retryCount = 0;
                break;
            }
        }
        
        if (retryCount >= maxRetries)
        {
            Console.WriteLine("Max reconnection attempts reached");
        }
    };
}
```

### 3. Validate Packet Data

Always validate received data before processing:

```csharp
void ProcessGpsData(byte[] payload)
{
    if (payload == null || payload.Length < 16)
    {
        throw new ArgumentException("GPS payload too small");
    }
    
    double lat = BitConverter.ToDouble(payload, 0);
    double lon = BitConverter.ToDouble(payload, 8);
    
    if (lat < -90 || lat > 90)
    {
        throw new ArgumentException($"Invalid latitude: {lat}");
    }
    
    if (lon < -180 || lon > 180)
    {
        throw new ArgumentException($"Invalid longitude: {lon}");
    }
    
    // Process valid data...
}
```

### 4. Use Async/Await Properly

Always await async operations:

```csharp
// ✅ Good
await connection.SendPttAsync(100, true);
await Task.Delay(1000);
await connection.SendPttAsync(100, false);

// ❌ Bad - can cause deadlocks
connection.SendPttAsync(100, true).Wait();
```

### 5. Dispose Resources

Use `using` statements or manually dispose:

```csharp
// Option 1: using statement
using (var connection = new HyteraConnection("192.168.1.100", 9000001))
{
    await connection.ConnectAsync();
    // Use connection...
} // Automatically disposed

// Option 2: manual disposal
var connection = new HyteraConnection("192.168.1.100", 9000001);
try
{
    await connection.ConnectAsync();
    // Use connection...
}
finally
{
    connection.Dispose();
}
```

### 6. Thread Safety

The HyteraConnection class is thread-safe for most operations, but avoid concurrent calls to the same method:

```csharp
// ✅ Good - different methods
Task.Run(() => connection.SendPttAsync(100, true));
Task.Run(() => connection.RequestGpsAsync(123456));

// ❌ Bad - same method concurrently
Task.Run(() => connection.SendPttAsync(100, true));
Task.Run(() => connection.SendPttAsync(100, true)); // May cause issues
```

---

## Using with HyteraConnectionService

The `HyteraConnectionService` provides a higher-level interface:

```csharp
using HyteraGateway.Radio.Services;

var service = new HyteraConnectionService(logger);

// Configure connection
service.Configure(radioIp: "192.168.1.100", dispatcherId: 9000001);

// Subscribe to events
service.RadioEvent += (sender, radioEvent) =>
{
    Console.WriteLine($"Event: {radioEvent.EventType}");
    Console.WriteLine($"Radio: {radioEvent.RadioDmrId}");
    Console.WriteLine($"Data: {radioEvent.Data}");
};

// Connect
await service.ConnectAsync();

// Send commands
await service.SendPttAsync(dmrId: 100, press: true);
await service.SendTextMessageAsync(dmrId: 123456, "Hello!");
await service.RequestGpsAsync(dmrId: 123456);

// Get status
var status = await service.GetStatusAsync(dmrId: 123456);
Console.WriteLine($"Radio state: {status.State}");

// Disconnect
await service.DisconnectAsync();
```

---

## Testing

### Unit Tests

```csharp
[Fact]
public void TestPacketSerialization()
{
    var packet = HyteraIPSCPacket.CreatePttPress(9000001, 100, 0);
    byte[] data = packet.ToBytes();
    
    var parsed = HyteraIPSCPacket.FromBytes(data);
    
    Assert.Equal(HyteraCommand.PTT_PRESS, parsed.Command);
    Assert.Equal(9000001u, parsed.SourceId);
    Assert.Equal(100u, parsed.DestinationId);
}

[Fact]
public void TestCrcValidation()
{
    var packet = new DMRPacket
    {
        SourceId = 12345,
        DestinationId = 100,
        Type = PacketType.Voice
    };
    
    byte[] data = packet.ToBytes();
    bool valid = DMRPacket.ValidateCrc(data);
    
    Assert.True(valid);
}
```

### Integration Tests

See the ProtocolTester tool for integration testing examples.

---

## See Also

- [DMR Protocol](PROTOCOL/DMR_PROTOCOL.md)
- [Hytera IPSC Protocol](PROTOCOL/HYTERA_IPSC.md)
- [Packet Reference](PROTOCOL/PACKET_REFERENCE.md)
- [Protocol Tester Tool](../tools/ProtocolTester/README.md)
