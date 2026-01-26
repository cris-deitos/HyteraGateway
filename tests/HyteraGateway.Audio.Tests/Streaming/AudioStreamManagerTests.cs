using HyteraGateway.Audio.Streaming;
using Microsoft.Extensions.Logging.Abstractions;

namespace HyteraGateway.Audio.Tests.Streaming;

public class AudioStreamManagerTests : IDisposable
{
    private readonly AudioStreamManager _manager;

    public AudioStreamManagerTests()
    {
        _manager = new AudioStreamManager(NullLogger<AudioStreamManager>.Instance);
    }

    [Fact]
    public void EncodePcmToOpus_ValidPcm_ReturnsOpusData()
    {
        // Arrange
        byte[] pcm48kHz = CreatePcmData(960); // 20ms @ 48kHz
        
        // Act
        byte[] opus = _manager.EncodePcmToOpus(pcm48kHz);
        
        // Assert
        Assert.NotNull(opus);
        Assert.True(opus.Length > 0);
        Assert.True(opus.Length < pcm48kHz.Length); // Opus should compress
    }

    [Fact]
    public void DecodeOpusToPcm_ValidOpus_ReturnsPcmData()
    {
        // Arrange
        byte[] pcm48kHz = CreatePcmData(960);
        byte[] opus = _manager.EncodePcmToOpus(pcm48kHz);
        
        // Act
        byte[] decodedPcm = _manager.DecodeOpusToPcm(opus);
        
        // Assert
        Assert.NotNull(decodedPcm);
        Assert.True(decodedPcm.Length > 0);
    }

    [Fact]
    public void BufferPacket_AddsPacketToBuffer()
    {
        // Arrange
        var packet = new AudioPacket
        {
            Timestamp = 12345,
            Data = new byte[] { 1, 2, 3, 4 },
            Codec = AudioCodec.Opus
        };
        
        // Act
        _manager.BufferPacket(packet);
        var retrieved = _manager.GetNextPacket();
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(packet.Timestamp, retrieved.Timestamp);
    }

    [Fact]
    public void GetNextPacket_EmptyBuffer_ReturnsNull()
    {
        // Act
        var packet = _manager.GetNextPacket();
        
        // Assert
        Assert.Null(packet);
    }

    [Fact]
    public void BufferPacket_ExceedsMaxSize_DropsOldestPackets()
    {
        // Arrange - Add more than 10 packets
        for (int i = 0; i < 15; i++)
        {
            _manager.BufferPacket(new AudioPacket
            {
                Timestamp = (uint)i,
                Data = new byte[] { (byte)i },
                Codec = AudioCodec.Opus
            });
        }
        
        // Act - Count remaining packets
        int count = 0;
        while (_manager.GetNextPacket() != null)
        {
            count++;
        }
        
        // Assert - Should have max 10 packets
        Assert.True(count <= 10);
    }

    private byte[] CreatePcmData(int sampleCount)
    {
        byte[] data = new byte[sampleCount * 2];
        Random random = new Random(42);
        
        // Generate a simple sine wave
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = (short)(5000 * Math.Sin(2 * Math.PI * 440 * i / 48000));
            BitConverter.GetBytes(sample).CopyTo(data, i * 2);
        }
        
        return data;
    }

    public void Dispose()
    {
        _manager?.Dispose();
    }
}
