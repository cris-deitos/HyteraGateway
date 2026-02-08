using HyteraGateway.Audio.Codecs.Ambe;
using Microsoft.Extensions.Logging;

namespace HyteraGateway.Audio.Services;

/// <summary>
/// Service for playing audio to Hytera radios
/// </summary>
public class AudioPlaybackService
{
    private readonly ILogger<AudioPlaybackService>? _logger;
    private readonly AmbeCodec _ambeCodec;
    
    /// <summary>
    /// Initializes a new instance of AudioPlaybackService
    /// </summary>
    /// <param name="logger">Optional logger instance</param>
    public AudioPlaybackService(ILogger<AudioPlaybackService>? logger = null)
    {
        _logger = logger;
        _ambeCodec = new AmbeCodec();
    }
    
    /// <summary>
    /// Plays audio to the radio
    /// </summary>
    /// <param name="audioData">Audio data to play (PCM format)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task PlayAudioAsync(byte[] audioData, CancellationToken cancellationToken = default)
    {
        if (audioData == null || audioData.Length == 0)
        {
            _logger?.LogWarning("PlayAudioAsync called with empty audio data");
            return;
        }
        
        _logger?.LogDebug("Playing {Length} bytes of audio", audioData.Length);
        
        // Audio playback is handled by the radio hardware
        // We just need to send the encoded audio packets
        // The actual transmission happens via HyteraConnection.SendAudioAsync
        
        // For now, simulate playback timing based on audio duration
        // PCM 8kHz 16-bit mono = 16000 bytes/second
        int durationMs = audioData.Length * 1000 / 16000;
        
        if (durationMs > 0)
        {
            // Limit maximum duration to 5 seconds to prevent blocking
            await Task.Delay(Math.Min(durationMs, 5000), cancellationToken);
        }
        
        _logger?.LogDebug("Audio playback completed");
    }
}
