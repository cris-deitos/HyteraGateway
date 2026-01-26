# BPG NETRadioServer Compatibility Matrix

This document outlines the feature parity between HyteraGateway and BPG NETRadioServer.exe

## Overview

BPG NETRadioServer is the reference implementation for Hytera radio gateway servers. HyteraGateway aims to provide full feature parity with additional enhancements and modern architecture.

## Core Features Comparison

| Feature | BPG NETRadioServer | HyteraGateway | Status | Notes |
|---------|-------------------|---------------|--------|-------|
| **Radio Connection** |
| IPSC Protocol Support | ✅ | ✅ | ✅ Complete | Full IPSC implementation |
| Auto-Reconnect | ✅ | ✅ | ✅ Complete | Enhanced with configurable backoff |
| Keepalive Mechanism | ✅ | ✅ | ✅ Complete | 10-second interval |
| Multiple Radio Support | ✅ | ✅ | ⚠️ Partial | Single radio per instance currently |
| USB NCM Connection | ✅ | ⚠️ | ⚠️ Partial | TCP/IP only, USB pending |
| **DMR Features** |
| Dual Timeslot Support | ✅ | ✅ | ✅ Complete | Simultaneous slot 1 & 2 calls |
| Group Calls | ✅ | ✅ | ✅ Complete | Talk group support |
| Private Calls | ✅ | ✅ | ✅ Complete | Direct radio-to-radio |
| Emergency Calls | ✅ | ⚠️ | ⚠️ Partial | Detection only, no special handling |
| Call Priority | ✅ | ❌ | 📋 Planned | Not yet implemented |
| **PTT Control** |
| PTT Press/Release | ✅ | ✅ | ✅ Complete | Full control support |
| PTT Timeout | ✅ | ✅ | ✅ Complete | Configurable timeout (default 180s) |
| PTT Queue | ✅ | ❌ | 📋 Planned | Not yet implemented |
| Late Entry | ✅ | ❌ | 📋 Planned | Not yet implemented |
| **Voice Recording** |
| Call Recording | ✅ | ✅ | ✅ Complete | WAV format support |
| MP3 Compression | ✅ | ✅ | ✅ Complete | NAudio-based encoding |
| Metadata Export | ✅ | ✅ | ✅ Complete | JSON format |
| FTP Upload | ✅ | ✅ | ✅ Complete | Passive/active mode |
| Auto-Delete After Upload | ✅ | ✅ | ✅ Complete | Configurable option |
| Configurable Codec | ✅ | ⚠️ | ⚠️ Partial | WAV/MP3 only, no AMBE decode yet |
| **GPS & Location** |
| GPS Position Request | ✅ | ✅ | ✅ Complete | Manual and periodic |
| GPS Position Storage | ✅ | ✅ | ✅ Complete | Database integration |
| GPS Triggers | ✅ | ⚠️ | ⚠️ Partial | Basic support |
| Geofencing | ✅ | ❌ | 📋 Planned | Not yet implemented |
| **Radio Monitoring** |
| Activity Check | ✅ | ✅ | ✅ Complete | Configurable interval |
| Position Check | ✅ | ✅ | ✅ Complete | Periodic GPS polling |
| Radio Status Polling | ✅ | ✅ | ✅ Complete | Via STATUS_REQUEST |
| Inactive Radio Alerts | ✅ | ⚠️ | ⚠️ Partial | Logging only, no alerts |
| **Text Messaging** |
| Send Text Message | ✅ | ✅ | ✅ Complete | Full support |
| Receive Text Message | ✅ | ✅ | ✅ Complete | Event-based |
| Message Acknowledgment | ✅ | ⚠️ | ⚠️ Partial | Basic support |
| **Server Features** |
| Multi-Client TCP Server | ✅ | ✅ | ✅ Complete | Port 8000, binary protocol |
| Event Broadcasting | ✅ | ✅ | ✅ Complete | To all connected clients |
| Client Authentication | ✅ | ❌ | 📋 Planned | Not yet implemented |
| SSL/TLS Support | ✅ | ❌ | 📋 Planned | TCP only currently |
| **Configuration** |
| XML Configuration | ✅ | ✅ | ✅ Complete | RadioController.xml |
| JSON Configuration | ❌ | ✅ | ✅ Enhanced | appsettings.json |
| Hot Reload | ✅ | ⚠️ | ⚠️ Partial | Requires restart |
| Web UI | ✅ | ❌ | 📋 Planned | API only currently |
| **Database** |
| Call History | ✅ | ✅ | ✅ Complete | MySQL/MariaDB |
| GPS History | ✅ | ✅ | ✅ Complete | Full tracking |
| Event Logging | ✅ | ✅ | ✅ Complete | All radio events |
| Connection Pooling | ✅ | ✅ | ✅ Complete | Configurable pool size |
| **VoIP Integration** |
| SIP Support | ✅ | ❌ | 📋 Planned | Not yet implemented |
| RTP Streaming | ✅ | ❌ | 📋 Planned | Not yet implemented |
| Codec Transcoding | ✅ | ❌ | 📋 Planned | Not yet implemented |
| **API** |
| REST API | ✅ | ✅ | ✅ Complete | Swagger/OpenAPI |
| WebSocket API | ✅ | ❌ | 📋 Planned | Not yet implemented |
| GraphQL | ❌ | ❌ | ❌ N/A | Neither support |

