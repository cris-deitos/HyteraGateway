namespace HyteraGateway.Audio.Codecs;

/// <summary>
/// Interface for Opus codec implementations
/// Used for high-quality audio encoding/decoding for WebSocket streaming
/// </summary>
public interface IOpusCodec : IDisposable
{
    /// <summary>
    /// Encode PCM audio to Opus format
    /// </summary>
    /// <param name="pcmData">PCM audio data (16-bit samples)</param>
    /// <returns>Opus encoded data</returns>
    byte[] Encode(byte[] pcmData);
    
    /// <summary>
    /// Decode Opus audio to PCM format
    /// </summary>
    /// <param name="opusData">Opus encoded data</param>
    /// <returns>PCM audio data (16-bit samples)</returns>
    byte[] Decode(byte[] opusData);
}
