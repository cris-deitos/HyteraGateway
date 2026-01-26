# HyteraGateway - Troubleshooting Guide

## Common Issues and Solutions

### Connection Issues

#### Problem: Cannot connect to radio

**Symptoms:**
- `ConnectAsync()` returns false
- Log shows "Failed to connect to radio"
- Connection timeout

**Solutions:**

1. **Check network connectivity:**
   ```bash
   ping 192.168.1.1  # Use your radio's IP
   telnet 192.168.1.1 50000  # Test TCP port
   ```

2. **Verify radio configuration:**
   - Ensure radio has IPSC/DMR protocol enabled
   - Check firewall settings on both sides
   - Verify radio is not already connected to another dispatcher

3. **Check configuration:**
   - Verify IP address and port in `appsettings.json` or `RadioController.xml`
   - Ensure DispatcherId is correctly set

4. **Enable debug logging:**
   ```json
   {
     "Logging": {
       "LogLevel": {
         "HyteraGateway.Radio": "Debug"
       }
     }
   }
   ```

#### Problem: Connection drops frequently

**Symptoms:**
- `ConnectionLost` event fires repeatedly
- Auto-reconnect cycles continuously
- Log shows keepalive timeout

**Solutions:**

1. **Check network stability:**
   ```bash
   ping -c 100 192.168.1.1  # Check for packet loss
   ```

2. **Increase keepalive tolerance:**
   - Modify `HyteraConnection.cs` keepalive timeout (default 30 seconds)

3. **Check radio load:**
   - Ensure radio is not overloaded with traffic
   - Check radio logs for errors

4. **Network issues:**
   - Look for WiFi interference
   - Check network switch/router logs
   - Verify cable quality

#### Problem: Auto-reconnect fails

**Symptoms:**
- `ReconnectFailed` event fires
- Gateway gives up after 10 attempts
- Cannot re-establish connection

**Solutions:**

1. **Manual reconnection:**
   ```csharp
   await connection.DisconnectAsync();
   await connection.ConnectAsync();
   ```

2. **Check reconnect configuration:**
   - Ensure `AutoReconnect = true`
   - Verify `MaxReconnectAttempts` setting

3. **Investigate root cause:**
   - Check logs for specific errors
   - Verify radio is still accessible
   - Check for IP address changes

### Packet Validation Issues

#### Problem: "Invalid signature" errors

**Symptoms:**
- Log shows "Invalid signature" errors
- Packets rejected by validator
- Communication fails

**Solutions:**

1. **Verify radio protocol:**
   - Ensure radio is using IPSC protocol (not DMRPlus, Hytera XPT, etc.)
   - Check radio firmware version compatibility

2. **Network issues:**
   - Look for packet corruption (bad cable, interference)
   - Check for MTU issues
   - Verify no firewall/proxy is modifying packets

3. **Debug packet data:**
   ```csharp
   var result = HyteraPacketValidator.Validate(data);
   logger.LogDebug("Validation: {Severity} - {Message}", 
       result.Severity, result.ErrorMessage);
   ```

#### Problem: CRC mismatch errors

**Symptoms:**
- Log shows "CRC mismatch" errors
- Packets fail validation
- Random communication failures

**Solutions:**

1. **Network quality:**
   - Check for electromagnetic interference
   - Test with different network cable
   - Verify network adapter settings (no offloading)

2. **Software issues:**
   - Update to latest gateway version
   - Check for known bugs in radio firmware

3. **Temporary workaround (not recommended):**
   - Disable CRC validation (only for debugging)
   - This indicates a serious problem that should be fixed

### Recording Issues

#### Problem: Recordings not created

**Symptoms:**
- No WAV files in recordings directory
- CallRecorder not logging activity
- Metadata JSON missing

**Solutions:**

1. **Check recording configuration:**
   ```json
   {
     "Recording": {
       "Enabled": true,
       "StoragePath": "./recordings/"
     }
   }
   ```

2. **Verify directory permissions:**
   ```bash
   ls -ld ./recordings/
   chmod 755 ./recordings/  # If needed
   ```

