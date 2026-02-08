using Concentus.Enums;
using Concentus.Structs;

namespace HyteraGateway.Audio.Codecs;

/// <summary>
/// Opus codec for high-quality audio encoding/decoding using Concentus library
/// Used for WebSocket streaming to browser clients
/// </summary>
public class OpusCodec : IOpusCodec
{
    private readonly OpusEncoder _encoder;
    private readonly OpusDecoder _decoder;
    
    // Opus standard parameters for WebSocket streaming
    private const int SAMPLE_RATE = 48000; // Opus standard sample rate
    private const int CHANNELS = 1; // Mono audio
    private const int FRAME_SIZE = 960; // 20ms @ 48kHz
    private const int BITRATE = 24000; // 24 kbps for voice (good quality)
    
    /// <summary>
    /// Initializes a new instance of OpusCodec with VOIP-optimized settings
    /// </summary>
    public OpusCodec()
    {
        _encoder = new OpusEncoder(SAMPLE_RATE, CHANNELS, OpusApplication.OPUS_APPLICATION_VOIP);
        _encoder.Bitrate = BITRATE;
        
        _decoder = new OpusDecoder(SAMPLE_RATE, CHANNELS);
    }
    
    /// <summary>
    /// Encodes PCM audio to Opus format
    /// </summary>
    /// <param name="pcmData">PCM audio data (16-bit samples at 48kHz)</param>
    /// <returns>Opus encoded data</returns>
    /// <exception cref="ArgumentNullException">Thrown when pcmData is null</exception>
    /// <exception cref="ArgumentException">Thrown when pcmData length is invalid</exception>
    public byte[] Encode(byte[] pcmData)
    {
        if (pcmData == null)
            throw new ArgumentNullException(nameof(pcmData));
            
        if (pcmData.Length == 0)
            return Array.Empty<byte>();
        
        // Convert PCM bytes (16-bit) to short array
        int sampleCount = pcmData.Length / 2;
        if (sampleCount != FRAME_SIZE)
        {
            throw new ArgumentException(
                $"PCM data must contain {FRAME_SIZE} samples ({FRAME_SIZE * 2} bytes), got {pcmData.Length} bytes",
                nameof(pcmData));
        }
        
        short[] pcmSamples = new short[sampleCount];
        Buffer.BlockCopy(pcmData, 0, pcmSamples, 0, pcmData.Length);
        
        // Encode to Opus
        byte[] opusPacket = new byte[4000]; // Max Opus packet size
        int encodedLength = _encoder.Encode(pcmSamples, 0, FRAME_SIZE, opusPacket, 0, opusPacket.Length);
        
        // Return trimmed result
        byte[] result = new byte[encodedLength];
        Array.Copy(opusPacket, result, encodedLength);
        
        return result;
    }
    
    /// <summary>
    /// Decodes Opus audio to PCM format
    /// </summary>
    /// <param name="opusData">Opus encoded data</param>
    /// <returns>PCM audio data (16-bit samples at 48kHz)</returns>
    /// <exception cref="ArgumentNullException">Thrown when opusData is null</exception>
    public byte[] Decode(byte[] opusData)
    {
        if (opusData == null)
            throw new ArgumentNullException(nameof(opusData));
            
        if (opusData.Length == 0)
            return Array.Empty<byte>();
        
        // Decode Opus to PCM samples
        short[] pcmSamples = new short[FRAME_SIZE];
        int decodedSamples = _decoder.Decode(opusData, 0, opusData.Length, pcmSamples, 0, FRAME_SIZE, false);
        
        // Convert short array to bytes (16-bit PCM)
        byte[] pcmBytes = new byte[decodedSamples * 2];
        Buffer.BlockCopy(pcmSamples, 0, pcmBytes, 0, pcmBytes.Length);
        
        return pcmBytes;
    }
    
    /// <summary>
    /// Disposes the Opus encoder and decoder
    /// </summary>
    public void Dispose()
    {
        // Concentus OpusEncoder and OpusDecoder are structs and don't require disposal
        // This method is here to satisfy the IDisposable interface for future implementations
        GC.SuppressFinalize(this);
    }
}

