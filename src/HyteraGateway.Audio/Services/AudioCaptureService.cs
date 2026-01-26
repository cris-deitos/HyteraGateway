using HyteraGateway.Audio.Codecs.Ambe;
using HyteraGateway.Audio.Processing;
using HyteraGateway.Audio.Streaming;
using Microsoft.Extensions.Logging;

namespace HyteraGateway.Audio.Services;

/// <summary>
/// Captures AMBE voice frames from radio and processes them
/// </summary>
public class AudioCaptureService
{
    private readonly IAmbeCodec _ambeCodec;
    private readonly AudioStreamManager _streamManager;
    private readonly ILogger<AudioCaptureService> _logger;
    
    public event EventHandler<AudioPacket>? AudioPacketReady;
    
    public AudioCaptureService(
        IAmbeCodec ambeCodec,
        AudioStreamManager streamManager,
        ILogger<AudioCaptureService> logger)
    {
        _ambeCodec = ambeCodec;
        _streamManager = streamManager;
        _logger = logger;
    }
    
    /// <summary>
    /// Process incoming AMBE voice frame from radio
    /// </summary>
    public async Task ProcessVoiceFrameAsync(byte[] ambeFrame, int radioId, int talkGroupId)
    {
        try
        {
            // Decode AMBE to PCM @ 8kHz
            byte[] pcm8kHz = _ambeCodec.DecodeToPcm(ambeFrame);
            
            // Resample to 48kHz for web streaming
            byte[] pcm48kHz = AudioPipeline.Resample(pcm8kHz, 48000);
            
            // Apply AGC
            pcm48kHz = AudioPipeline.ApplyAgc(pcm48kHz);
            
            // Encode to Opus
            byte[] opusData = _streamManager.EncodePcmToOpus(pcm48kHz);
            
            // Create packet
            var packet = new AudioPacket
            {
                Timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Data = opusData,
                Codec = AudioCodec.Opus
            };
            
            // Raise event for broadcasting
            AudioPacketReady?.Invoke(this, packet);
            
            _logger.LogTrace("Processed voice frame: {AmbeSize}B → {OpusSize}B", ambeFrame.Length, opusData.Length);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process voice frame");
        }
    }
}
