using HyteraGateway.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace HyteraGateway.Radio.Services;

/// <summary>
/// Monitors PTT duration and automatically releases after timeout (default 180s)
/// Prevents stuck PTT situations
/// </summary>
public class PttTimeoutService
{
    private readonly ILogger<PttTimeoutService> _logger;
    private readonly IRadioService _radioService;
    private readonly TimeSpan _timeout;
    
    private readonly Dictionary<(uint destinationId, byte slot), PttSession> _activeSessions = new();
    private Timer? _checkTimer;
    
    public PttTimeoutService(ILogger<PttTimeoutService> logger, IRadioService radioService, TimeSpan? timeout = null)
    {
        _logger = logger;
        _radioService = radioService;
        _timeout = timeout ?? TimeSpan.FromSeconds(180);
        
        _checkTimer = new Timer(CheckTimeouts, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }
    
    public void OnPttPressed(uint destinationId, byte slot)
    {
        var key = (destinationId, slot);
        _activeSessions[key] = new PttSession
        {
            StartTime = DateTime.UtcNow,
            DestinationId = destinationId,
            Slot = slot
        };
        
        _logger.LogDebug("PTT session started: TG={TalkGroup}, Slot={Slot}", destinationId, slot);
    }
    
    public void OnPttReleased(uint destinationId, byte slot)
    {
        var key = (destinationId, slot);
        if (_activeSessions.Remove(key, out var session))
        {
            var duration = DateTime.UtcNow - session.StartTime;
            _logger.LogDebug("PTT session ended: TG={TalkGroup}, Slot={Slot}, Duration={Duration:mm\\:ss}", 
                destinationId, slot, duration);
        }
    }
    
    private async void CheckTimeouts(object? state)
    {
        try
        {
            var now = DateTime.UtcNow;
            var timedOut = _activeSessions
                .Where(kvp => now - kvp.Value.StartTime > _timeout)
                .ToList();
            
            foreach (var (key, session) in timedOut)
            {
                _logger.LogWarning("PTT timeout reached: TG={TalkGroup}, Slot={Slot}, Duration={Duration:mm\\:ss}. Auto-releasing PTT.",
                    session.DestinationId, session.Slot, now - session.StartTime);
                
                try
                {
                    await _radioService.SendPttAsync((int)session.DestinationId, false, CancellationToken.None);
                    _activeSessions.Remove(key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to auto-release PTT");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in PTT timeout check");
        }
    }
    
    private class PttSession
    {
        public DateTime StartTime { get; set; }
        public uint DestinationId { get; set; }
        public byte Slot { get; set; }
    }
}
