using HyteraGateway.Radio.Protocol.DMR;
using HyteraGateway.Radio.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HyteraGateway.Radio.Tests.Services;

public class DualSlotManagerTests
{
    private readonly Mock<ILogger<DualSlotManager>> _mockLogger;
    private readonly DualSlotManager _manager;
    
    public DualSlotManagerTests()
    {
        _mockLogger = new Mock<ILogger<DualSlotManager>>();
        _manager = new DualSlotManager(_mockLogger.Object);
    }
    
    [Fact]
    public void HandleCallStart_Slot1_SetsSlotActive()
    {
        // Arrange
        byte slot = 0;
        uint sourceId = 1234567;
        uint destinationId = 9;
        var callType = CallType.Group;
        
        // Act
        _manager.HandleCallStart(slot, sourceId, destinationId, callType);
        
        // Assert
        var state = _manager.GetSlotState(slot);
        Assert.True(state.IsActive);
        Assert.Equal(sourceId, state.SourceId);
        Assert.Equal(destinationId, state.DestinationId);
        Assert.Equal(callType, state.CallType);
    }
    
    [Fact]
    public void HandleCallStart_Slot2_SetsSlotActive()
    {
        // Arrange
        byte slot = 1;
        uint sourceId = 7654321;
        uint destinationId = 10;
        var callType = CallType.Private;
        
        // Act
        _manager.HandleCallStart(slot, sourceId, destinationId, callType);
        
        // Assert
        var state = _manager.GetSlotState(slot);
        Assert.True(state.IsActive);
        Assert.Equal(sourceId, state.SourceId);
        Assert.Equal(destinationId, state.DestinationId);
        Assert.Equal(callType, state.CallType);
    }
    
    [Fact]
    public void HandleCallEnd_ActiveSlot_ClearsSlot()
    {
        // Arrange
        byte slot = 0;
        _manager.HandleCallStart(slot, 1234567, 9, CallType.Group);
        
        // Act
        _manager.HandleCallEnd(slot);
        
        // Assert
        var state = _manager.GetSlotState(slot);
        Assert.False(state.IsActive);
        Assert.Equal(0u, state.SourceId);
        Assert.Equal(0u, state.DestinationId);
    }
    
    [Fact]
    public void IsSlotAvailable_InactiveSlot_ReturnsTrue()
    {
        // Act
        var available = _manager.IsSlotAvailable(0);
        
        // Assert
        Assert.True(available);
    }
    
    [Fact]
    public void IsSlotAvailable_ActiveSlot_ReturnsFalse()
    {
        // Arrange
        _manager.HandleCallStart(0, 1234567, 9, CallType.Group);
        
        // Act
        var available = _manager.IsSlotAvailable(0);
        
        // Assert
        Assert.False(available);
    }
    
    [Fact]
    public void GetAvailableSlot_BothSlotsFree_ReturnsSlot1()
    {
        // Act
        var slot = _manager.GetAvailableSlot();
        
        // Assert
        Assert.Equal((byte)0, slot);
    }
    
    [Fact]
    public void GetAvailableSlot_Slot1Busy_ReturnsSlot2()
    {
        // Arrange
        _manager.HandleCallStart(0, 1234567, 9, CallType.Group);
        
        // Act
        var slot = _manager.GetAvailableSlot();
        
        // Assert
        Assert.Equal((byte)1, slot);
    }
    
    [Fact]
    public void GetAvailableSlot_BothSlotsBusy_ReturnsNull()
    {
        // Arrange
        _manager.HandleCallStart(0, 1234567, 9, CallType.Group);
        _manager.HandleCallStart(1, 7654321, 10, CallType.Private);
        
        // Act
        var slot = _manager.GetAvailableSlot();
        
        // Assert
        Assert.Null(slot);
    }
    
    [Fact]
    public void SlotStateChanged_WhenCallStarts_EventRaised()
    {
        // Arrange
        bool eventRaised = false;
        byte? eventSlot = null;
        SlotState? eventState = null;
        
        _manager.SlotStateChanged += (sender, args) =>
        {
            eventRaised = true;
            eventSlot = args.Slot;
            eventState = args.State;
        };
        
        // Act
        _manager.HandleCallStart(0, 1234567, 9, CallType.Group);
        
        // Assert
        Assert.True(eventRaised);
        Assert.Equal((byte)0, eventSlot);
        Assert.NotNull(eventState);
        Assert.True(eventState.IsActive);
    }
    
    [Fact]
    public void SlotStateChanged_WhenCallEnds_EventRaised()
    {
        // Arrange
        _manager.HandleCallStart(0, 1234567, 9, CallType.Group);
        
        bool eventRaised = false;
        byte? eventSlot = null;
        SlotState? eventState = null;
        
        _manager.SlotStateChanged += (sender, args) =>
        {
            eventRaised = true;
            eventSlot = args.Slot;
            eventState = args.State;
        };
        
        // Act
        _manager.HandleCallEnd(0);
        
        // Assert
        Assert.True(eventRaised);
        Assert.Equal((byte)0, eventSlot);
        Assert.NotNull(eventState);
        Assert.False(eventState.IsActive);
    }
}