3. **Check disk space:**
   ```bash
   df -h  # Verify available disk space
   ```

4. **Enable debug logging:**
   - Look for errors in CallRecorder
   - Check if frames are being received

#### Problem: FTP upload fails

**Symptoms:**
- Recordings created but not uploaded
- FTP errors in logs
- Files remain on local disk

**Solutions:**

1. **Test FTP connection:**
   ```bash
   ftp ftp.example.com
   # Test login manually
   ```

2. **Check FTP configuration:**
   ```json
   {
     "Ftp": {
       "Host": "ftp.example.com",
       "Port": 21,
       "Username": "user",
       "Password": "pass",
       "UsePassive": true
     }
   }
   ```

3. **Firewall issues:**
   - Passive mode: Ensure high ports (>1024) are open
   - Active mode: Ensure server can connect back to client

4. **Credentials:**
   - Verify username/password
   - Check FTP account permissions
   - Test with FileZilla or similar tool

### PTT Timeout Issues

#### Problem: PTT automatically releases too quickly

**Symptoms:**
- PTT releases before user is done talking
- Timeout warnings in log

**Solutions:**

1. **Increase timeout:**
   ```json
   {
     "PttTimeout": {
       "TimeoutSeconds": 300  // 5 minutes
     }
   }
   ```

2. **Check if stuck PTT:**
   - Verify PTT release commands are being sent
   - Check for hardware issues with PTT button

#### Problem: Stuck PTT not releasing

**Symptoms:**
- PTT remains active indefinitely
- Radio continues transmitting
- No timeout warning

**Solutions:**

1. **Verify PttTimeoutService is running:**
   - Check logs for timeout service startup
   - Ensure service is registered in DI container

2. **Manual PTT release:**
   ```csharp
   await radioService.SendPttAsync(talkGroupId, false);
   ```

3. **Check timeout configuration:**
   - Ensure timeout is not set too high
   - Verify timer is running (logs should show periodic checks)

### RadioServer Issues

#### Problem: Clients cannot connect to RadioServer

**Symptoms:**
- Dispatcher applications fail to connect
- Port 8000 not listening
- Connection refused errors

**Solutions:**

1. **Verify server is running:**
   ```bash
   netstat -an | grep 8000
   # or
   ss -tln | grep 8000
   ```

2. **Check configuration:**
   ```json
   {
     "RadioServer": {
       "Enabled": true,
       "Port": 8000
     }
   }
   ```

3. **Firewall:**
   ```bash
   # Linux (ufw)
   sudo ufw allow 8000/tcp
   
   # Linux (iptables)
   sudo iptables -A INPUT -p tcp --dport 8000 -j ACCEPT
   
   # Windows
   # Add inbound rule in Windows Firewall
   ```

4. **Binding issues:**
   - Check if another application is using port 8000
   - Try different port
   - Ensure binding to correct interface (0.0.0.0 for all)

#### Problem: Events not broadcast to clients

**Symptoms:**
- Clients connected but not receiving updates
- Radio events not propagating
- One-way communication

**Solutions:**

1. **Check client implementation:**
   - Verify client is reading response packets
   - Check for buffer/timeout issues

2. **Enable debug logging:**
   - Log all outgoing events
   - Verify broadcast logic is triggered

3. **Network issues:**
   - Check for client-side firewall blocking responses
   - Verify TCP connection is fully established

### Database Issues

#### Problem: Database connection fails

**Symptoms:**
- Cannot save call records
- Entity Framework errors
- Database timeout

**Solutions:**

1. **Verify MySQL is running:**
   ```bash
   systemctl status mysql
   # or
   systemctl status mariadb
   ```

2. **Test connection:**
   ```bash
   mysql -h localhost -u root -p
   ```

3. **Check connection string:**
   ```json
   {
     "Database": {
       "Host": "localhost",
       "Port": 3306,
       "Database": "easyvol",
       "Username": "root",
       "Password": "yourpassword"
     }
   }
   ```

4. **Verify database exists:**
   ```sql
   SHOW DATABASES;
   USE easyvol;
   SHOW TABLES;
   ```

5. **Run migrations:**
   ```bash
   dotnet ef database update
   ```

