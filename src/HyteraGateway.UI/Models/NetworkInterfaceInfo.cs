namespace HyteraGateway.UI.Models;

/// <summary>
/// Represents information about a network interface
/// </summary>
public class NetworkInterfaceInfo
{
    /// <summary>
    /// Gets or sets the name of the network interface
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the network interface
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IP address of the interface
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gateway address (likely the radio IP)
    /// </summary>
    public string GatewayAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of interface (RNDIS, NCM, Unknown)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets a display string for the interface
    /// </summary>
    public string DisplayName => $"{Name} - {Description} ({IpAddress})";
}
