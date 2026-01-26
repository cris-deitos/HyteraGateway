using HyteraGateway.Core.Models;

namespace HyteraGateway.Core.Interfaces;

/// <summary>
/// Service for database operations
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// Saves a call record to the database
    /// </summary>
    /// <param name="callRecord">Call record to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of rows affected</returns>
    Task<int> SaveCallRecordAsync(CallRecord callRecord, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a GPS position to the database
    /// </summary>
    /// <param name="position">GPS position to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of rows affected</returns>
    Task<int> SaveGpsPositionAsync(GpsPosition position, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a radio event to the database
    /// </summary>
    /// <param name="radioEvent">Radio event to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of rows affected</returns>
    Task<int> SaveRadioEventAsync(RadioEvent radioEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves an emergency alert to the database
    /// </summary>
    /// <param name="alert">Emergency alert to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of rows affected</returns>
    Task<int> SaveEmergencyAlertAsync(EmergencyAlert alert, CancellationToken cancellationToken = default);
}
