using HyteraGateway.Radio.Protocol.Hytera;
using System.Text;
using Xunit;

namespace HyteraGateway.Radio.Tests.Protocol;

public class HyteraIPSCPacketTests
{
    [Fact]
    public void ToBytes_ShouldSerializePacketCorrectly()
    {
        // Arrange
        var packet = new HyteraIPSCPacket
        {
            Sequence = 1,
            Command = HyteraCommand.PTT_PRESS,
            Slot = 0,
            SourceId = 9000001,
            DestinationId = 100,
            Payload = new byte[] { 0x01, 0x02 }
        };

        // Act
        byte[] data = packet.ToBytes();

        // Assert
        Assert.NotNull(data);
        
        // Check signature "PH" (0x5048 big-endian)
        Assert.Equal(0x50, data[0]);
        Assert.Equal(0x48, data[1]);
        
        // Verify packet length is correct
        Assert.True(data.Length >= 19); // Minimum packet size
    }

    [Fact]
    public void FromBytes_ToBytes_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var original = new HyteraIPSCPacket
        {
            Sequence = 42,
            Command = HyteraCommand.GPS_RESPONSE,
            Slot = 1,
            SourceId = 123456,
            DestinationId = 9000001,
            Payload = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD }
        };

        // Act
        byte[] data = original.ToBytes();
        var parsed = HyteraIPSCPacket.FromBytes(data);

        // Assert
        Assert.Equal(original.Sequence, parsed.Sequence);
        Assert.Equal(original.Command, parsed.Command);
        Assert.Equal(original.Slot, parsed.Slot);
        Assert.Equal(original.SourceId, parsed.SourceId);
        Assert.Equal(original.DestinationId, parsed.DestinationId);
        Assert.Equal(original.Payload.Length, parsed.Payload.Length);
        Assert.Equal(original.Payload, parsed.Payload);
    }

    [Fact]
    public void FromBytes_WithInvalidSignature_ShouldThrowException()
    {
        // Arrange
        byte[] invalidData = new byte[21];
        invalidData[0] = 0xFF; // Invalid signature
        invalidData[1] = 0xFF;

        // Act & Assert
        var exception = Assert.Throws<InvalidDataException>(() => HyteraIPSCPacket.FromBytes(invalidData));
        Assert.Contains("Invalid packet", exception.Message);
    }

    [Fact]
    public void FromBytes_WithTooSmallPacket_ShouldThrowException()
    {
        // Arrange
        byte[] tooSmall = new byte[] { 0x50, 0x48, 0x01 };

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => HyteraIPSCPacket.FromBytes(tooSmall));
    }

    [Fact]
    public void CreateLogin_ShouldBuildValidLoginPacket()
    {
        // Arrange
        uint dispatcherId = 9000001;
        byte[] authData = Encoding.UTF8.GetBytes("password123");

        // Act
        var packet = HyteraIPSCPacket.CreateLogin(dispatcherId, authData);

        // Assert
        Assert.Equal(HyteraCommand.LOGIN, packet.Command);
        Assert.Equal(dispatcherId, packet.SourceId);
        Assert.Equal(authData, packet.Payload);
    }

    [Fact]
    public void CreateKeepalive_ShouldBuildValidKeepalivePacket()
    {
        // Arrange
        uint dispatcherId = 9000001;

        // Act
        var packet = HyteraIPSCPacket.CreateKeepalive(dispatcherId);

        // Assert
        Assert.Equal(HyteraCommand.KEEPALIVE, packet.Command);
        Assert.Equal(dispatcherId, packet.SourceId);
        Assert.Empty(packet.Payload);
    }

    [Fact]
    public void CreatePttPress_ShouldBuildValidPttPacket()
    {
        // Arrange
        uint sourceId = 9000001;
        uint destinationId = 100;
        byte slot = 0;

        // Act
        var packet = HyteraIPSCPacket.CreatePttPress(sourceId, destinationId, slot);

        // Assert
        Assert.Equal(HyteraCommand.PTT_PRESS, packet.Command);
        Assert.Equal(sourceId, packet.SourceId);
        Assert.Equal(destinationId, packet.DestinationId);
        Assert.Equal(slot, packet.Slot);
    }

    [Fact]
    public void CreatePttRelease_ShouldBuildValidPttPacket()
    {
        // Arrange
        uint sourceId = 9000001;
        uint destinationId = 100;
        byte slot = 1;

        // Act
        var packet = HyteraIPSCPacket.CreatePttRelease(sourceId, destinationId, slot);

        // Assert
        Assert.Equal(HyteraCommand.PTT_RELEASE, packet.Command);
        Assert.Equal(sourceId, packet.SourceId);
        Assert.Equal(destinationId, packet.DestinationId);
        Assert.Equal(slot, packet.Slot);
    }

    [Fact]
    public void CreateGpsRequest_ShouldBuildValidGpsPacket()
    {
        // Arrange
        uint sourceId = 9000001;
        uint targetId = 123456;

        // Act
        var packet = HyteraIPSCPacket.CreateGpsRequest(sourceId, targetId);

        // Assert
        Assert.Equal(HyteraCommand.GPS_REQUEST, packet.Command);
        Assert.Equal(sourceId, packet.SourceId);
        Assert.Equal(targetId, packet.DestinationId);
    }

    [Fact]
    public void CreateTextMessage_ShouldBuildValidTextMessagePacket()
    {
        // Arrange
        uint sourceId = 9000001;
        uint destinationId = 123456;
        string message = "Hello Radio!";

        // Act
        var packet = HyteraIPSCPacket.CreateTextMessage(sourceId, destinationId, message);

        // Assert
        Assert.Equal(HyteraCommand.TEXT_MESSAGE_SEND, packet.Command);
        Assert.Equal(sourceId, packet.SourceId);
        Assert.Equal(destinationId, packet.DestinationId);
        
        string decodedMessage = Encoding.UTF8.GetString(packet.Payload);
        Assert.Equal(message, decodedMessage);
    }

    [Fact]
    public void CreateDisconnect_ShouldBuildValidDisconnectPacket()
    {
        // Arrange
        uint dispatcherId = 9000001;

        // Act
        var packet = HyteraIPSCPacket.CreateDisconnect(dispatcherId);

        // Assert
        Assert.Equal(HyteraCommand.DISCONNECT, packet.Command);
        Assert.Equal(dispatcherId, packet.SourceId);
    }

    [Theory]
    [InlineData(HyteraCommand.PTT_PRESS)]
    [InlineData(HyteraCommand.GPS_REQUEST)]
    [InlineData(HyteraCommand.TEXT_MESSAGE_SEND)]
    [InlineData(HyteraCommand.EMERGENCY_DECLARE)]
    public void CommandTypes_ShouldBePreservedInSerialization(HyteraCommand command)
    {
        // Arrange
        var packet = new HyteraIPSCPacket
        {
            Command = command,
            SourceId = 12345,
            DestinationId = 100
        };

        // Act
        byte[] data = packet.ToBytes();
        var parsed = HyteraIPSCPacket.FromBytes(data);

        // Assert
        Assert.Equal(command, parsed.Command);
    }

    [Fact]
    public void ToBytes_WithLargePayload_ShouldHandleCorrectly()
    {
        // Arrange
        byte[] largePayload = new byte[1000];
        for (int i = 0; i < largePayload.Length; i++)
        {
            largePayload[i] = (byte)(i % 256);
        }

        var packet = new HyteraIPSCPacket
        {
            Command = HyteraCommand.VOICE_FRAME,
            SourceId = 12345,
            DestinationId = 100,
            Payload = largePayload
        };

        // Act
        byte[] data = packet.ToBytes();
        var parsed = HyteraIPSCPacket.FromBytes(data);

        // Assert
        Assert.Equal(largePayload.Length, parsed.Payload.Length);
        Assert.Equal(largePayload, parsed.Payload);
    }

    [Fact]
    public void SequenceNumber_ShouldIncrement()
    {
        // Arrange
        var packet1 = new HyteraIPSCPacket { Sequence = 1, Command = HyteraCommand.KEEPALIVE };
        var packet2 = new HyteraIPSCPacket { Sequence = 2, Command = HyteraCommand.KEEPALIVE };

        // Act
        byte[] data1 = packet1.ToBytes();
        byte[] data2 = packet2.ToBytes();
        var parsed1 = HyteraIPSCPacket.FromBytes(data1);
        var parsed2 = HyteraIPSCPacket.FromBytes(data2);

        // Assert
        Assert.Equal(1u, parsed1.Sequence);
        Assert.Equal(2u, parsed2.Sequence);
        Assert.True(parsed2.Sequence > parsed1.Sequence);
    }
}
