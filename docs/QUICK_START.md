# HyteraGateway Quick Start Guide

Get up and running with HyteraGateway in 5 minutes!

## Prerequisites

Before you begin, ensure you have:

- ✅ Windows 10 or Windows 11 (64-bit)
- ✅ .NET 8.0 Runtime (included with installer)
- ✅ Hytera DMR radio (MD785i, HM785, or compatible model)
- ✅ USB cable (for MD785i) OR Ethernet cable (for HM785)
- ✅ 4GB RAM minimum
- ✅ 500MB free disk space

**Optional** (for full features):
- MySQL 8.0 or later (for call history and statistics)
- Active network connection (for Ethernet radios)

## Step 1: Install HyteraGateway (1 minute)

1. Download the latest installer: `HyteraGateway-Setup.exe`
2. Run the installer and follow the wizard
3. Accept the license agreement
4. Click "Install" (uses default location: `C:\Program Files\HyteraGateway`)
5. Launch HyteraGateway from the Start Menu or Desktop shortcut

## Step 2: Connect Your Radio (2 minutes)

### Option A: USB Connection (MD785i)

1. **Connect the radio**:
   - Plug the USB cable into the radio and your computer
   - Wait for Windows to detect the USB network interface (about 30 seconds)

2. **Configure in HyteraGateway**:
   - Open **Settings** (gear icon in left sidebar)
   - Click the **Radio** tab
   - Select "USB" as Connection Type
   - Click **"Refresh USB Interfaces"**
   - Select the detected interface from the dropdown
   - The IP address will auto-fill (typically `192.168.1.1` or `192.168.42.1`)

3. **Test the connection**:
   - Click **"Test Connection"** button
   - Wait for green "Connection successful!" message
   - Click **"SAVE"** button at the bottom

✅ **Done!** Your USB radio is connected.

### Option B: Ethernet Connection (HM785)

1. **Connect the radio**:
   - Plug Ethernet cable into the radio and your network switch/router
   - Power on the radio
   - Wait for the radio to get an IP address (DHCP or static)

2. **Find the radio's IP** (choose one method):

   **Method 1: Network Scanner** (Recommended)
   - Open **Settings** → **Radio** tab
   - Select "Ethernet" as Connection Type
   - Enter your subnet in the "Scan Subnet" field (e.g., `192.168.1`)
   - Click **"SCAN"** button
   - Wait for scan to complete (1-3 minutes)
   - Select the discovered radio from the table
   - Click **"Use Selected Radio"**

   **Method 2: Manual Entry** (If you know the IP)
   - Select "Ethernet" as Connection Type
   - Manually enter the radio's IP address in "Radio IP Address" field
   - Leave ports as default (Control: 50000, Audio: 50001)

3. **Test the connection**:
   - Click **"Test Connection"** button
   - Wait for green "Connection successful!" message
   - Click **"SAVE"** button at the bottom

✅ **Done!** Your Ethernet radio is connected.

## Step 3: Verify Connection (1 minute)

1. **Go to Dashboard**:
   - Click the home icon in the left sidebar
   - You should see your radio displayed as a card

2. **Check radio status**:
   - Status should show "Connected" in green
   - Radio model and IP address should be displayed
   - DMR ID and channel information should appear

3. **Test communication** (if radio is active):
   - Press PTT on the radio
   - The Dashboard should show "Transmitting" status in real-time
   - Release PTT and status returns to "Idle"

✅ **Success!** Your radio is communicating with HyteraGateway.

## Step 4: Explore Features (1 minute)

### View Live Logs
- Click **Logs** icon (list icon) in the left sidebar
- See real-time application and radio events
- Use the search box to filter logs
- Try changing log level filter to see different detail levels

### Enable Audio Recording
1. Go to **Settings** → **Audio** tab
2. Check **"Enable Recording"**
3. Click folder icon next to **Storage Path**
4. Choose a folder for recordings (e.g., `C:\HyteraRecordings`)
5. Select audio format (WAV or MP3)
6. Click **"SAVE"**

Now all radio calls will be automatically recorded!

