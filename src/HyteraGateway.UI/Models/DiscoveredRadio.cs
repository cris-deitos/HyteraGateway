using System;
using HyteraGateway.Core.Configuration;

namespace HyteraGateway.UI.Models;

/// <summary>
/// Represents a radio discovered on the network
/// </summary>
public class DiscoveredRadio
{
    /// <summary>
    /// IP address of the discovered radio
    /// </summary>
    public string IpAddress { get; set; } = "";
    
    /// <summary>
    /// Port number (typically 50000)
    /// </summary>
    public int Port { get; set; } = 50000;
    
    /// <summary>
    /// Connection type
    /// </summary>
    public RadioConnectionType ConnectionType { get; set; }
    
    /// <summary>
    /// Radio model (if detected)
    /// </summary>
    public string Model { get; set; } = "";
    
    /// <summary>
    /// Radio name/identifier
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Whether the radio is currently online
    /// </summary>
    public bool IsOnline { get; set; }
    
    /// <summary>
    /// When the radio was discovered
    /// </summary>
    public DateTime DiscoveredAt { get; set; } = DateTime.Now;
}
