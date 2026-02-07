using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HyteraGateway.Core.Configuration;
using HyteraGateway.UI.Models;

namespace HyteraGateway.UI.Services;

/// <summary>
/// Service for discovering network interfaces and testing radio connections
/// </summary>
public class NetworkDiscoveryService
{
    /// <summary>
    /// Get all network interfaces that might be RNDIS or NCM (USB network adapters)
    /// </summary>
    public List<NetworkInterfaceInfo> GetUsbNetworkInterfaces()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
            .Where(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet 
                      || ni.Description.Contains("RNDIS", StringComparison.OrdinalIgnoreCase)
                      || ni.Description.Contains("NCM", StringComparison.OrdinalIgnoreCase)
                      || ni.Description.Contains("USB", StringComparison.OrdinalIgnoreCase)
                      || ni.Description.Contains("CDC", StringComparison.OrdinalIgnoreCase))
            .Select(ni => new NetworkInterfaceInfo
            {
                Name = ni.Name,
                Description = ni.Description,
                IpAddress = GetIpAddress(ni),
                GatewayAddress = GetGateway(ni),
                Type = DetectType(ni)
            })
            .Where(ni => !string.IsNullOrEmpty(ni.IpAddress))
            .ToList();
    }

    /// <summary>
    /// Get the IP address of a network interface
    /// </summary>
    private string GetIpAddress(NetworkInterface networkInterface)
    {
        try
        {
            var ipProperties = networkInterface.GetIPProperties();
            var ipv4Address = ipProperties.UnicastAddresses
                .FirstOrDefault(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork);
            
            return ipv4Address?.Address.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Get the gateway address of a network interface
    /// </summary>
    private string GetGateway(NetworkInterface networkInterface)
    {
        try
        {
            var ipProperties = networkInterface.GetIPProperties();
            var gateway = ipProperties.GatewayAddresses
                .FirstOrDefault(ga => ga.Address.AddressFamily == AddressFamily.InterNetwork);
            
            return gateway?.Address.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Detect the type of network interface
    /// </summary>
    private string DetectType(NetworkInterface networkInterface)
    {
        var description = networkInterface.Description.ToLowerInvariant();
        
        if (description.Contains("rndis"))
            return "RNDIS";
        
        if (description.Contains("ncm") || description.Contains("cdc"))
            return "NCM";
        
        if (description.Contains("usb"))
            return "USB";
        
        return "Ethernet";
    }

    /// <summary>
    /// Test radio connection by attempting to connect to the specified IP and port
    /// </summary>
    public async Task<bool> TestRadioConnectionAsync(string ipAddress, int port = 50000, int timeoutMs = 3000)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(ipAddress, port);
            var timeoutTask = Task.Delay(timeoutMs);
            
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            
            if (completedTask == connectTask && client.Connected)
            {
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ping an IP address to test basic connectivity
    /// </summary>
    public async Task<(bool Success, long RoundtripTime, string Message)> PingAsync(string ipAddress, int timeoutMs = 3000)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return (false, 0, "IP address cannot be empty");
        }

        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress, timeoutMs);
            
            if (reply.Status == IPStatus.Success)
            {
                return (true, reply.RoundtripTime, $"Reply from {ipAddress}: time={reply.RoundtripTime}ms");
            }
            
            return (false, 0, $"Ping failed: {reply.Status}");
        }
        catch (Exception ex)
        {
            return (false, 0, $"Ping error: {ex.Message}");
        }
    }

    /// <summary>
    /// Scan a subnet for Hytera radios by testing port 50000
    /// Used for Ethernet-connected radios like HM785
    /// </summary>
    public async Task<List<DiscoveredRadio>> ScanSubnetForRadiosAsync(
        string subnet,           // e.g., "10.0.0"
        int startIp = 1,
        int endIp = 254,
        int timeoutMs = 500,
        IProgress<int>? progress = null,
        CancellationToken ct = default)
    {
        var radios = new List<DiscoveredRadio>();
        var tasks = new List<Task>();
        var lockObj = new object();
        
        for (int i = startIp; i <= endIp; i++)
        {
            if (ct.IsCancellationRequested)
                break;
                
            string ip = $"{subnet}.{i}";
            int index = i;
            
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    if (await TestRadioConnectionAsync(ip, 50000, timeoutMs))
                    {
                        var radio = await IdentifyRadioAsync(ip);
                        lock (lockObj)
                        {
                            radios.Add(radio);
                        }
                    }
                }
                catch
                {
                    // Ignore individual scan errors
                }
                finally
                {
                    progress?.Report(index - startIp + 1);
                }
            }, ct));
        }
        
        await Task.WhenAll(tasks);
        return radios;
    }
    
    /// <summary>
    /// Try to identify radio model by connecting and reading response
    /// </summary>
    public Task<DiscoveredRadio> IdentifyRadioAsync(string ipAddress)
    {
        // TODO: Future enhancement - query IPSC protocol to get actual model info
        // For now, we just return basic info
        var radio = new DiscoveredRadio
        {
            IpAddress = ipAddress,
            Port = 50000,
            ConnectionType = RadioConnectionType.Ethernet,
            Model = "Unknown (Hytera IPSC)",
            IsOnline = true,
            DiscoveredAt = DateTime.Now
        };
        
        return Task.FromResult(radio);
    }
}
