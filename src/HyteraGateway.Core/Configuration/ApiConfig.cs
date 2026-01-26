namespace HyteraGateway.Core.Configuration;

/// <summary>
/// Configuration for API and WebSocket
/// </summary>
public class ApiConfig
{
    /// <summary>
    /// API listening port
    /// </summary>
    public int Port { get; set; } = 5000;

    /// <summary>
    /// Enable HTTPS
    /// </summary>
    public bool UseHttps { get; set; } = false;

    /// <summary>
    /// Enable Swagger UI
    /// </summary>
    public bool EnableSwagger { get; set; } = true;

    /// <summary>
    /// Enable CORS
    /// </summary>
    public bool EnableCors { get; set; } = true;

    /// <summary>
    /// Allowed CORS origins (empty = all)
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Enable authentication
    /// </summary>
    public bool EnableAuth { get; set; } = false;

    /// <summary>
    /// API key for authentication (if EnableAuth is true)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Enable WebSocket for real-time events
    /// </summary>
    public bool EnableWebSocket { get; set; } = true;

    /// <summary>
    /// WebSocket path
    /// </summary>
    public string WebSocketPath { get; set; } = "/ws";
}
