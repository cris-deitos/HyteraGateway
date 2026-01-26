using HyteraGateway.Radio.Protocol.DMR;
using Xunit;

namespace HyteraGateway.Radio.Tests.Protocol;

public class DMRVoiceFrameTests
{
    [Fact]
    public void FromAmbeData_WithValid33Bytes_ShouldSucceed()
    {
        // Arrange
        byte[] ambeData = new byte[33];
        for (int i = 0; i < 33; i++)
        {
            ambeData[i] = (byte)i;
        }

        // Act
        var frame = DMRVoiceFrame.FromAmbeData(ambeData, frameNumber: 3, isLastFrame: false);

        // Assert
        Assert.NotNull(frame);
        Assert.Equal(3, frame.FrameNumber);
        Assert.False(frame.IsLastFrame);
        Assert.Equal(33, frame.AmbeData.Length);
        Assert.Equal(ambeData, frame.AmbeData);
    }

    [Fact]
    public void FromAmbeData_WithInvalidSize_ShouldThrowException()
    {
        // Arrange
        byte[] invalidData = new byte[32]; // Wrong size

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            DMRVoiceFrame.FromAmbeData(invalidData));
        Assert.Contains("33 bytes", exception.Message);
    }

    [Fact]
    public void ToBytes_ShouldReturn33Bytes()
    {
        // Arrange
        byte[] ambeData = new byte[33];
        Array.Fill(ambeData, (byte)0xAA);
        var frame = DMRVoiceFrame.FromAmbeData(ambeData);

        // Act
        byte[] result = frame.ToBytes();

        // Assert
        Assert.Equal(33, result.Length);
        Assert.Equal(ambeData, result);
    }

    [Fact]
    public void FromBytes_WithValid33Bytes_ShouldSucceed()
    {
        // Arrange
        byte[] data = new byte[33];
        for (int i = 0; i < 33; i++)
        {
            data[i] = (byte)(i * 2);
        }

        // Act
        var frame = DMRVoiceFrame.FromBytes(data);

        // Assert
        Assert.NotNull(frame);
        Assert.Equal(33, frame.AmbeData.Length);
        Assert.Equal(data[0..33], frame.AmbeData);
    }

    [Fact]
    public void FromBytes_WithLargerArray_ShouldUseFirst33Bytes()
    {
        // Arrange
        byte[] data = new byte[100];
        for (int i = 0; i < 100; i++)
        {
            data[i] = (byte)i;
        }

        // Act
        var frame = DMRVoiceFrame.FromBytes(data);

        // Assert
        Assert.Equal(33, frame.AmbeData.Length);
        Assert.Equal(data[0..33], frame.AmbeData);
    }

    [Fact]
    public void FromBytes_WithTooSmallArray_ShouldThrowException()
    {
        // Arrange
        byte[] tooSmall = new byte[32];

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            DMRVoiceFrame.FromBytes(tooSmall));
        Assert.Contains("at least 33 bytes", exception.Message);
    }

    [Fact]
    public void IsLastFrame_ShouldBeSetCorrectly()
    {
        // Arrange
        byte[] ambeData = new byte[33];

        // Act
        var notLastFrame = DMRVoiceFrame.FromAmbeData(ambeData, isLastFrame: false);
        var lastFrame = DMRVoiceFrame.FromAmbeData(ambeData, isLastFrame: true);

        // Assert
        Assert.False(notLastFrame.IsLastFrame);
        Assert.True(lastFrame.IsLastFrame);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void FrameNumber_ShouldBeSetCorrectly(byte frameNumber)
    {
        // Arrange
        byte[] ambeData = new byte[33];

        // Act
        var frame = DMRVoiceFrame.FromAmbeData(ambeData, frameNumber: frameNumber);

        // Assert
        Assert.Equal(frameNumber, frame.FrameNumber);
    }

    [Fact]
    public void Timestamp_ShouldBeSetOnCreation()
    {
        // Arrange
        byte[] ambeData = new byte[33];
        var beforeCreation = DateTime.UtcNow;

        // Act
        var frame = DMRVoiceFrame.FromAmbeData(ambeData);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(frame.Timestamp >= beforeCreation);
        Assert.True(frame.Timestamp <= afterCreation);
    }

    [Fact]
    public void ToBytes_FromBytes_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        byte[] originalData = new byte[33];
        for (int i = 0; i < 33; i++)
        {
            originalData[i] = (byte)(i * 3);
        }
        var frame = DMRVoiceFrame.FromAmbeData(originalData);

        // Act
        byte[] serialized = frame.ToBytes();
        var deserialized = DMRVoiceFrame.FromBytes(serialized);

        // Assert
        Assert.Equal(originalData, deserialized.AmbeData);
    }
}
