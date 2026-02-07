using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
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
}
