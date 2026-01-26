namespace HyteraGateway.Core.Models;

/// <summary>
/// Represents a radio event from a Hytera radio
/// </summary>
public class RadioEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Type of radio event
    /// </summary>
    public RadioEventType EventType { get; set; }

    /// <summary>
    /// DMR ID of the radio that triggered the event
    /// </summary>
    public int RadioDmrId { get; set; }

    /// <summary>
    /// Optional additional data as JSON
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Radio call sign or alias
    /// </summary>
    public string? CallSign { get; set; }
}
