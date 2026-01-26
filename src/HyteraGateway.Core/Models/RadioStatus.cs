namespace HyteraGateway.Core.Models;

/// <summary>
/// Represents the current status of a radio
/// </summary>
public class RadioStatus
{
    /// <summary>
    /// DMR ID of the radio
    /// </summary>
    public int RadioDmrId { get; set; }

    /// <summary>
    /// Call sign or alias
    /// </summary>
    public string? CallSign { get; set; }

    /// <summary>
    /// Current state of the radio
    /// </summary>
    public RadioState State { get; set; } = RadioState.Unknown;

    /// <summary>
    /// Last seen timestamp
    /// </summary>
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Current talkgroup
    /// </summary>
    public int? CurrentTalkgroup { get; set; }

    /// <summary>
    /// Current channel
    /// </summary>
    public int? CurrentChannel { get; set; }

    /// <summary>
    /// Battery level percentage (0-100)
    /// </summary>
    public int? BatteryLevel { get; set; }

    /// <summary>
    /// Signal strength in dBm
    /// </summary>
    public int? SignalStrength { get; set; }

    /// <summary>
    /// Last known GPS position
    /// </summary>
    public GpsPosition? LastPosition { get; set; }
}
