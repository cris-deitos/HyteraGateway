using HyteraGateway.Core.Configuration;

namespace HyteraGateway.UI.Models;

/// <summary>
/// Represents a saved radio configuration for quick connect
/// </summary>
public class SavedRadio
{
    /// <summary>
    /// User-defined name for the radio
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// IP address of the radio
    /// </summary>
    public string IpAddress { get; set; } = "";
    
    /// <summary>
    /// Connection type
    /// </summary>
    public RadioConnectionType ConnectionType { get; set; }
    
    /// <summary>
    /// Radio model (optional)
    /// </summary>
    public string Model { get; set; } = "";
}