### Use Diagnostic Tools
1. Go to **Tools** (wrench icon)
2. Try the **Ping Test**:
   - Your radio's IP is pre-filled
   - Click **"PING"** button
   - See response times and connectivity

3. Try the **USB Network Scanner**:
   - Click refresh icon
   - See all USB network interfaces detected
   - Useful for troubleshooting USB connections

## Next Steps

Now that you're set up, explore these features:

### 📊 Statistics
- View call analytics and charts
- Track daily call volume
- Analyze call types (Private, Group, Emergency)
- Export reports to Excel

### 🔧 Advanced Settings
- Configure database connection for call history
- Set up FTP upload for automatic recording backups
- Customize API settings for integration
- Adjust audio quality and bitrate

### 📡 Multiple Radios
- Connect additional radios
- All radios appear on the Dashboard
- Each radio is monitored independently
- Switch between radios easily

## Common Quick Start Issues

### ❌ "USB interface not found"
**Solution**: 
- Wait 30-60 seconds after plugging in USB cable
- Try different USB port
- Install USB drivers for your radio model
- Click "Refresh USB Interfaces" again

### ❌ "Connection failed"
**USB Connection**:
- Verify cable is connected properly
- Check Device Manager for "Remote NDIS" or "NCM" device
- Try different USB port
- Restart the application

**Ethernet Connection**:
- Verify cable is connected and link lights are on
- Use network scanner to find the radio
- Check radio has valid IP address
- Try ping test in Tools view
- Check Windows Firewall isn't blocking connection

### ❌ "Database connection failed"
**Solution**: This is **optional** for basic operation
- You can use HyteraGateway without a database
- To enable database features, install MySQL 8.0+
- Configure connection in Settings → Database tab

### ❌ Radio appears but shows "Disconnected"
**Solution**:
- Check firewall settings
- Verify ports 50000 and 50001 are not blocked
- Use Tools → TCP Connection Test to verify ports
- Check radio is powered on and within network range

## Quick Reference

### Default Values
- **Control Port**: 50000
- **Audio Port**: 50001
- **Dispatcher ID**: 1
- **USB IP** (MD785i): 192.168.1.1 or 192.168.42.1
- **API URL**: http://localhost:5000

### UI Navigation
- **Home/Dashboard**: View connected radios
- **Statistics**: Call analytics and charts
- **Logs**: Real-time events and logging
- **Settings**: Configure application
- **Tools**: Diagnostic utilities

### Keyboard Shortcuts
- **Ctrl+1** to **Ctrl+5**: Switch between views
- **F5**: Refresh current view
- **Ctrl+S**: Save settings
- **Ctrl+E**: Export data

## Getting Help

### Resources
- **Full Documentation**: See `USER_GUIDE.md` for detailed information
- **Architecture**: See `ARCHITECTURE.md` for technical details
- **Troubleshooting**: See `TROUBLESHOOTING.md` for common issues

### Support
- **GitHub Issues**: https://github.com/cris-deitos/HyteraGateway/issues
- **Logs**: Export logs from Logs view to help diagnose issues
- **Screenshots**: Take screenshots when reporting problems

## Tips for Success

1. **Start Simple**: Get basic connection working before configuring advanced features
2. **Use Auto-Discovery**: Let the application find USB interfaces and network radios
3. **Test Each Step**: Use "Test Connection" button after configuration
4. **Check the Logs**: The Logs view shows what's happening in real-time
5. **Keep Defaults**: Default port numbers work for most radios
6. **Update Regularly**: Check for updates to get new features and fixes

## What's Next?

After completing this quick start:

1. **Learn More**: Read the full USER_GUIDE.md
2. **Configure Recording**: Set up audio recording for call archival
3. **Set Up Database**: Enable MySQL for long-term call history
4. **Explore Statistics**: Analyze your radio usage patterns
5. **Integrate**: Use the REST API for custom applications

---

**Congratulations!** 🎉 You've successfully set up HyteraGateway!

*For detailed information on all features, see the [User Guide](USER_GUIDE.md).*
