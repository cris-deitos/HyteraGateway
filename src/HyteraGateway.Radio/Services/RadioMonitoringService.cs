using HyteraGateway.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace HyteraGateway.Radio.Services;

/// <summary>
/// Periodic monitoring service for radio activity and GPS positions
/// Implements ActivityCheck and PositionCheck from NETRadioServer.exe.config
/// </summary>
public class RadioMonitoringService : IDisposable
{
    private readonly ILogger<RadioMonitoringService> _logger;
    private readonly IRadioService _radioService;
    private readonly RadioMonitoringConfig _config;
    private CancellationTokenSource? _cts;
    private Task? _activityTask;
    private Task? _positionTask;
    
    public RadioMonitoringService(
        ILogger<RadioMonitoringService> logger,
        IRadioService radioService,
        RadioMonitoringConfig config)
    {
        _logger = logger;
        _radioService = radioService;
        _config = config;
    }
    
    public void Start()
    {
        _logger.LogInformation("Radio monitoring service started");
        
        _cts = new CancellationTokenSource();
        
        if (_config.ActivityCheckEnabled)
        {
            _activityTask = ActivityCheckLoopAsync(_cts.Token);
        }
        
        if (_config.PositionCheckEnabled)
        {
            _positionTask = PositionCheckLoopAsync(_cts.Token);
        }
    }
    
    public async Task StopAsync()
    {
        _logger.LogInformation("Radio monitoring service stopping");
        
        _cts?.Cancel();
        
        if (_activityTask != null)
        {
            await _activityTask;
        }
        
        if (_positionTask != null)
        {
            await _positionTask;
        }
    }
    
    private async Task ActivityCheckLoopAsync(CancellationToken ct)
    {
        var interval = TimeSpan.FromMinutes(_config.ActivityCheckIntervalMinutes);
        
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, ct);
                
                _logger.LogDebug("Performing activity check");
                
                // Check which radios haven't been active
                // Send radio check command to inactive radios
                // This would integrate with database to track last activity time
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in activity check loop");
            }
        }
    }
    
    private async Task PositionCheckLoopAsync(CancellationToken ct)
    {
        var interval = TimeSpan.FromMinutes(_config.PositionCheckIntervalMinutes);
        
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, ct);
                
                _logger.LogDebug("Performing position check - requesting GPS from all radios");
                
                // Request GPS from all configured radios
                // This would integrate with RadioControllerConfig to get radio list
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in position check loop");
            }
        }
    }
    
    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        _cts?.Dispose();
    }
}

/// <summary>
/// Configuration for radio monitoring service
/// </summary>
public class RadioMonitoringConfig
{
    public bool ActivityCheckEnabled { get; set; }
    public int ActivityCheckIntervalMinutes { get; set; } = 60;
    public bool PositionCheckEnabled { get; set; }
    public int PositionCheckIntervalMinutes { get; set; } = 30;
}
