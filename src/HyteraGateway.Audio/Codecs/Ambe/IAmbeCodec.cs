namespace HyteraGateway.Audio.Codecs.Ambe;

/// <summary>
/// Interface for AMBE codec implementations
/// </summary>
public interface IAmbeCodec
{
    /// <summary>
    /// Decode AMBE frame to PCM samples
    /// </summary>
    /// <param name="ambeFrame">AMBE encoded frame</param>
    /// <returns>PCM audio data (16-bit samples)</returns>
    byte[] DecodeToPcm(byte[] ambeFrame);
    
    /// <summary>
    /// Encode PCM samples to AMBE frame
    /// </summary>
    /// <param name="pcmData">PCM audio data (16-bit samples)</param>
    /// <returns>AMBE encoded frame</returns>
    byte[] EncodeFromPcm(byte[] pcmData);
}
