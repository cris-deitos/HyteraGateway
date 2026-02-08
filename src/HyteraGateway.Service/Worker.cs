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

            // Main service loop with connection health monitoring
            int reconnectAttempts = 0;
            const int MAX_RECONNECT_ATTEMPTS = 10;
            const int HEALTH_CHECK_INTERVAL_MS = 5000;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Service running at: {time}", DateTimeOffset.Now);
                }

                // Monitor connection health
                if (!_radioService.IsConnected)
                {
                    _logger.LogWarning("Connection to Hytera radio lost. Attempting to reconnect...");
                    reconnectAttempts++;

                    if (reconnectAttempts > MAX_RECONNECT_ATTEMPTS)
                    {
                        _logger.LogError("Failed to reconnect after {attempts} attempts. Service will stop.", MAX_RECONNECT_ATTEMPTS);
                        break;
                    }

                    try
                    {
                        _logger.LogInformation("Reconnect attempt {attempt}/{maxAttempts}", reconnectAttempts, MAX_RECONNECT_ATTEMPTS);
                        var reconnected = await _radioService.ConnectAsync(stoppingToken);

                        if (reconnected)
                        {
                            _logger.LogInformation("Successfully reconnected to Hytera radio");
                            reconnectAttempts = 0; // Reset counter on successful reconnect
                        }
                        else
                        {
                            _logger.LogWarning("Reconnect attempt {attempt} failed", reconnectAttempts);
                            // Wait before next reconnect attempt
                            await Task.Delay(3000, stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during reconnect attempt {attempt}", reconnectAttempts);
                        await Task.Delay(3000, stoppingToken);
                    }
                }
                else
                {
                    // Connection is healthy, reset reconnect attempts counter
                    if (reconnectAttempts > 0)
                    {
                        reconnectAttempts = 0;
                    }
                }

                await Task.Delay(HEALTH_CHECK_INTERVAL_MS, stoppingToken);
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