### Performance Issues

#### Problem: High CPU usage

**Symptoms:**
- CPU at 100%
- System slowness
- Log shows performance warnings

**Solutions:**

1. **Check logging level:**
   - Reduce logging verbosity in production
   - Avoid Trace level logging

2. **Optimize database queries:**
   - Add indexes to frequently queried columns
   - Use connection pooling

3. **Review background tasks:**
   - Increase monitoring intervals
   - Disable unnecessary features

#### Problem: High memory usage

**Symptoms:**
- Memory continuously growing
- OutOfMemoryException
- System swapping

**Solutions:**

1. **Check for memory leaks:**
   - Review Dispose() implementations
   - Ensure event handlers are unsubscribed

2. **Optimize recording:**
   - Reduce recording quality
   - Clean up old recordings
   - Increase FTP upload frequency

3. **Reduce buffer sizes:**
   - Adjust packet buffer sizes
   - Limit concurrent recordings

## Debugging Tips

### Enable Verbose Logging

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "HyteraGateway": "Debug",
      "HyteraGateway.Radio.Protocol": "Trace"
    }
  }
}
```

### Packet Inspection

Use the PacketInspector tool to analyze raw packets:

```bash
cd tools/PacketInspector
dotnet run -- --file packet.bin
```

### Network Monitoring

Capture packets with tcpdump or Wireshark:

```bash
# Capture radio traffic
sudo tcpdump -i eth0 -w capture.pcap port 50000

# Capture RadioServer traffic
sudo tcpdump -i any -w capture.pcap port 8000
```

### Health Checks

Monitor system health:

```bash
# Check process status
ps aux | grep HyteraGateway

# Check resource usage
top -p $(pgrep -f HyteraGateway)

# Check file descriptors
lsof -p $(pgrep -f HyteraGateway)

# Check disk I/O
iostat -x 1 10
```

### Test Mode

Run in test mode with mock radio:

```bash
dotnet run --project tools/ProtocolTester
```

## Getting Help

### Log Collection

When reporting issues, provide:

1. **Full logs** with Debug or Trace level:
   ```bash
   journalctl -u hyteragateway.service > logs.txt
   ```

2. **Configuration files** (sanitize passwords):
   - appsettings.json
   - RadioController.xml

3. **System information:**
   ```bash
   uname -a
   dotnet --info
   free -h
   df -h
   ```

4. **Network information:**
   ```bash
   ip addr show
   ip route show
   ss -s
   ```

### Support Channels

- GitHub Issues: https://github.com/cris-deitos/HyteraGateway/issues
- Documentation: https://github.com/cris-deitos/HyteraGateway/docs

### Known Limitations

- Single radio connection per gateway instance
- AMBE codec requires external library (mbelib)
- No built-in web UI (use API or RadioServer protocol)
- FTP upload is synchronous (may block on slow networks)
- No HTTPS support for RadioServer (TCP only)

## Preventive Maintenance

### Regular Tasks

1. **Clean old recordings:**
   ```bash
   find ./recordings -mtime +30 -delete
   ```

2. **Rotate logs:**
   - Configure log rotation in systemd or logrotate

3. **Monitor disk space:**
   - Set up alerts for low disk space
   - Archive old recordings

4. **Database maintenance:**
   ```sql
   OPTIMIZE TABLE calls;
   OPTIMIZE TABLE gps_positions;
   OPTIMIZE TABLE events;
   ```

5. **Update dependencies:**
   ```bash
   dotnet list package --outdated
   dotnet add package <PackageName>
   ```

### Health Monitoring

Set up monitoring for:
- Radio connection status
- Database connection
- Disk space usage
- CPU and memory usage
- Recording failures
- FTP upload failures

### Backup Strategy

1. **Configuration files:**
   - appsettings.json
   - RadioController.xml

2. **Database:**
   ```bash
   mysqldump -u root -p easyvol > backup.sql
   ```

3. **Recordings:**
   - Archive to cold storage
   - Verify FTP uploads succeeded

4. **Test restores:**
   - Regularly test backup restoration
   - Document recovery procedures
