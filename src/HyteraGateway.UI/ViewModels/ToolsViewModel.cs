using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HyteraGateway.UI.Models;
using HyteraGateway.UI.Services;

namespace HyteraGateway.UI.ViewModels;

/// <summary>
/// ViewModel for the Tools view, providing diagnostic utilities
/// </summary>
public partial class ToolsViewModel : ObservableObject
{
    private readonly NetworkDiscoveryService _networkDiscovery;
    
    /// <summary>
    /// Target IP address for ping test
    /// </summary>
    [ObservableProperty]
    private string _pingTarget = "192.168.1.1";
    
    [ObservableProperty]
    private string _pingResult = "";
    
    [ObservableProperty]
    private bool _isPinging;
    
    [ObservableProperty]
    private string _testConnectionTarget = "192.168.1.1";
    
    [ObservableProperty]
    private int _testConnectionPort = 50000;
    
    [ObservableProperty]
    private string _testConnectionResult = "";
    
    [ObservableProperty]
    private bool _isTesting;
    
    [ObservableProperty]
    private string _hexPacketInput = "";
    
    [ObservableProperty]
    private string _packetAnalysisResult = "";
    
    [ObservableProperty]
    private ObservableCollection<NetworkInterfaceInfo> _detectedInterfaces = new();
    
    [ObservableProperty]
    private bool _isScanning;
    
    // Network radio scanner
    [ObservableProperty]
    private string _scanSubnet = "192.168.1";
    
    [ObservableProperty]
    private ObservableCollection<DiscoveredRadio> _discoveredRadios = new();
    
    [ObservableProperty]
    private int _scanProgress;
    
    [ObservableProperty]
    private string _serviceStatus = "Unknown";
    
    [ObservableProperty]
    private string _serviceStatusColor = "#9E9E9E";

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolsViewModel"/> class
    /// </summary>
    /// <param name="networkDiscovery">The network discovery service</param>
    public ToolsViewModel(NetworkDiscoveryService networkDiscovery)
    {
        _networkDiscovery = networkDiscovery;
        RefreshServiceStatus();
    }

    /// <summary>
    /// Pings the specified target IP address
    /// </summary>
    [RelayCommand]
    private async Task PingRadio()
    {
        if (string.IsNullOrWhiteSpace(PingTarget))
        {
            PingResult = "Please enter a target IP address";
            return;
        }

        IsPinging = true;
        PingResult = $"Pinging {PingTarget}...\n";

        try
        {
            for (int i = 0; i < 4; i++)
            {
                var result = await _networkDiscovery.PingAsync(PingTarget);
                PingResult += result.Message + "\n";
                
                if (i < 3)
                {
                    await Task.Delay(1000);
                }
            }
        }
        catch (Exception ex)
        {
            PingResult += $"\nError: {ex.Message}";
        }
        finally
        {
            IsPinging = false;
        }
    }

    /// <summary>
    /// Tests TCP connection to the specified target and port
    /// </summary>
    [RelayCommand]
    private async Task TestConnection()
    {
        if (string.IsNullOrWhiteSpace(TestConnectionTarget))
        {
            TestConnectionResult = "Please enter a target IP address";
            return;
        }

        IsTesting = true;
        TestConnectionResult = $"Testing TCP connection to {TestConnectionTarget}:{TestConnectionPort}...\n";

        try
        {
            var connected = await _networkDiscovery.TestRadioConnectionAsync(
                TestConnectionTarget, 
                TestConnectionPort);

            if (connected)
            {
                TestConnectionResult += "✓ Connection successful!\n";
                TestConnectionResult += "The radio is reachable and accepting connections.";
            }
            else
            {
                TestConnectionResult += "✗ Connection failed!\n";
                TestConnectionResult += "The radio is not responding or the port is not open.";
            }
        }
        catch (Exception ex)
        {
            TestConnectionResult += $"\nError: {ex.Message}";
        }
        finally
        {
            IsTesting = false;
        }
    }

