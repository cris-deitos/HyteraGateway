using HyteraGateway.Audio.Codecs.Ambe;
using Microsoft.Extensions.Logging.Abstractions;

namespace HyteraGateway.Audio.Tests.Codecs;

public class MbelibAmbeCodecTests : IDisposable
{
    private readonly MbelibAmbeCodec? _codec;
    private readonly bool _mbelibAvailable;

    public MbelibAmbeCodecTests()
    {
        try
        {
            _codec = new MbelibAmbeCodec(NullLogger<MbelibAmbeCodec>.Instance);
            _mbelibAvailable = true;
        }
        catch (DllNotFoundException)
        {
            _mbelibAvailable = false;
            _codec = null;
        }
    }

    [Fact]
    public void DecodeToPcm_ValidFrame_ReturnsCorrectSize()
    {
        // Skip if mbelib is not available
        if (!_mbelibAvailable) return;
        
        // Arrange
        byte[] ambeFrame = new byte[33]; // Simulate AMBE frame
        
        // Act
        byte[] pcm = _codec!.DecodeToPcm(ambeFrame);
        
        // Assert
        Assert.Equal(320, pcm.Length); // 160 samples * 2 bytes
    }

    [Fact]
    public void DecodeToPcm_InvalidFrame_ReturnsSilence()
    {
        // Skip if mbelib is not available
        if (!_mbelibAvailable) return;
        
        // Arrange
        byte[] invalidFrame = new byte[10];
        
        // Act
        byte[] pcm = _codec!.DecodeToPcm(invalidFrame);
        
        // Assert
        Assert.Equal(320, pcm.Length);
        Assert.All(pcm, b => Assert.Equal(0, b));
    }

    [Fact]
    public void DecodeToPcm_NullFrame_ReturnsSilence()
    {
        // Skip if mbelib is not available
        if (!_mbelibAvailable) return;
        
        // Act
        byte[] pcm = _codec!.DecodeToPcm(null!);
        
        // Assert
        Assert.Equal(320, pcm.Length);
        Assert.All(pcm, b => Assert.Equal(0, b));
    }

    [Fact]
    public void EncodeFromPcm_ThrowsNotImplementedException()
    {
        // Skip if mbelib is not available
        if (!_mbelibAvailable) return;
        
        // Arrange
        byte[] pcmData = new byte[320];
        
        // Act & Assert
        Assert.Throws<NotImplementedException>(() => _codec!.EncodeFromPcm(pcmData));
    }

    [Fact]
    public void Constructor_InitializesSuccessfully()
    {
        // Skip if mbelib is not available
        if (!_mbelibAvailable) return;
        
        // Act
        using var codec = new MbelibAmbeCodec(NullLogger<MbelibAmbeCodec>.Instance);
        
        // Assert
        Assert.NotNull(codec);
    }

    public void Dispose()
    {
        _codec?.Dispose();
    }
}
