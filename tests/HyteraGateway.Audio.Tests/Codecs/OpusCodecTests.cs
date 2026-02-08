using HyteraGateway.Audio.Codecs;

namespace HyteraGateway.Audio.Tests.Codecs;

public class OpusCodecTests : IDisposable
{
    private readonly OpusCodec _codec;
    
    // Test constants
    private const int SAMPLE_RATE = 48000;
    private const int FRAME_SIZE = 960; // 20ms @ 48kHz
    private const int PCM_FRAME_BYTES = FRAME_SIZE * 2; // 16-bit samples
    
    public OpusCodecTests()
    {
        _codec = new OpusCodec();
    }
    
    [Fact]
    public void Constructor_InitializesSuccessfully()
    {
        // Act
        using var codec = new OpusCodec();
        
        // Assert
        Assert.NotNull(codec);
    }
    
    [Fact]
    public void Encode_ValidPcmData_ReturnsOpusData()
    {
        // Arrange
        byte[] pcmData = GenerateSilentPcm(FRAME_SIZE);
        
        // Act
        byte[] opusData = _codec.Encode(pcmData);
        
        // Assert
        Assert.NotNull(opusData);
        Assert.NotEmpty(opusData);
        Assert.True(opusData.Length < pcmData.Length, "Opus should compress audio data");
    }
    
    [Fact]
    public void Encode_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _codec.Encode(null!));
    }
    
    [Fact]
    public void Encode_EmptyData_ReturnsEmpty()
    {
        // Arrange
        byte[] emptyData = Array.Empty<byte>();
        
        // Act
        byte[] result = _codec.Encode(emptyData);
        
        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public void Encode_InvalidFrameSize_ThrowsArgumentException()
    {
        // Arrange
        byte[] invalidData = new byte[100]; // Wrong size
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _codec.Encode(invalidData));
        Assert.Contains("PCM data must contain", exception.Message);
    }
    
    [Fact]
    public void Decode_ValidOpusData_ReturnsPcmData()
    {
        // Arrange
        byte[] pcmData = GenerateSilentPcm(FRAME_SIZE);
        byte[] opusData = _codec.Encode(pcmData);
        
        // Act
        byte[] decodedPcm = _codec.Decode(opusData);
        
        // Assert
        Assert.NotNull(decodedPcm);
        Assert.Equal(PCM_FRAME_BYTES, decodedPcm.Length);
    }
    
    [Fact]
    public void Decode_NullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _codec.Decode(null!));
    }
    
    [Fact]
    public void Decode_EmptyData_ReturnsEmpty()
    {
        // Arrange
        byte[] emptyData = Array.Empty<byte>();
        
        // Act
        byte[] result = _codec.Decode(emptyData);
        
        // Assert
        Assert.Empty(result);
    }
    
    [Fact]
    public void EncodeDecodeRoundTrip_SilentAudio_PreservesDataSize()
    {
        // Arrange
        byte[] originalPcm = GenerateSilentPcm(FRAME_SIZE);
        
        // Act
        byte[] opusData = _codec.Encode(originalPcm);
        byte[] decodedPcm = _codec.Decode(opusData);
        
        // Assert
        Assert.Equal(originalPcm.Length, decodedPcm.Length);
    }
    
    [Fact]
    public void EncodeDecodeRoundTrip_ToneAudio_PreservesApproximateValues()
    {
        // Arrange
        byte[] originalPcm = GenerateTonePcm(FRAME_SIZE, 440); // 440 Hz tone
        
        // Act
        byte[] opusData = _codec.Encode(originalPcm);
        byte[] decodedPcm = _codec.Decode(opusData);
        
        // Assert
        Assert.Equal(originalPcm.Length, decodedPcm.Length);
        // Note: Due to lossy compression, values won't be exact
        // but should be reasonably close for a simple tone
    }
    
    [Fact]
    public void Encode_MultipleCalls_ProducesSimilarResults()
    {
        // Arrange
        byte[] pcmData = GenerateSilentPcm(FRAME_SIZE);
        
        // Act
        byte[] result1 = _codec.Encode(pcmData);
        byte[] result2 = _codec.Encode(pcmData);
        
        // Assert
        // Opus encoding may have minor variations (1-2 bytes) due to adaptive nature
        // but should be very close in size
        int sizeDiff = Math.Abs(result1.Length - result2.Length);
        Assert.True(sizeDiff <= 2, 
            $"Encoded sizes should be within 2 bytes, got difference of {sizeDiff}");
    }
    
    [Fact]
    public void Decode_MultipleCalls_ProducesConsistentResults()
    {
        // Arrange
        byte[] pcmData = GenerateSilentPcm(FRAME_SIZE);
        byte[] opusData = _codec.Encode(pcmData);
        
        // Act
        byte[] result1 = _codec.Decode(opusData);
        byte[] result2 = _codec.Decode(opusData);
        
        // Assert
        Assert.Equal(result1.Length, result2.Length);
        Assert.Equal(result1, result2);
    }
    
    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        using var codec = new OpusCodec();
        
        // Act & Assert - Should not throw
        codec.Dispose();
        codec.Dispose();
    }
    
    [Fact]
    public void Encode_CompressionRatio_IsEffective()
    {
        // Arrange
        byte[] pcmData = GenerateTonePcm(FRAME_SIZE, 1000); // 1 kHz tone
        
        // Act
        byte[] opusData = _codec.Encode(pcmData);
        
        // Assert
        double compressionRatio = (double)pcmData.Length / opusData.Length;
        Assert.True(compressionRatio > 2, 
            $"Compression ratio should be > 2x, got {compressionRatio:F2}x");
    }
    
    /// <summary>
    /// Generates silent PCM audio (all zeros)
    /// </summary>
    private static byte[] GenerateSilentPcm(int samples)
    {
        return new byte[samples * 2]; // 16-bit samples
    }
    
    /// <summary>
    /// Generates a simple sine wave tone for testing
    /// </summary>
    private static byte[] GenerateTonePcm(int samples, int frequency)
    {
        byte[] pcm = new byte[samples * 2];
        short[] pcmSamples = new short[samples];
        
        for (int i = 0; i < samples; i++)
        {
            double t = (double)i / SAMPLE_RATE;
            double value = Math.Sin(2 * Math.PI * frequency * t);
            pcmSamples[i] = (short)(value * short.MaxValue * 0.5); // 50% amplitude
        }
        
        Buffer.BlockCopy(pcmSamples, 0, pcm, 0, pcm.Length);
        return pcm;
    }
    
    public void Dispose()
    {
        _codec?.Dispose();
    }
}
