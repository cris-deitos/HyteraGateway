namespace HyteraGateway.Core.Models;

/// <summary>
/// Represents a recorded call/transmission
/// </summary>
public class CallRecord
{
    /// <summary>
    /// Unique identifier for the call record
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// DMR timeslot (1 or 2)
    /// </summary>
    public int Slot { get; set; }

    /// <summary>
    /// DMR ID of the caller
    /// </summary>
    public int CallerDmrId { get; set; }

    /// <summary>
    /// Call sign or alias of caller
    /// </summary>
    public string? CallerAlias { get; set; }

    /// <summary>
    /// Target talkgroup or individual ID
    /// </summary>
    public int TargetId { get; set; }

    /// <summary>
    /// Type of call
    /// </summary>
    public CallType CallType { get; set; }

    /// <summary>
    /// Call start time
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Call end time
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Duration in seconds
    /// </summary>
    public double? Duration { get; set; }

    /// <summary>
    /// Path to recorded audio file
    /// </summary>
    public string? AudioFilePath { get; set; }

    /// <summary>
    /// Audio file size in bytes
    /// </summary>
    public long? AudioFileSize { get; set; }
}
