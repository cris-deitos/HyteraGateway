using Concentus.Structs;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace HyteraGateway.Audio.Streaming;

/// <summary>
/// Manages real-time audio streaming with Opus codec and jitter buffer
/// </summary>
public class AudioStreamManager : IDisposable
{
    private readonly ILogger<AudioStreamManager> _logger;
    private readonly OpusEncoder _opusEncoder;
    private readonly OpusDecoder _opusDecoder;
    private readonly ConcurrentQueue<AudioPacket> _jitterBuffer = new();
    
    private const int OPUS_SAMPLE_RATE = 48000;
    private const int OPUS_FRAME_SIZE = 960; // 20ms @ 48kHz
    private const int OPUS_CHANNELS = 1;
    
    public AudioStreamManager(ILogger<AudioStreamManager> logger)
    {
        _logger = logger;
        
        // Initialize Opus encoder/decoder
        _opusEncoder = new OpusEncoder(OPUS_SAMPLE_RATE, OPUS_CHANNELS, Concentus.Enums.OpusApplication.OPUS_APPLICATION_VOIP);
        _opusDecoder = new OpusDecoder(OPUS_SAMPLE_RATE, OPUS_CHANNELS);
        
        _logger.LogInformation("AudioStreamManager initialized (Opus @ 48kHz)");
    }
    
    /// <summary>
    /// Encode PCM to Opus for web streaming
    /// </summary>
    public byte[] EncodePcmToOpus(byte[] pcm48kHz)
    {
        // Convert bytes to short[]
        short[] pcmSamples = new short[pcm48kHz.Length / 2];
        Buffer.BlockCopy(pcm48kHz, 0, pcmSamples, 0, pcm48kHz.Length);
        
        // Encode to Opus
        byte[] opusPacket = new byte[4000]; // Max Opus packet size
        int encodedLength = _opusEncoder.Encode(pcmSamples, 0, OPUS_FRAME_SIZE, opusPacket, 0, opusPacket.Length);
        
        // Trim to actual size
        byte[] result = new byte[encodedLength];
        Array.Copy(opusPacket, result, encodedLength);
        
        return result;
    }
    
    /// <summary>
    /// Decode Opus to PCM
    /// </summary>
    public byte[] DecodeOpusToPcm(byte[] opusPacket)
    {
        short[] pcmSamples = new short[OPUS_FRAME_SIZE];
        int decodedSamples = _opusDecoder.Decode(opusPacket, 0, opusPacket.Length, pcmSamples, 0, OPUS_FRAME_SIZE, false);
        
        byte[] pcmBytes = new byte[decodedSamples * 2];
        Buffer.BlockCopy(pcmSamples, 0, pcmBytes, 0, pcmBytes.Length);
        
        return pcmBytes;
    }
    
    /// <summary>
    /// Add packet to jitter buffer
    /// </summary>
    public void BufferPacket(AudioPacket packet)
    {
        _jitterBuffer.Enqueue(packet);
        
        // Limit buffer size
        while (_jitterBuffer.Count > 10)
        {
            _jitterBuffer.TryDequeue(out _);
        }
    }
    
    /// <summary>
    /// Get next packet from jitter buffer
    /// </summary>
    public AudioPacket? GetNextPacket()
    {
        return _jitterBuffer.TryDequeue(out var packet) ? packet : null;
    }
    
    public void Dispose()
    {
        // Opus encoder/decoder don't need disposal in Concentus
    }
}

public class AudioPacket
{
    public uint Timestamp { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public AudioCodec Codec { get; set; }
}

public enum AudioCodec
{
    PCM,
    AMBE,
    Opus
}
