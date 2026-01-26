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
        byte[] bytes = null!;
        
        // Act
        var isValid = HyteraIPSCPacket.IsValidPacket(bytes);
        
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

    // New HyteraPacketValidator tests
    [Fact]
    public void Validator_ValidPacket_ReturnsNone()
    {
        // Arrange
        var packet = HyteraIPSCPacket.CreateKeepalive(9000001);
        var bytes = packet.ToBytes();
        
        // Act
        var result = HyteraPacketValidator.Validate(bytes);
        
        // Assert
        Assert.Equal(ValidationSeverity.None, result.Severity);
        Assert.Null(result.ErrorMessage);
        Assert.True(result.IsValid);
        Assert.True(result.IsPerfect);
    }

    [Fact]
    public void Validator_NullData_ReturnsError()
    {
        // Arrange
        byte[] bytes = null!;
        
        // Act
        var result = HyteraPacketValidator.Validate(bytes);
        
        // Assert
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Contains("null", result.ErrorMessage);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_TooShort_ReturnsError()
    {
        // Arrange
        var bytes = new byte[] { 0x50, 0x48, 0x00 };
        
        // Act
        var result = HyteraPacketValidator.Validate(bytes);
        
        // Assert
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Contains("too short", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_InvalidSignature_ReturnsError()
    {
        // Arrange
        var bytes = new byte[20];
        bytes[0] = 0xFF;
        bytes[1] = 0xFF;
        
        // Act
        var result = HyteraPacketValidator.Validate(bytes);
        
        // Assert
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Contains("signature", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_LengthTooSmall_ReturnsError()
    {
        // Arrange
        var bytes = new byte[20];
        bytes[0] = 0x50; // P
        bytes[1] = 0x48; // H
        bytes[6] = 10; // Length too small (< 19)
        bytes[7] = 0;
        
        // Act
        var result = HyteraPacketValidator.Validate(bytes);
        
        // Assert
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Contains("too small", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_LengthMismatch_ReturnsError()
    {
        // Arrange
        var packet = HyteraIPSCPacket.CreateKeepalive(9000001);
        var bytes = packet.ToBytes();
        
        // Corrupt length to not match actual
        bytes[6] = 100;
        
        // Act
        var result = HyteraPacketValidator.Validate(bytes);
        
        // Assert
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Contains("mismatch", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_CrcMismatch_ReturnsError()
    {
        // Arrange
        var packet = HyteraIPSCPacket.CreateKeepalive(9000001);
        var bytes = packet.ToBytes();
        
        // Corrupt CRC (last 2 bytes)
        bytes[^1] = 0xFF;
        bytes[^2] = 0xFF;
        
        // Act
        var result = HyteraPacketValidator.Validate(bytes);
        
        // Assert
        Assert.Equal(ValidationSeverity.Error, result.Severity);
        Assert.Contains("CRC", result.ErrorMessage);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_UnknownCommand_ReturnsWarning()
    {
        // Arrange
        var bytes = new byte[19];
        bytes[0] = 0x50; // P
        bytes[1] = 0x48; // H
        bytes[2] = 0xFF; // Unknown command
        bytes[3] = 0xFF;
        bytes[6] = 19; // Length
        bytes[7] = 0;
        
        // Calculate correct CRC
        var crc = HyteraIPSCPacket.CalculateCrc(bytes, 0, 17);
        bytes[17] = (byte)(crc & 0xFF);
        bytes[18] = (byte)(crc >> 8);
        
        // Act
        var result = HyteraPacketValidator.Validate(bytes);
        
        // Assert
        Assert.Equal(ValidationSeverity.Warning, result.Severity);
        Assert.Contains("Unknown command", result.ErrorMessage);
        Assert.True(result.IsValid); // Warnings still allow IsValid to be true
        Assert.False(result.IsPerfect);
    }

    [Fact]
    public void Validator_IsValid_ReturnsTrueForValidPacket()
    {
        // Arrange
        var packet = HyteraIPSCPacket.CreateKeepalive(9000001);
        var bytes = packet.ToBytes();
        
        // Act
        var isValid = HyteraPacketValidator.IsValid(bytes);
        
        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void Validator_IsValid_ReturnsFalseForInvalidPacket()
    {
        // Arrange
        var bytes = new byte[] { 0xFF, 0xFF };
        
        // Act
        var isValid = HyteraPacketValidator.IsValid(bytes);
        
        // Assert
        Assert.False(isValid);
    }
}

