namespace HyteraGateway.Core.Configuration;

/// <summary>
/// Configuration for database connection
/// </summary>
public class DatabaseConfig
{
    /// <summary>
    /// MySQL server hostname or IP
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// MySQL server port
    /// </summary>
    public int Port { get; set; } = 3306;

    /// <summary>
    /// Database name
    /// </summary>
    public string Database { get; set; } = "easyvol";

    /// <summary>
    /// Database username
    /// </summary>
    public string Username { get; set; } = "root";

    /// <summary>
    /// Database password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable connection pooling
    /// </summary>
    public bool UsePooling { get; set; } = true;

    /// <summary>
    /// Minimum pool size
    /// </summary>
    public int MinPoolSize { get; set; } = 5;

    /// <summary>
    /// Maximum pool size
    /// </summary>
    public int MaxPoolSize { get; set; } = 50;
}
