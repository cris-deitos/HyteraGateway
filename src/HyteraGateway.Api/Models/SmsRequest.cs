namespace HyteraGateway.Api.Models;

/// <summary>
/// Request model for sending text message
/// </summary>
public class SmsRequest
{
    /// <summary>
    /// Message text to send
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
