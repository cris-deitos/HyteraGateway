using HyteraGateway.Radio.Protocol.DMR;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NAudio.Lame;
using System.Collections.Concurrent;
using System.Text.Json;
using HyteraGateway.Audio.Codecs.Ambe;
using HyteraGateway.Core.Models;
using HyteraGateway.Data.Repositories;

namespace HyteraGateway.Radio.Services;

/// <summary>
/// Records DMR voice calls by capturing AMBE+2 frames and converting to audio files
/// </summary>
public class CallRecorder : IDisposable
{
    private readonly ILogger<CallRecorder> _logger;
    private readonly IAmbeCodec? _ambeCodec;
    private readonly TransmissionRepository? _transmissionRepository;
    private readonly FtpClient? _ftpClient;
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
    /// Gets or sets whether to auto-upload recordings to FTP server
    /// </summary>
    public bool AutoUploadToFtp { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the CallRecorder
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="ambeCodec">Optional AMBE codec for audio decoding</param>
    /// <param name="transmissionRepository">Optional repository for database storage</param>
    /// <param name="ftpClient">Optional FTP client for auto-upload</param>
    public CallRecorder(
        ILogger<CallRecorder> logger, 
        IAmbeCodec? ambeCodec = null,
        TransmissionRepository? transmissionRepository = null,
        FtpClient? ftpClient = null)
    {
        _logger = logger;
        _ambeCodec = ambeCodec;
        _transmissionRepository = transmissionRepository;
        _ftpClient = ftpClient;
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

                // Save to database if repository is configured
                if (_transmissionRepository != null)
                {
                    try
                    {
                        var fileInfo = new FileInfo(filePath);
                        var callRecord = new CallRecord
                        {
                            Id = Guid.NewGuid(),
                            Slot = recording.Slot + 1, // Store as 1-based
                            CallerDmrId = (int)recording.RadioId,
                            CallerAlias = null, // Not available in current recording
                            TargetId = (int)recording.TalkGroupId,
                            CallType = Core.Models.CallType.Group, // Default to Group
                            StartTime = recording.StartTime,
                            EndTime = recording.EndTime,
                            Duration = (recording.EndTime - recording.StartTime).TotalSeconds,
                            AudioFilePath = filePath,
                            AudioFileSize = fileInfo.Length
                        };

                        await _transmissionRepository.InsertAsync(callRecord, cancellationToken);
                        _logger.LogDebug("Call record saved to database: {CallId}", callRecord.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save call record to database for {CallId}", callId);
                        // Continue - recording file is still saved
                    }
                }

                // Upload to FTP if configured and enabled
                if (AutoUploadToFtp && _ftpClient != null)
                {
                    try
                    {
                        var uploadSuccess = await _ftpClient.UploadFileAsync(filePath, cancellationToken);
                        if (uploadSuccess)
                        {
                            _logger.LogInformation("Recording uploaded to FTP: {FilePath}", filePath);
                            
                            // Also upload metadata file
                            var metadataPath = Path.ChangeExtension(filePath, ".json");
                            if (File.Exists(metadataPath))
                            {
                                await _ftpClient.UploadFileAsync(metadataPath, cancellationToken);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Failed to upload recording to FTP: {FilePath}", filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading recording to FTP: {FilePath}", filePath);
                        // Continue - recording file is still saved locally
                    }
                }

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
    /// Converts AMBE frames to WAV or MP3 audio file
    /// </summary>
    /// <param name="recording">Active recording data</param>
    /// <param name="filePath">Output file path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if conversion succeeded</returns>
    private async Task<bool> ConvertToAudioAsync(ActiveRecording recording, string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var waveFormat = new WaveFormat(8000, 16, 1); // 8kHz, 16-bit, mono
            bool isMp3 = Format.ToLowerInvariant() == "mp3";

            await Task.Run(() =>
            {
                // First, decode AMBE frames to PCM and create WAV in memory
                byte[] wavBytes;
                using (var memoryStream = new MemoryStream())
                {
                    using (var tempWriter = new WaveFileWriter(memoryStream, waveFormat))
                    {
                        if (_ambeCodec != null && recording.Frames.Count > 0)
                        {
                            _logger.LogDebug("Decoding {FrameCount} AMBE frames using codec", recording.Frames.Count);
                            
                            foreach (var frame in recording.Frames)
                            {
                                try
                                {
                                    byte[] pcmData = _ambeCodec.DecodeToPcm(frame.AmbeData);
                                    
                                    if (pcmData != null && pcmData.Length > 0)
                                    {
                                        tempWriter.Write(pcmData, 0, pcmData.Length);
                                    }
                                    else
                                    {
                                        byte[] silence = new byte[320];
                                        tempWriter.Write(silence, 0, silence.Length);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Failed to decode AMBE frame, writing silence");
                                    byte[] silence = new byte[320];
                                    tempWriter.Write(silence, 0, silence.Length);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("No AMBE codec available or no frames, creating silent audio");
                            var duration = recording.EndTime - recording.StartTime;
                            var sampleCount = (int)(waveFormat.SampleRate * duration.TotalSeconds);
                            var silenceBuffer = new byte[sampleCount * 2];
                            tempWriter.Write(silenceBuffer, 0, silenceBuffer.Length);
                        }
                    } // tempWriter is disposed here, flushes WAV header
                    
                    // Get the WAV bytes before memoryStream is disposed
                    wavBytes = memoryStream.ToArray();
                }

                // Now write to final format
                if (isMp3)
                {
                    // Convert to MP3 using LAME
                    using var wavStream = new MemoryStream(wavBytes);
                    using var reader = new WaveFileReader(wavStream);
                    using var mp3Writer = new LameMP3FileWriter(filePath, reader.WaveFormat, LAMEPreset.STANDARD);
                    reader.CopyTo(mp3Writer);
                    _logger.LogDebug("Saved recording as MP3: {FilePath}", filePath);
                }
                else
                {
                    // Save as WAV
                    File.WriteAllBytes(filePath, wavBytes);
                    _logger.LogDebug("Saved recording as WAV: {FilePath}", filePath);
                }
                
            }, cancellationToken);

            _logger.LogDebug("Converted {FrameCount} AMBE frames to {Format} for call {CallId}",
                recording.Frames.Count, Format.ToUpperInvariant(), recording.CallId);

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