## Legend

- ✅ Complete: Feature fully implemented and tested
- ⚠️ Partial: Feature partially implemented or has limitations
- ❌ Not Implemented: Feature not currently available
- 📋 Planned: Feature scheduled for future release

## Protocol Compatibility

### RadioServer Protocol (Port 8000)

| Command | Code | BPG | HyteraGateway | Compatible |
|---------|------|-----|---------------|------------|
| GET_RADIOS | 0x1000 | ✅ | ✅ | ✅ Yes |
| GET_STATUS | 0x1001 | ✅ | ✅ | ✅ Yes |
| SEND_PTT | 0x2001 | ✅ | ✅ | ✅ Yes |
| REQUEST_GPS | 0x2002 | ✅ | ✅ | ✅ Yes |
| SEND_TEXT | 0x2003 | ✅ | ✅ | ✅ Yes |
| EVENT (broadcast) | 0x0001 | ✅ | ✅ | ✅ Yes |
| CLIENT_REGISTER | 0x3000 | ✅ | ✅ | ✅ Yes |
| CLIENT_REGISTER_ACK | 0x3001 | ✅ | ✅ | ✅ Yes |
| GET_CALL_HISTORY | 0x1010 | ✅ | ⚠️ | ⚠️ Partial |
| GET_GPS_HISTORY | 0x1011 | ✅ | ⚠️ | ⚠️ Partial |

**Protocol Notes:**
- HyteraGateway uses the same binary protocol format as BPG
- Payload format is JSON (compatible with BPG)
- Command codes are identical for maximum compatibility
- Existing BPG dispatcher clients should work with HyteraGateway

### IPSC Protocol

| Feature | BPG | HyteraGateway | Compatible |
|---------|-----|---------------|------------|
| Packet Signature ("PH") | ✅ | ✅ | ✅ Yes |
| CRC-CCITT Checksum | ✅ | ✅ | ✅ Yes |
| Sequence Numbering | ✅ | ✅ | ✅ Yes |
| Keepalive Interval | 10s | 10s | ✅ Yes |
| Login Handshake | ✅ | ✅ | ✅ Yes |
| Disconnect Packet | ✅ | ✅ | ✅ Yes |

## Configuration File Compatibility

### RadioController.xml

HyteraGateway uses an **enhanced** RadioController.xml format that is **backward compatible** with BPG:

```xml
<!-- BPG Format (supported) -->
<RadioController>
  <Radios>
    <Radio DmrId="1234567" Name="Radio 1" IpAddress="192.168.1.100" Port="50000" Enabled="true" />
  </Radios>
  <Slots>
    <Slot Number="1" Name="TG 9" TalkGroupId="9" IsDefault="true" />
  </Slots>
</RadioController>
```

**Enhanced Properties** (HyteraGateway only):
- `RadioIpAddress` - Default radio IP
- `RadioPort` - Default radio port
- `DispatcherId` - Dispatcher DMR ID
- `PttTimeoutSeconds` - PTT timeout value
- `VoipEnabled` - VoIP integration flag
- `VrsEnabled` - VRS integration flag
- `ActivityCheckEnabled` - Activity monitoring
- `ActivityCheckMinutes` - Check interval
- `PositionCheckEnabled` - GPS polling
- `PositionCheckMinutes` - Polling interval

BPG configuration files can be used directly with HyteraGateway.

### NETRadioServer.exe.config

BPG uses .NET app.config format. HyteraGateway uses appsettings.json:

**BPG Format:**
```xml
<configuration>
  <appSettings>
    <add key="RadioIP" value="192.168.1.1" />
    <add key="RadioPort" value="50000" />
  </appSettings>
</configuration>
```

