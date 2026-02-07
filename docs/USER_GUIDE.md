# HyteraGateway User Guide

## Table of Contents
1. [Introduction](#introduction)
2. [Installation](#installation)
3. [Getting Started](#getting-started)
4. [User Interface Overview](#user-interface-overview)
5. [Dashboard](#dashboard)
6. [Statistics](#statistics)
7. [Logs](#logs)
8. [Settings](#settings)
9. [Tools](#tools)
10. [Troubleshooting](#troubleshooting)

## Introduction

HyteraGateway is a comprehensive Windows application for managing and monitoring Hytera DMR radios. It provides real-time call monitoring, audio recording, GPS tracking, and diagnostic tools for radio fleet management.

### Key Features
- **Real-time Monitoring**: Track radio activity, calls, and events in real-time
- **USB & Ethernet Support**: Connect via USB (NCM/RNDIS) or direct Ethernet
- **Network Discovery**: Automatically detect radios on your network
- **Audio Recording**: Record and manage radio transmissions
- **Statistics & Analytics**: View call statistics with interactive charts
- **Diagnostic Tools**: Built-in network tools for troubleshooting
- **Multi-Radio Support**: Manage multiple radios simultaneously

### Supported Radio Models

#### USB Connection (NCM/RNDIS)
- Hytera MD785i - Mobile radio with USB port
- Hytera MD785G - Mobile radio with GPS and USB
- Other Hytera DMR radios with USB data cable

#### Ethernet Connection (Direct IP)
- Hytera HM785 - Mobile radio with built-in Ethernet
- Hytera HR1065 - Repeater with network interface
- Other Hytera devices with RJ45 Ethernet port

## Installation

### System Requirements
- **Operating System**: Windows 10 or Windows 11 (64-bit)
- **RAM**: Minimum 4 GB, recommended 8 GB or more
- **.NET Runtime**: .NET 8.0 Runtime (included with installer)
- **MySQL**: MySQL 8.0 or later (for database features)
- **Disk Space**: 500 MB for application, additional space for recordings

### Installation Steps

1. **Download the Installer**
   - Download the latest release from the GitHub releases page
   - Choose `HyteraGateway-Setup-x.x.x.exe`

2. **Run the Installer**
   - Double-click the installer executable
   - Follow the installation wizard
   - Accept the license agreement
   - Choose installation directory (default: `C:\Program Files\HyteraGateway`)
   - Select components (UI, Service, Documentation)

3. **Install Dependencies**
   - The installer will automatically install .NET 8.0 Runtime if not present
   - MySQL must be installed separately if you want database features

4. **First Launch**
   - Launch HyteraGateway from the Start Menu or Desktop shortcut
   - The application will create default configuration files on first run

## Getting Started

### Quick Setup Wizard

On first launch, you'll be guided through initial setup:

1. **Connection Type Selection**
   - Choose **USB** if you're using a USB cable connection (MD785i)
   - Choose **Ethernet** if your radio has built-in Ethernet (HM785)
   - Choose **Auto** to let the application detect the best method

2. **Radio Discovery**
   - For USB: The application will detect USB network interfaces automatically
   - For Ethernet: Scan your network or enter the radio IP address manually

3. **Basic Configuration**
   - Set your Dispatcher ID (default: 1)
   - Configure control and audio ports (defaults: 50000 and 50001)
   - Enable/disable audio recording

4. **Test Connection**
   - Click "Test Connection" to verify radio connectivity
   - If successful, you're ready to start monitoring

### Connecting Your First Radio

#### USB Connection (MD785i)
1. Connect the radio to your computer via USB cable
2. Wait for Windows to recognize the USB network interface
3. Go to **Settings** → **Radio** tab
4. Select "USB" as connection type
5. Click "Refresh USB Interfaces"
6. Select the detected interface (usually shows "Remote NDIS" or "NCM")
7. The gateway IP address will be auto-filled (typically 192.168.1.1 or 192.168.42.1)
8. Click "Test Connection"
9. Click "Save" when connection is successful

#### Ethernet Connection (HM785)
1. Connect the radio to your network via Ethernet cable
2. Ensure the radio has an IP address (DHCP or static)
3. Go to **Settings** → **Radio** tab
4. Select "Ethernet" as connection type
5. Enter the radio's IP address manually, or:
   - Enter your subnet (e.g., "192.168.1")
   - Click "Scan" to discover radios on the network
   - Select the discovered radio and click "Use Selected Radio"
6. Click "Test Connection"
7. Click "Save" when connection is successful

## User Interface Overview

The HyteraGateway UI is organized into five main sections accessible from the left navigation panel:

1. **Dashboard** - Overview of connected radios and current status
2. **Statistics** - Call analytics with charts and reports
3. **Logs** - Real-time application and radio logs
4. **Settings** - Application and radio configuration
5. **Tools** - Diagnostic and troubleshooting utilities

### Navigation
- Click any icon in the left sidebar to switch views
- Icons show tooltips on hover for quick reference
- The active view is highlighted in the navigation

### Theme
- The application uses Material Design with a modern dark theme
- Primary colors: Blue (#2196F3), Green (#4CAF50), Orange (#FF9800), Red (#F44336)

## Dashboard

The Dashboard provides a real-time overview of your radio fleet and system status.

### Radio Status Cards
Each connected radio is displayed as a card showing:
- **Radio Model** and IP Address
- **Connection Status** (Connected/Disconnected)
- **DMR ID** and Channel
- **Signal Strength** indicator
- **PTT Status** (Transmitting/Receiving/Idle)
- **Battery Level** (if supported)
- **GPS Position** (if available)

### Quick Actions
- **Connect/Disconnect**: Toggle radio connection
- **Request GPS**: Get current GPS position
- **View Details**: Open detailed radio information

### System Status Bar (Bottom)
- **API Status**: Shows if the backend API is running
- **Database Status**: Shows database connection status
- **Recording Status**: Indicates if audio recording is active
- **Uptime**: Application running time

## Statistics

The Statistics view provides comprehensive call analytics and reports.

### Summary Cards
At the top of the view, three cards display key metrics:
- **Total Calls**: Number of calls in the selected period
- **Total Duration**: Combined duration of all calls
- **Active Radios**: Number of radios that made calls

### Calls Per Day Chart
Line chart showing call volume over time:
- X-axis: Dates
- Y-axis: Number of calls
- Hover over points to see exact values
- Use the date range selector to adjust the time period

### Call Type Distribution
Pie chart showing breakdown by call type:
- **Private Calls** (Green): One-to-one calls
- **Group Calls** (Blue): Group communication
- **Emergency** (Red): Emergency calls

### Date Range Selector
- **From Date**: Start of reporting period
- **To Date**: End of reporting period
- Click **Refresh** button to update statistics
- Default range: Last 7 days

### Export Options
- **Export to Excel**: Download statistics as spreadsheet
- Includes all visible data and charts

## Logs

The Logs view displays real-time application and radio event logs.

### Log Display
- Shows logs in a scrollable data grid
- Columns: Timestamp, Level, Source, Message
- Auto-scrolls to newest logs when enabled
- Color-coded by severity level:
  - **Error** (Red): Critical issues requiring attention
  - **Warning** (Orange): Important but non-critical issues
  - **Info** (Blue): General information messages
  - **Debug** (Gray): Detailed debugging information

### Filtering
- **Search Box**: Filter logs by message or source text
- **Level Filter**: Show only specific log levels
  - All: Show all log levels
  - Error: Show only errors
  - Warning: Show warnings and errors
  - Info: Show Info, Warning, and Error
  - Debug: Show all logs including debug

### Controls
- **Auto-scroll Toggle**: Enable/disable automatic scrolling to new logs
- **Clear Logs**: Remove all logs from the display (doesn't delete log files)
- **Export Logs**: Save logs to file
  - Choose TXT format for plain text
  - Choose CSV format for Excel import
  - Filename includes timestamp automatically

### Tips
- Clear logs periodically to maintain performance
- Use filters to focus on specific issues
- Export logs before clearing if you need to keep them
- The log display is limited to the most recent 1000 entries for performance

## Settings

Configure all aspects of the application in the Settings view.

### Radio Tab
Configure radio connection settings:

**Connection Type**
- USB: For radios with USB cable
- Ethernet: For radios with built-in Ethernet
- Auto: Automatically detect connection type

**USB Mode Settings**
- **Network Interfaces**: Select detected USB interface
- **Refresh**: Re-scan for USB interfaces
- Interface shows name, IP, gateway, and type

**Ethernet Mode Settings**
- **Scan Subnet**: Enter subnet to scan (e.g., "192.168.1")
- **Scan Button**: Discover radios on the network
- **Discovered Radios**: Shows found radios with IP, model, and status
- **Use Selected Radio**: Apply selected radio's settings

**Connection Parameters**
- **Radio IP Address**: IP address of the radio
- **Control Port**: Port for control protocol (default: 50000)
- **Audio Port**: Port for audio streaming (default: 50001)
- **Dispatcher ID**: Your dispatcher station ID (default: 1)
- **Auto-reconnect**: Automatically reconnect if connection is lost

**Actions**
- **Test Connection**: Verify radio connectivity

### Database Tab
Configure MySQL database connection:

- **Host**: Database server hostname or IP (default: localhost)
- **Port**: MySQL port (default: 3306)
- **Database Name**: Name of database (default: easyvol)
- **Username**: MySQL username (default: root)
- **Password**: MySQL password (masked input)

**Actions**
- **Test Connection**: Verify database connectivity

### API Tab
Configure REST API settings:

- **Base URL**: API server address (default: http://localhost:5000)
- **Port**: API port number (default: 5000)
- **Enable HTTPS**: Use secure HTTPS connection
- **Enable Swagger**: Enable API documentation interface
- **CORS Origins**: Allowed origins for cross-origin requests

### Audio Tab
Configure audio recording settings:

- **Enable Recording**: Turn audio recording on/off
- **Storage Path**: Directory for recorded audio files
  - Click folder icon to browse for directory
  - Ensure the directory has sufficient space
- **Audio Format**: WAV or MP3
  - WAV: Uncompressed, highest quality, larger files
  - MP3: Compressed, smaller files, good quality
- **Bitrate**: Audio quality in kbps (default: 128)
  - Higher bitrate = better quality = larger files

### FTP Tab
Configure FTP upload for recordings (optional):

- **Enable FTP Upload**: Turn FTP upload on/off
- **FTP Host**: FTP server address
- **Port**: FTP port (default: 21)
- **Username**: FTP login username
- **Password**: FTP login password (masked)
- **Remote Path**: Destination directory on FTP server
- **Auto-upload**: Automatically upload after recording

**Note**: FTP settings are only active when FTP is enabled

### Saving Settings
- Click **SAVE** button to apply all changes
- Click **CANCEL** to revert to last saved settings
- Status message appears after save (success or error)
- Some settings require application restart to take effect

## Tools

The Tools view provides diagnostic utilities for troubleshooting.

### Ping Test
Test network connectivity to a radio:

1. Enter the target IP address
2. Click **PING** button
3. Results show:
   - Round-trip time (ms)
   - Packet loss
   - Success/failure for each ping

**Use Case**: Verify basic network connectivity before attempting radio connection

### TCP Connection Test
Test TCP port connectivity:

1. Enter target IP address
2. Enter port number (e.g., 50000 for control port)
3. Click **TEST** button
4. Results show:
   - Connection success or failure
   - Error details if connection failed

**Use Case**: Verify the radio's control or audio port is accessible

### Packet Inspector
Analyze raw protocol packets:

1. Paste hex packet data (e.g., "48 65 6C 6C 6F")
2. Click **ANALYZE** button
3. Results show:
   - Packet length
   - Raw bytes in hex format
   - ASCII interpretation
   - Integer interpretations (big/little endian)

**Use Case**: Debug protocol issues or analyze captured packets

### USB Network Scanner
Discover USB network interfaces:

1. Click **Refresh** icon to scan
2. Detected interfaces appear in the list
3. Each interface shows:
   - Interface name
   - Description
   - IP address and gateway
   - Interface type (NCM, RNDIS, etc.)

**Tip**: The gateway address is typically the radio's IP address

### Network Radio Scanner
Discover radios on your Ethernet network:

1. Enter subnet to scan (e.g., "192.168.1")
2. Click **SCAN** button
3. Progress bar shows scan progress (checks 254 addresses)
4. Discovered radios appear in the table showing:
   - IP address
   - Port
   - Model (if detected)
   - Online status
   - Discovery timestamp

**Use Case**: Find Ethernet-connected radios without knowing their IP addresses

### Tips
- Ping test is the first step in troubleshooting connectivity
- If ping succeeds but TCP test fails, check firewall settings
- Packet inspector is for advanced users familiar with the protocol
- Network scanner may take 2-3 minutes for a full subnet scan
- Some routers may block ICMP, causing ping to fail even when radio is reachable

## Troubleshooting

### Common Issues

#### Cannot Connect to Radio

**Problem**: "Connection failed" or "Connection timeout" error

**Solutions**:
1. Verify physical connection:
   - USB: Check cable is properly connected
   - Ethernet: Check cable and link lights on switch/router
2. Check IP address:
   - USB: Should be gateway address of USB interface (192.168.1.1 or 192.168.42.1)
   - Ethernet: Use network scanner to find correct IP
3. Verify ports:
   - Control port: Default 50000
   - Audio port: Default 50001
   - Some models use different ports
4. Check firewall:
   - Windows Firewall may block connections
   - Add HyteraGateway to firewall exceptions
5. Test with diagnostic tools:
   - Use Ping Test in Tools view
   - Use TCP Connection Test to check ports

#### No Logs Appearing

**Problem**: Log view is empty or not updating

**Solutions**:
1. Check log level filter: Set to "All" to see all logs
2. Clear search filter: Empty the search box
3. Check API connection: Logs are received via SignalR from API
4. Restart application: Close and reopen HyteraGateway

#### Statistics Not Loading

**Problem**: Statistics view shows no data or "Loading..."

**Solutions**:
1. Check date range: Ensure there are calls in the selected period
2. Check database connection: Go to Settings → Database → Test Connection
3. Verify API is running: Check bottom status bar
4. Wait for initial load: First load may take a few seconds

#### Audio Recording Not Working

**Problem**: Calls not being recorded

**Solutions**:
1. Check recording enabled: Settings → Audio → Enable Recording
2. Verify storage path: Ensure path exists and has write permissions
3. Check disk space: Ensure sufficient space for recordings
4. Test audio port: Use Tools → TCP Test to verify port 50001
5. Check logs: Look for audio-related errors in Logs view

#### Network Scanner Finds No Radios

**Problem**: Network Radio Scanner completes but shows no results

**Solutions**:
1. Verify subnet: Ensure correct subnet (e.g., "192.168.1" not "192.168.1.0")
2. Check network connectivity: Radios must be on same network
3. Verify radio is powered on and has IP address
4. Check DHCP/static IP: Ensure radio has valid network configuration
5. Try different subnet: Radio may be on different network segment
6. Check firewall: May be blocking scan traffic

### Performance Issues

#### Application Runs Slowly

**Solutions**:
1. Clear old logs: Use Clear Logs button in Logs view
2. Reduce log level: Set to "Info" or "Warning" instead of "Debug"
3. Close unused views: Some views update in real-time and consume resources
4. Check system resources: Task Manager → Performance tab
5. Update drivers: Ensure network drivers are up to date

#### High Memory Usage

**Solutions**:
1. Clear logs periodically
2. Limit recording: Disable recording when not needed
3. Restart application: Restart once per day for 24/7 operation

### Error Messages

#### "Database connection failed"
- Check MySQL is running
- Verify connection settings in Settings → Database
- Test with: `mysql -h localhost -u root -p`

#### "API not available"
- Check if HyteraGateway.Api service is running
- Verify API URL in Settings → API
- Check firewall isn't blocking port 5000

#### "USB interface not found"
- Install USB drivers for your radio
- Try different USB port
- Check Device Manager for USB network adapters

#### "Permission denied"
- Run application as Administrator
- Check folder permissions for recording path
- Verify firewall rules

### Getting Help

If you continue to experience issues:

1. **Check Logs**: Export logs and review error messages
2. **Documentation**: Read ARCHITECTURE.md for technical details
3. **GitHub Issues**: Report bugs at https://github.com/cris-deitos/HyteraGateway/issues
4. **Include Details**: Provide:
   - Radio model
   - Connection type (USB/Ethernet)
   - Error messages from logs
   - Steps to reproduce the issue
   - Screenshot if applicable

### Best Practices

1. **Regular Backups**: Export recordings and logs regularly
2. **Keep Updated**: Install updates when available
3. **Monitor Disk Space**: Recordings can consume significant space
4. **Test After Changes**: Use diagnostic tools after changing settings
5. **Document Configuration**: Keep notes on your radio IPs and ports
6. **Network Security**: Use HTTPS in production environments
7. **Secure Passwords**: Use strong passwords for database and FTP

## Keyboard Shortcuts

- **Ctrl+S**: Save settings (when in Settings view)
- **Ctrl+E**: Export (logs or statistics, depending on active view)
- **Ctrl+F**: Focus search box (in Logs view)
- **F5**: Refresh current view
- **Ctrl+1** to **Ctrl+5**: Switch between views (Dashboard, Statistics, Logs, Settings, Tools)

## About

**Version**: 1.0.0  
**Developer**: HyteraGateway Project  
**License**: See LICENSE file  
**Website**: https://github.com/cris-deitos/HyteraGateway

This application is not affiliated with or endorsed by Hytera Communications Corporation Limited.
