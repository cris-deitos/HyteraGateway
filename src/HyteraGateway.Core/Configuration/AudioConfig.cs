namespace HyteraGateway.Core.Configuration;

/// <summary>
/// Configuration for audio processing
/// </summary>
public class AudioConfig
{
    /// <summary>
    /// Enable audio recording
    /// </summary>
    public bool EnableRecording { get; set; } = true;

    /// <summary>
    /// Audio recording directory
    /// </summary>
    public string RecordingDirectory { get; set; } = "recordings";

    /// <summary>
    /// Audio file format (wav, mp3, opus)
    /// </summary>
    public string FileFormat { get; set; } = "wav";

    /// <summary>
    /// Sample rate in Hz
    /// </summary>
    public int SampleRate { get; set; } = 8000;

    /// <summary>
    /// Bits per sample
    /// </summary>
    public int BitsPerSample { get; set; } = 16;

    /// <summary>
    /// Number of channels (1=mono, 2=stereo)
    /// </summary>
    public int Channels { get; set; } = 1;

    /// <summary>
    /// Enable audio streaming via WebSocket
    /// </summary>
    public bool EnableStreaming { get; set; } = true;

    /// <summary>
    /// Codec for streaming (ambe, opus, pcm)
    /// </summary>
    public string StreamingCodec { get; set; } = "opus";

    /// <summary>
    /// Delete recordings older than X days (0 = never delete)
    /// </summary>
    public int RetentionDays { get; set; } = 30;
}
