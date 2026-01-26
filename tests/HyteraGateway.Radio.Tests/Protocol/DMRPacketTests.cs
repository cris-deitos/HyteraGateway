using HyteraGateway.Radio.Protocol.DMR;
using Xunit;

namespace HyteraGateway.Radio.Tests.Protocol;

public class DMRPacketTests
{
    [Fact]
    public void ToBytes_ShouldSerializePacketCorrectly()
    {
        // Arrange
        var packet = new DMRPacket
        {
            SyncPattern = new byte[] { 0x77, 0x55, 0xFD, 0x7D, 0xF7, 0x5F },
            Slot = 0,
            ColorCode = 1,
            Type = PacketType.Voice,
            CallType = CallType.Group,
            SourceId = 12345,
            DestinationId = 100,
            SequenceNumber = 42,
            Payload = new byte[] { 0x01, 0x02, 0x03, 0x04 }
        };

        // Act
        byte[] data = packet.ToBytes();

        // Assert
        Assert.NotNull(data);
        Assert.True(data.Length > 0);
        
        // Check sync pattern
        Assert.Equal(0x77, data[0]);
        Assert.Equal(0x55, data[1]);
        
        // Check slot
        Assert.Equal(0, data[6]);
        
        // Check that CRC is present (last 2 bytes)
        Assert.True(data.Length >= 2);
    }

    [Fact]
    public void FromBytes_ToBytes_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var original = new DMRPacket
        {
            SyncPattern = new byte[] { 0x77, 0x55, 0xFD, 0x7D, 0xF7, 0x5F },
            Slot = 1,
            ColorCode = 5,
            Type = PacketType.Voice,
            CallType = CallType.Private,
            SourceId = 123456,
            DestinationId = 789012,
            SequenceNumber = 100,
            Payload = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF }
        };

        // Act
        byte[] data = original.ToBytes();
        var parsed = DMRPacket.FromBytes(data);

        // Assert
        Assert.Equal(original.Slot, parsed.Slot);
        Assert.Equal(original.ColorCode, parsed.ColorCode);
        Assert.Equal(original.Type, parsed.Type);
        Assert.Equal(original.CallType, parsed.CallType);
        Assert.Equal(original.SourceId, parsed.SourceId);
        Assert.Equal(original.DestinationId, parsed.DestinationId);
        Assert.Equal(original.SequenceNumber, parsed.SequenceNumber);
        Assert.Equal(original.Payload.Length, parsed.Payload.Length);
        Assert.Equal(original.Payload, parsed.Payload);
    }

    [Fact]
    public void ValidateCrc_WithValidPacket_ShouldReturnTrue()
    {
        // Arrange
        var packet = new DMRPacket
        {
            SyncPattern = new byte[] { 0x77, 0x55, 0xFD, 0x7D, 0xF7, 0x5F },
            Slot = 0,
            ColorCode = 1,
            Type = PacketType.Voice,
            CallType = CallType.Group,
            SourceId = 12345,
            DestinationId = 100,
            SequenceNumber = 1,
            Payload = new byte[] { 0x01, 0x02, 0x03 }
        };
        byte[] data = packet.ToBytes();

        // Act
        bool isValid = DMRPacket.ValidateCrc(data);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateCrc_WithCorruptedData_ShouldReturnFalse()
    {
        // Arrange
        var packet = new DMRPacket
        {
            SyncPattern = new byte[] { 0x77, 0x55, 0xFD, 0x7D, 0xF7, 0x5F },
            Slot = 0,
            ColorCode = 1,
            Type = PacketType.Voice,
            CallType = CallType.Group,
            SourceId = 12345,
            DestinationId = 100,
            SequenceNumber = 1,
            Payload = new byte[] { 0x01, 0x02, 0x03 }
        };
        byte[] data = packet.ToBytes();
        
        // Corrupt a byte in the middle
        data[10] ^= 0xFF;

        // Act
        bool isValid = DMRPacket.ValidateCrc(data);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void FromBytes_WithTooSmallPacket_ShouldThrowException()
    {
        // Arrange
        byte[] tooSmall = new byte[] { 0x01, 0x02, 0x03 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => DMRPacket.FromBytes(tooSmall));
    }

    [Fact]
    public void ToBytes_WithEmptyPayload_ShouldSucceed()
    {
        // Arrange
        var packet = new DMRPacket
        {
            SyncPattern = new byte[] { 0x77, 0x55, 0xFD, 0x7D, 0xF7, 0x5F },
            Slot = 0,
            ColorCode = 1,
            Type = PacketType.Control,
            CallType = CallType.Group,
            SourceId = 12345,
            DestinationId = 100,
            SequenceNumber = 1,
            Payload = Array.Empty<byte>()
        };

        // Act
        byte[] data = packet.ToBytes();

        // Assert
        Assert.NotNull(data);
        Assert.True(data.Length > 0);
        
        // Should still have valid CRC
        Assert.True(DMRPacket.ValidateCrc(data));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(15, 15)]
    public void ColorCode_ShouldBePreservedInSerialization(byte inputColorCode, byte expectedColorCode)
    {
        // Arrange
        var packet = new DMRPacket
        {
            SyncPattern = new byte[] { 0x77, 0x55, 0xFD, 0x7D, 0xF7, 0x5F },
            Slot = 0,
            ColorCode = inputColorCode,
            Type = PacketType.Voice,
            CallType = CallType.Group,
            SourceId = 12345,
            DestinationId = 100,
            SequenceNumber = 1
        };

        // Act
        byte[] data = packet.ToBytes();
        var parsed = DMRPacket.FromBytes(data);

        // Assert
        Assert.Equal(expectedColorCode, parsed.ColorCode);
    }

    [Theory]
    [InlineData(CallType.Private)]
    [InlineData(CallType.Group)]
    [InlineData(CallType.All)]
    public void CallType_ShouldBePreservedInSerialization(CallType callType)
    {
        // Arrange
        var packet = new DMRPacket
        {
            SyncPattern = new byte[] { 0x77, 0x55, 0xFD, 0x7D, 0xF7, 0x5F },
            Slot = 0,
            ColorCode = 1,
            Type = PacketType.Voice,
            CallType = callType,
            SourceId = 12345,
            DestinationId = 100,
            SequenceNumber = 1
        };

        // Act
        byte[] data = packet.ToBytes();
        var parsed = DMRPacket.FromBytes(data);

        // Assert
        Assert.Equal(callType, parsed.CallType);
    }
}
