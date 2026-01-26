using HyteraGateway.Core.Models;

namespace HyteraGateway.Core.Interfaces;

/// <summary>
/// Service for interacting with Hytera radios
/// </summary>
public interface IRadioService
{
    /// <summary>
    /// Connects to the Hytera radio via USB NCM
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection successful</returns>
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from the radio
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends PTT (Push-To-Talk) command to a radio
    /// </summary>
    /// <param name="dmrId">DMR ID of the target radio</param>
    /// <param name="press">True to press PTT, false to release</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if command was sent successfully</returns>
    Task<bool> SendPttAsync(int dmrId, bool press, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests GPS position from a radio
    /// </summary>
    /// <param name="dmrId">DMR ID of the target radio</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>GPS position if available</returns>
    Task<GpsPosition?> RequestGpsAsync(int dmrId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a text message to a radio
    /// </summary>
    /// <param name="dmrId">DMR ID of the target radio</param>
    /// <param name="message">Message text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if message was sent successfully</returns>
    Task<bool> SendTextMessageAsync(int dmrId, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of a radio
    /// </summary>
    /// <param name="dmrId">DMR ID of the radio</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Radio status</returns>
    Task<RadioStatus?> GetStatusAsync(int dmrId, CancellationToken cancellationToken = default);
}
