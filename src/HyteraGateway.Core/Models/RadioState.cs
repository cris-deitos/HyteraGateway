namespace HyteraGateway.Core.Models;

/// <summary>
/// Represents the current state of a radio
/// </summary>
public enum RadioState
{
    /// <summary>
    /// Radio is online and idle
    /// </summary>
    Online,

    /// <summary>
    /// Radio is offline or disconnected
    /// </summary>
    Offline,

    /// <summary>
    /// Radio is transmitting
    /// </summary>
    Transmitting,

    /// <summary>
    /// Radio is receiving
    /// </summary>
    Receiving,

    /// <summary>
    /// Radio is in emergency mode
    /// </summary>
    Emergency,

    /// <summary>
    /// Radio status unknown
    /// </summary>
    Unknown
}
