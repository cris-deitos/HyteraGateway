using System;

namespace HyteraGateway.UI.Models;

/// <summary>
/// Represents a log entry with timestamp, level, source, and message
/// </summary>
public class LogEntry
{
    /// <summary>
    /// Gets or sets the timestamp of the log entry
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the log level (Debug, Info, Warning, Error)
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source of the log entry
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the log message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
