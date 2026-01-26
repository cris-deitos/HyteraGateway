namespace HyteraGateway.Core.Models;

/// <summary>
/// Represents an emergency alert from a radio
/// </summary>
public class EmergencyAlert
{
    /// <summary>
    /// Unique identifier for the alert
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// DMR ID of the radio in emergency
    /// </summary>
    public int RadioDmrId { get; set; }

    /// <summary>
    /// Call sign or alias
    /// </summary>
    public string? CallSign { get; set; }

    /// <summary>
    /// Timestamp when emergency was triggered
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the emergency has been acknowledged
    /// </summary>
    public bool Acknowledged { get; set; }

    /// <summary>
    /// Timestamp of acknowledgement
    /// </summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// User who acknowledged the emergency
    /// </summary>
    public string? AcknowledgedBy { get; set; }

    /// <summary>
    /// GPS position at time of emergency (if available)
    /// </summary>
    public GpsPosition? Position { get; set; }

    /// <summary>
    /// Additional notes about the emergency
    /// </summary>
    public string? Notes { get; set; }
}
