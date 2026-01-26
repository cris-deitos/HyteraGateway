namespace HyteraGateway.Core.Models;

/// <summary>
/// Represents the type of radio event
/// </summary>
public enum RadioEventType
{
    /// <summary>
    /// PTT button pressed
    /// </summary>
    PttPressed,

    /// <summary>
    /// PTT button released
    /// </summary>
    PttReleased,

    /// <summary>
    /// Call started
    /// </summary>
    CallStart,

    /// <summary>
    /// Call ended
    /// </summary>
    CallEnd,

    /// <summary>
    /// GPS position update
    /// </summary>
    GpsPosition,

    /// <summary>
    /// Emergency alert triggered
    /// </summary>
    Emergency,

    /// <summary>
    /// Text message received
    /// </summary>
    TextMessage,

    /// <summary>
    /// Radio status changed
    /// </summary>
    StatusChange,

    /// <summary>
    /// Radio connected
    /// </summary>
    Connected,

    /// <summary>
    /// Radio disconnected
    /// </summary>
    Disconnected
}
