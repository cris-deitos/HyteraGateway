using HyteraGateway.Core.Interfaces;

namespace HyteraGateway.Service;

/// <summary>
/// Background service for monitoring and managing radio connections
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IRadioService _radioService;

    /// <summary>
    /// Initializes a new instance of the Worker
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="radioService">Radio service instance</param>
    public Worker(ILogger<Worker> logger, IRadioService radioService)
    {
        _logger = logger;
        _radioService = radioService;
    }

    /// <summary>
    /// Executes the background service
    /// </summary>
    /// <param name="stoppingToken">Cancellation token</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("HyteraGateway Service starting at: {time}", DateTimeOffset.Now);

        try
        {
            // Connect to radio
            _logger.LogInformation("Connecting to Hytera radio...");
            var connected = await _radioService.ConnectAsync(stoppingToken);

            if (!connected)
            {
                _logger.LogError("Failed to connect to Hytera radio");
                return;
            }

            _logger.LogInformation("Successfully connected to Hytera radio");

            // Main service loop
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Service running at: {time}", DateTimeOffset.Now);
                }

                // TODO: Process radio events, monitor connection health, etc.

                await Task.Delay(5000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Service is stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in service");
        }
        finally
        {
            _logger.LogInformation("Disconnecting from radio...");
            await _radioService.DisconnectAsync(stoppingToken);
            _logger.LogInformation("HyteraGateway Service stopped at: {time}", DateTimeOffset.Now);
        }
    }
}