    /// <summary>
    /// Analyzes a hex packet and displays decoded information
    /// </summary>
    [RelayCommand]
    private void AnalyzePacket()
    {
        if (string.IsNullOrWhiteSpace(HexPacketInput))
        {
            PacketAnalysisResult = "Please enter hex packet data";
            return;
        }

        try
        {
            // Remove spaces and validate hex
            var hexClean = HexPacketInput.Replace(" ", "").Replace("\n", "").Replace("\r", "");
            
            if (hexClean.Length % 2 != 0)
            {
                PacketAnalysisResult = "Invalid hex string: length must be even";
                return;
            }

            // Convert to bytes
            var bytes = new byte[hexClean.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexClean.Substring(i * 2, 2), 16);
            }

            // Analyze packet
            var sb = new StringBuilder();
            sb.AppendLine($"Packet Analysis ({bytes.Length} bytes):");
            sb.AppendLine();
            sb.AppendLine("Raw Bytes:");
            sb.AppendLine(BitConverter.ToString(bytes).Replace("-", " "));
            sb.AppendLine();
            sb.AppendLine("ASCII Interpretation:");
            sb.AppendLine(Encoding.ASCII.GetString(bytes.Select(b => b >= 32 && b <= 126 ? b : (byte)'.').ToArray()));
            sb.AppendLine();
            
            if (bytes.Length >= 4)
            {
                sb.AppendLine("First 4 bytes as uint32 (little-endian): " + BitConverter.ToUInt32(bytes, 0));
                
                // Create a copy of just the first 4 bytes and reverse them for big-endian
                var first4Bytes = new byte[4];
                Array.Copy(bytes, 0, first4Bytes, 0, 4);
                Array.Reverse(first4Bytes);
                sb.AppendLine("First 4 bytes as uint32 (big-endian): " + BitConverter.ToUInt32(first4Bytes, 0));
            }

            PacketAnalysisResult = sb.ToString();
        }
        catch (Exception ex)
        {
            PacketAnalysisResult = $"Error analyzing packet: {ex.Message}";
        }
    }

    /// <summary>
    /// Scans for USB network interfaces
    /// </summary>
    [RelayCommand]
    private async Task ScanNetworkInterfaces()
    {
        IsScanning = true;
        DetectedInterfaces.Clear();

        try
        {
            await Task.Run(() =>
            {
                var interfaces = _networkDiscovery.GetUsbNetworkInterfaces();
                
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var iface in interfaces)
                    {
                        DetectedInterfaces.Add(iface);
                    }
                });
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error scanning interfaces: {ex.Message}");
        }
        finally
        {
            IsScanning = false;
        }
    }

    /// <summary>
    /// Scans the specified subnet for radios
    /// </summary>
    [RelayCommand]
    private async Task ScanNetwork()
    {
        IsScanning = true;
        ScanProgress = 0;
        DiscoveredRadios.Clear();

        try
        {
            var progress = new Progress<int>(p => ScanProgress = p);
            var radios = await _networkDiscovery.ScanSubnetForRadiosAsync(
                ScanSubnet, 1, 254, 500, progress);
            
            foreach (var radio in radios)
            {
                DiscoveredRadios.Add(radio);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error scanning network: {ex.Message}");
        }
        finally
        {
            IsScanning = false;
        }
    }

    /// <summary>
    /// Refreshes the service status display
    /// </summary>
    [RelayCommand]
    private void RefreshServiceStatus()
    {
        try
        {
            // Try to find HyteraGateway service
            var services = ServiceController.GetServices();
            var service = services.FirstOrDefault(s => 
                s.ServiceName.Contains("HyteraGateway", StringComparison.OrdinalIgnoreCase));

            if (service != null)
            {
                ServiceStatus = service.Status.ToString();
                ServiceStatusColor = service.Status switch
                {
                    ServiceControllerStatus.Running => "#4CAF50",
                    ServiceControllerStatus.Stopped => "#F44336",
                    ServiceControllerStatus.Paused => "#FF9800",
                    _ => "#9E9E9E"
                };
            }
            else
            {
                ServiceStatus = "Not Installed";
                ServiceStatusColor = "#9E9E9E";
            }
        }
        catch (Exception ex)
        {
            ServiceStatus = $"Error: {ex.Message}";
            ServiceStatusColor = "#F44336";
        }
    }

    /// <summary>
    /// Starts the HyteraGateway Windows service
    /// </summary>
    [RelayCommand]
    private void StartService()
    {
        try
        {
            var services = ServiceController.GetServices();
            var service = services.FirstOrDefault(s => 
                s.ServiceName.Contains("HyteraGateway", StringComparison.OrdinalIgnoreCase));

            if (service != null && service.Status != ServiceControllerStatus.Running)
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                RefreshServiceStatus();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error starting service: {ex.Message}");
            ServiceStatus = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Stops the HyteraGateway Windows service
    /// </summary>
    [RelayCommand]
    private void StopService()
    {
        try
        {
            var services = ServiceController.GetServices();
            var service = services.FirstOrDefault(s => 
                s.ServiceName.Contains("HyteraGateway", StringComparison.OrdinalIgnoreCase));

            if (service != null && service.Status == ServiceControllerStatus.Running)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                RefreshServiceStatus();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error stopping service: {ex.Message}");
            ServiceStatus = $"Error: {ex.Message}";
        }
    }
}
