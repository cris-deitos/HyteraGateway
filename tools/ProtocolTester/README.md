# Protocol Tester

A CLI tool for testing Hytera radio protocol communication.

## Usage

```bash
ProtocolTester [command] --radio-ip <ip> --dispatcher-id <id> [options]
```

## Commands

- **connect** - Test connection and keepalive
- **ptt** - Test PTT press/release
- **gps** - Test GPS request
- **sms** - Test text messaging
- **interactive** - Interactive mode for manual testing

## Options

### Required
- `--radio-ip <ip>` - Radio IP address
- `--dispatcher-id <id>` - Dispatcher DMR ID

### Optional
- `--port <port>` - Port (default: 50000)
- `--target-id <id>` - Target radio ID for commands
- `--message <text>` - Message text for SMS
- `--verbose, -v` - Enable verbose logging
- `--help, -h` - Show help

## Examples

### Test Connection
```bash
dotnet run --project tools/ProtocolTester -- connect \
  --radio-ip 192.168.1.100 \
  --dispatcher-id 9000001 \
  --verbose
```

### Test PTT
```bash
dotnet run --project tools/ProtocolTester -- ptt \
  --radio-ip 192.168.1.100 \
  --dispatcher-id 9000001 \
  --target-id 100 \
  --verbose
```

### Request GPS
```bash
dotnet run --project tools/ProtocolTester -- gps \
  --radio-ip 192.168.1.100 \
  --dispatcher-id 9000001 \
  --target-id 123456
```

### Send SMS
```bash
dotnet run --project tools/ProtocolTester -- sms \
  --radio-ip 192.168.1.100 \
  --dispatcher-id 9000001 \
  --target-id 123456 \
  --message "Hello from dispatcher"
```

### Interactive Mode
```bash
dotnet run --project tools/ProtocolTester -- interactive \
  --radio-ip 192.168.1.100 \
  --dispatcher-id 9000001
```

In interactive mode, you can use these commands:
- `ptt <target_id>` - Press and release PTT
- `gps <target_id>` - Request GPS position
- `sms <target_id> <message>` - Send text message
- `quit` or `exit` - Exit interactive mode

## Building

```bash
dotnet build tools/ProtocolTester/ProtocolTester.csproj
```

## Publishing

```bash
dotnet publish tools/ProtocolTester/ProtocolTester.csproj -c Release -o ./publish
```

Then run with:
```bash
./publish/ProtocolTester connect --radio-ip 192.168.1.100 --dispatcher-id 9000001
```

## Notes

- The tool requires network connectivity to the radio
- Default port is 50000 (Hytera IPSC standard)
- Use `--verbose` flag to see detailed packet logging
- The connection test includes keepalive verification
- All commands handle connection setup and teardown automatically
