namespace HyteraGateway.Core.Interfaces;

/// <summary>
/// Service for capturing and processing audio from radios
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// Starts capturing audio from the radio
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StartCaptureAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops capturing audio
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StopCaptureAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Plays audio to the radio
    /// </summary>
    /// <param name="audioData">Audio data to play</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PlayAudioAsync(byte[] audioData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decodes AMBE+2 audio to PCM
    /// </summary>
    /// <param name="ambeData">AMBE+2 encoded data</param>
    /// <returns>PCM audio data</returns>
    byte[] DecodeAmbe(byte[] ambeData);

    /// <summary>
    /// Encodes PCM audio to AMBE+2
    /// </summary>
    /// <param name="pcmData">PCM audio data</param>
    /// <returns>AMBE+2 encoded data</returns>
    byte[] EncodeAmbe(byte[] pcmData);
}