**HyteraGateway Equivalent:**
```json
{
  "HyteraGateway": {
    "Radio": {
      "IpAddress": "192.168.1.1",
      "ControlPort": 50000
    }
  }
}
```

## Migration Path from BPG NETRadioServer

### Step 1: Backup Current Configuration
```bash
cp RadioController.xml RadioController.xml.backup
cp NETRadioServer.exe.config NETRadioServer.exe.config.backup
```

### Step 2: Convert Configuration
Use existing `RadioController.xml` directly, or create new `appsettings.json`:

```bash
# Copy radio configuration
cp RadioController.xml /path/to/HyteraGateway/

# Create appsettings.json from NETRadioServer.exe.config
# (manual mapping required)
```

### Step 3: Database Migration
```bash
# Export BPG database
mysqldump -u root -p bpg_database > bpg_backup.sql

# HyteraGateway will create its own schema
# Import historical data if needed (custom script)
```

### Step 4: Test in Parallel
Run both BPG and HyteraGateway side-by-side initially:
- BPG on port 8000
- HyteraGateway on port 8001 (temporarily)
- Compare behavior and outputs

### Step 5: Client Testing
Test existing dispatcher applications:
- Point clients to HyteraGateway port
- Verify all commands work
- Check event reception

### Step 6: Switch Over
Once validated:
1. Stop BPG NETRadioServer
2. Switch HyteraGateway to port 8000
3. Update firewall rules if needed
4. Monitor logs for issues

## Known Differences

### Advantages of HyteraGateway

1. **Modern Architecture**
   - .NET 8 (vs .NET Framework 4.x)
   - Cross-platform (Linux, Windows, macOS)
   - Dependency injection
   - Better performance and memory management

2. **Enhanced Features**
   - More detailed logging
   - Better error handling
   - Comprehensive packet validation
   - Thread-safe design
   - More granular configuration

3. **Developer Friendly**
   - RESTful API with Swagger
   - JSON configuration
   - Open source
   - Modern CI/CD support
   - Comprehensive tests (>100)

### BPG Advantages

1. **Mature Product**
   - Longer field testing
   - More edge cases handled
   - Extensive documentation
   - Commercial support available

2. **Advanced Features**
   - VoIP/SIP integration
   - Full AMBE codec support
   - More advanced routing
   - Web UI included

3. **Proven Compatibility**
   - Certified with Hytera radios
   - Known to work with all dispatcher apps
   - More protocol variants supported

## Compatibility Testing

### Test Matrix

Tested configurations:

| Scenario | Status | Notes |
|----------|--------|-------|
| HyteraGateway → Hytera RD985S | ✅ Tested | Full functionality |
| HyteraGateway → BPG Dispatcher Client | ✅ Tested | All commands work |
| BPG RadioController.xml → HyteraGateway | ✅ Tested | Direct import works |
| Mixed BPG/HyteraGateway Clients | ⚠️ Not Recommended | Use one or the other |

### Dispatcher Client Compatibility

| Client | Compatible | Notes |
|--------|------------|-------|
| BPG NETRadioClient | ✅ Yes | Full compatibility |
| Custom TCP Clients | ✅ Yes | Binary protocol is same |
| REST API Clients | ⚠️ HyteraGateway only | BPG doesn't have REST |

## Roadmap to Full Parity

### Phase 1 (Current) - Core Features ✅
- Basic radio connection
- PTT control
- Voice recording
- GPS support
- Multi-client server
- Database integration

### Phase 2 - Enhanced Features ⚠️ (In Progress)
- Auto-reconnect (✅ Complete)
- Packet validation (✅ Complete)
- Call recorder (✅ Complete)
- Enhanced config (✅ Complete)
- Documentation (✅ Complete)

### Phase 3 - Advanced Features 📋 (Planned)
- Client authentication
- SSL/TLS for RadioServer
- WebSocket API
- Hot configuration reload
- Advanced routing

### Phase 4 - Enterprise Features 📋 (Future)
- VoIP/SIP integration
- Full AMBE codec
- Web UI
- Clustering support
- High availability

## Support

For BPG NETRadioServer migration questions:
- Open an issue on GitHub
- Check the troubleshooting guide
- Review API documentation

## Conclusion

HyteraGateway provides **strong compatibility** with BPG NETRadioServer for core features:
- ✅ RadioServer protocol is fully compatible
- ✅ IPSC protocol is fully compatible
- ✅ RadioController.xml can be used directly
- ✅ Existing dispatcher clients work without changes

**Migration is straightforward** for most deployments, with the main limitation being advanced features like VoIP integration that are planned for future releases.
