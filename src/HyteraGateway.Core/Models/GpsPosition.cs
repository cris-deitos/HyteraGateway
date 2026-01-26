namespace HyteraGateway.Core.Models;

/// <summary>
/// Represents a GPS position report from a radio
/// </summary>
public class GpsPosition
{
    /// <summary>
    /// Unique identifier for the position record
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// DMR ID of the radio
    /// </summary>
    public int RadioDmrId { get; set; }

    /// <summary>
    /// Timestamp of position report
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Latitude in decimal degrees
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude in decimal degrees
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Altitude in meters
    /// </summary>
    public double? Altitude { get; set; }

    /// <summary>
    /// Speed in km/h
    /// </summary>
    public double? Speed { get; set; }

    /// <summary>
    /// Heading/bearing in degrees (0-359)
    /// </summary>
    public int? Heading { get; set; }

    /// <summary>
    /// GPS fix accuracy in meters
    /// </summary>
    public double? Accuracy { get; set; }
}
