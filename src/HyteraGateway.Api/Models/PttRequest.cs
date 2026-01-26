namespace HyteraGateway.Api.Models;

/// <summary>
/// Request model for PTT command
/// </summary>
public class PttRequest
{
    /// <summary>
    /// True to press PTT, false to release
    /// </summary>
    public bool Press { get; set; }
}
