using HyteraGateway.Radio.Protocol.Hytera;

namespace HyteraGateway.Radio.Tests.Protocol;

public class PacketValidationTests
{
    [Fact]
    public void IsValidPacket_ValidPacket_ReturnsTrue()
    {
        // Arrange
        var packet = HyteraIPSCPacket.CreateKeepalive(9000001);
        var bytes = packet.ToBytes();
        
        // Act
        var isValid = HyteraIPSCPacket.IsValidPacket(bytes);
        
        // Assert
        Assert.True(isValid);
    }
    
    [Fact]
    public void IsValidPacket_InvalidSignature_ReturnsFalse()
    {
        // Arrange
        var bytes = new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00 };
        
        // Act
        var isValid = HyteraIPSCPacket.IsValidPacket(bytes);
        
        // Assert
        Assert.False(isValid);
    }
    
    [Fact]
    public void IsValidPacket_TooShort_ReturnsFalse()
    {
        // Arrange
        var bytes = new byte[] { 0x50, 0x48 };
        
        // Act
        var isValid = HyteraIPSCPacket.IsValidPacket(bytes);
        
        // Assert
        Assert.False(isValid);
    }
    
    [Fact]
    public void IsValidPacket_NullData_ReturnsFalse()
    {
        // Arrange
        byte[]? bytes = null;
        
        // Act
        var isValid = HyteraIPSCPacket.IsValidPacket(bytes!);
        
        // Assert
        Assert.False(isValid);
    }
    
    [Fact]
    public void IsValidPacket_WrongLength_ReturnsFalse()
    {
        // Arrange
        var packet = HyteraIPSCPacket.CreateKeepalive(9000001);
        var bytes = packet.ToBytes();
        
        // Corrupt the length field (at offset 6)
        bytes[6] = 0xFF;
        bytes[7] = 0xFF;
        
        // Act
        var isValid = HyteraIPSCPacket.IsValidPacket(bytes);
        
        // Assert
        Assert.False(isValid);
    }
    
    [Fact]
    public void FromBytes_InvalidPacket_ThrowsInvalidDataException()
    {
        // Arrange
        var bytes = new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00 };
        
        // Act & Assert
        Assert.Throws<InvalidDataException>(() => HyteraIPSCPacket.FromBytes(bytes));
    }
    
    [Fact]
    public void FromBytes_ValidPacket_DoesNotThrow()
    {
        // Arrange
        var packet = HyteraIPSCPacket.CreateKeepalive(9000001);
        var bytes = packet.ToBytes();
        
        // Act & Assert - Should not throw
        var parsed = HyteraIPSCPacket.FromBytes(bytes);
        Assert.NotNull(parsed);
    }
}
