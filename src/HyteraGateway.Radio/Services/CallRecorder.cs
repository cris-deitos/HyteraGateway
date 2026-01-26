using HyteraGateway.Radio.Protocol.DMR;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System.Collections.Concurrent;
using System.Text.Json;

namespace HyteraGateway.Radio.Services;

/// <summary>
/// Records DMR voice calls by capturing AMBE+2 frames and converting to audio files
/// </summary>
public class CallRecorder : IDisposable
{
    private readonly ILogger<CallRecorder> _logger;
    private readonly ConcurrentDictionary<string, ActiveRecording> _activeRecordings = new();
    private readonly SemaphoreSlim _disposeLock = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Gets or sets the storage path for recordings
    /// </summary>
    public string StoragePath { get; set; } = "./recordings";

    /// <summary>
    /// Gets or sets the audio format (wav/mp3)
    /// </summary>
    public string Format { get; set; } = "wav";

    /// <summary>
    /// Gets or sets whether recording is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the CallRecorder
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public CallRecorder(ILogger<CallRecorder> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Starts recording a call
    /// </summary>
    /// <param name="callId">Unique call identifier</param>
    /// <param name="radioId">DMR radio ID</param>
    /// <param name="talkGroupId">Talk group ID</param>
    /// <param name="slot">DMR slot number (0 or 1)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if recording started successfully</returns>
    public async Task<bool> StartRecordingAsync(string callId, uint radioId, uint talkGroupId, byte slot, CancellationToken cancellationToken = default)
    {
        if (!Enabled)
        {
            _logger.LogDebug("Recording is disabled, skipping call {CallId}", callId);
            return false;
        }

        try
        {
            await _disposeLock.WaitAsync(cancellationToken);
            try
            {
                if (_disposed)
                {
                    _logger.LogWarning("Cannot start recording, CallRecorder is disposed");
                    return false;
                }

                if (_activeRecordings.ContainsKey(callId))
                {
                    _logger.LogWarning("Recording already active for call {CallId}", callId);
                    return false;
                }

                var recording = new ActiveRecording
                {
                    CallId = callId,
                    RadioId = radioId,
                    TalkGroupId = talkGroupId,
                    Slot = slot,
                    StartTime = DateTime.UtcNow
                };

                if (_activeRecordings.TryAdd(callId, recording))
                {
                    _logger.LogInformation("Started recording call {CallId} from radio {RadioId} on TG {TalkGroupId} slot {Slot}",
                        callId, radioId, talkGroupId, slot);
                    return true;
                }

                return false;
            }
            finally
            {
                _disposeLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting recording for call {CallId}", callId);
            return false;
        }
    }

    /// <summary>
    /// Appends an AMBE frame to the recording buffer
    /// </summary>
    /// <param name="callId">Call identifier</param>
    /// <param name="frame">DMR voice frame containing AMBE+2 data</param>
    public void AppendFrame(string callId, DMRVoiceFrame frame)
    {
        if (!Enabled || !_activeRecordings.TryGetValue(callId, out var recording))
        {
            return;
        }

        lock (recording.Frames)
        {
            recording.Frames.Add(frame);
        }
    }

    /// <summary>
    /// Stops recording and saves the call to disk
    /// </summary>
    /// <param name="callId">Call identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Path to the saved recording file, or null if failed</returns>
    public async Task<string?> StopRecordingAsync(string callId, CancellationToken cancellationToken = default)
    {
        if (!_activeRecordings.TryRemove(callId, out var recording))
        {
            _logger.LogWarning("No active recording found for call {CallId}", callId);
            return null;
        }

        recording.EndTime = DateTime.UtcNow;

        try
        {
            // Ensure storage directory exists
            Directory.CreateDirectory(StoragePath);

            // Generate filename: yyyyMMdd_HHmmss_{dmrId}_{talkGroupId}_slot{N}.{format}
            var timestamp = recording.StartTime.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{timestamp}_{recording.RadioId}_{recording.TalkGroupId}_slot{recording.Slot + 1}.{Format.ToLowerInvariant()}";
            var filePath = Path.Combine(StoragePath, fileName);

            // Convert AMBE frames to audio
            var success = await ConvertToAudioAsync(recording, filePath, cancellationToken);

            if (success)
            {
                // Save metadata
                await SaveMetadataAsync(recording, filePath, cancellationToken);

                _logger.LogInformation("Recording saved: {FilePath} ({FrameCount} frames, {Duration:mm\\:ss})",
                    filePath, recording.Frames.Count, recording.EndTime - recording.StartTime);

                return filePath;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording for call {CallId}", callId);
            return null;
        }
    }

    /// <summary>
    /// Converts AMBE frames to WAV audio file
    /// </summary>
    /// <param name="recording">Active recording data</param>
    /// <param name="filePath">Output file path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if conversion succeeded</returns>
    private async Task<bool> ConvertToAudioAsync(ActiveRecording recording, string filePath, CancellationToken cancellationToken)
    {
        try
        {
            // AMBE+2 in DMR is compressed audio that requires a vocoder to decode
            // For now, we'll create a placeholder WAV file with the raw AMBE data stored as metadata
            // A proper implementation would use an AMBE vocoder library to decode to PCM

            // Create a simple WAV file with 8kHz mono PCM (DMR standard)
            // Note: This creates a silent WAV as a placeholder. A real implementation needs AMBE decoder.
            var waveFormat = new WaveFormat(8000, 16, 1); // 8kHz, 16-bit, mono
            var duration = recording.EndTime - recording.StartTime;
            var sampleCount = (int)(waveFormat.SampleRate * duration.TotalSeconds);

            await Task.Run(() =>
            {
                using var writer = new WaveFileWriter(filePath, waveFormat);
                
                // Write silence as placeholder (in production, decode AMBE frames to PCM here)
                var silenceBuffer = new byte[sampleCount * 2]; // 16-bit = 2 bytes per sample
                writer.Write(silenceBuffer, 0, silenceBuffer.Length);
                
                // In a production system, you would:
                // 1. Use an AMBE+2 vocoder library (e.g., mbelib, md380tools)
                // 2. Decode each AMBE frame to PCM samples
                // 3. Write the decoded PCM to the WAV file
                
            }, cancellationToken);

            _logger.LogDebug("Converted {FrameCount} AMBE frames to WAV for call {CallId}",
                recording.Frames.Count, recording.CallId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting AMBE to audio for call {CallId}", recording.CallId);
            return false;
        }
    }

    /// <summary>
    /// Saves recording metadata to JSON file
    /// </summary>
    /// <param name="recording">Recording data</param>
    /// <param name="audioFilePath">Path to the audio file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task SaveMetadataAsync(ActiveRecording recording, string audioFilePath, CancellationToken cancellationToken)
    {
        try
        {
            var metadata = new
            {
                callId = recording.CallId,
                radioId = recording.RadioId,
                talkGroup = recording.TalkGroupId,
                slot = recording.Slot + 1, // Display as 1-based
                startTime = recording.StartTime,
                endTime = recording.EndTime,
                duration = (recording.EndTime - recording.StartTime).TotalSeconds,
                frameCount = recording.Frames.Count,
                audioFile = Path.GetFileName(audioFilePath)
            };

            var metadataPath = Path.ChangeExtension(audioFilePath, ".json");
            var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(metadataPath, json, cancellationToken);

            _logger.LogDebug("Saved metadata for call {CallId} to {MetadataPath}", recording.CallId, metadataPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving metadata for call {CallId}", recording.CallId);
        }
    }

    /// <summary>
    /// Gets the count of active recordings
    /// </summary>
    public int ActiveRecordingCount => _activeRecordings.Count;

    /// <summary>
    /// Disposes the CallRecorder and cleans up resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the CallRecorder
    /// </summary>
    /// <param name="disposing">True if disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _disposeLock.Wait();
            try
            {
                // Stop all active recordings
                foreach (var callId in _activeRecordings.Keys.ToList())
                {
                    _ = StopRecordingAsync(callId).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                _activeRecordings.Clear();
                _disposed = true;
            }
            finally
            {
                _disposeLock.Release();
                _disposeLock.Dispose();
            }
        }
    }

    /// <summary>
    /// Represents an active recording session
    /// </summary>
    private class ActiveRecording
    {
        public string CallId { get; set; } = string.Empty;
        public uint RadioId { get; set; }
        public uint TalkGroupId { get; set; }
        public byte Slot { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<DMRVoiceFrame> Frames { get; set; } = new();
    }
}
