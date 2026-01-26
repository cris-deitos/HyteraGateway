using HyteraGateway.Radio.Protocol.DMR;
using Microsoft.Extensions.Logging;

namespace HyteraGateway.Radio.Services;

/// <summary>
/// Manages DMR dual timeslot (Slot 1 and Slot 2) for simultaneous calls
/// </summary>
public class DualSlotManager
{
    private readonly ILogger<DualSlotManager> _logger;
    private readonly SlotState _slot1 = new();
    private readonly SlotState _slot2 = new();
    
    public event EventHandler<SlotEventArgs>? SlotStateChanged;
    
    public DualSlotManager(ILogger<DualSlotManager> logger)
    {
        _logger = logger;
    }
    
    public void HandleCallStart(byte slot, uint sourceId, uint destinationId, CallType callType)
    {
        var slotState = GetSlot(slot);
        
        slotState.IsActive = true;
        slotState.SourceId = sourceId;
        slotState.DestinationId = destinationId;
        slotState.CallType = callType;
        slotState.StartTime = DateTime.UtcNow;
        
        _logger.LogInformation("Slot {Slot}: Call started - {SourceId} → {DestId} ({Type})", 
            slot + 1, sourceId, destinationId, callType);
        
        SlotStateChanged?.Invoke(this, new SlotEventArgs(slot, slotState));
    }
    
    public void HandleCallEnd(byte slot)
    {
        var slotState = GetSlot(slot);
        
        if (slotState.IsActive)
        {
            var duration = DateTime.UtcNow - slotState.StartTime;
            _logger.LogInformation("Slot {Slot}: Call ended - Duration: {Duration:mm\\:ss}", 
                slot + 1, duration);
        }
        
        slotState.IsActive = false;
        slotState.SourceId = 0;
        slotState.DestinationId = 0;
        
        SlotStateChanged?.Invoke(this, new SlotEventArgs(slot, slotState));
    }
    
    public SlotState GetSlotState(byte slot) => GetSlot(slot);
    
    public bool IsSlotAvailable(byte slot) => !GetSlot(slot).IsActive;
    
    public byte? GetAvailableSlot()
    {
        if (!_slot1.IsActive) return 0;
        if (!_slot2.IsActive) return 1;
        return null; // Both slots busy
    }
    
    private SlotState GetSlot(byte slot) => slot == 0 ? _slot1 : _slot2;
}

public class SlotState
{
    public bool IsActive { get; set; }
    public uint SourceId { get; set; }
    public uint DestinationId { get; set; }
    public CallType CallType { get; set; }
    public DateTime StartTime { get; set; }
}

public class SlotEventArgs : EventArgs
{
    public byte Slot { get; }
    public SlotState State { get; }
    
    public SlotEventArgs(byte slot, SlotState state)
    {
        Slot = slot;
        State = state;
    }
}
