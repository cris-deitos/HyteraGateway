using HyteraGateway.Radio.Protocol.DMR;
using HyteraGateway.Radio.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HyteraGateway.Radio.Tests.Services;

public class CallRecorderTests : IDisposable
{
    private readonly Mock<ILogger<CallRecorder>> _mockLogger;
    private readonly CallRecorder _recorder;
    private readonly string _testStoragePath;

    public CallRecorderTests()
    {
        _mockLogger = new Mock<ILogger<CallRecorder>>();
        _recorder = new CallRecorder(_mockLogger.Object);
        
        // Use a temporary directory for tests
        _testStoragePath = Path.Combine(Path.GetTempPath(), $"CallRecorderTests_{Guid.NewGuid()}");
        _recorder.StoragePath = _testStoragePath;
    }

    [Fact]
    public async Task StartRecordingAsync_ValidCall_ReturnsTrue()
    {
        // Arrange
        var callId = "test-call-1";
        uint radioId = 1234567;
        uint talkGroupId = 9;
        byte slot = 0;

        // Act
        var result = await _recorder.StartRecordingAsync(callId, radioId, talkGroupId, slot);

        // Assert
        Assert.True(result);
        Assert.Equal(1, _recorder.ActiveRecordingCount);
    }

    [Fact]
    public async Task StartRecordingAsync_DuplicateCall_ReturnsFalse()
    {
        // Arrange
        var callId = "test-call-1";
        uint radioId = 1234567;
        uint talkGroupId = 9;
        byte slot = 0;

        await _recorder.StartRecordingAsync(callId, radioId, talkGroupId, slot);

        // Act
        var result = await _recorder.StartRecordingAsync(callId, radioId, talkGroupId, slot);

        // Assert
        Assert.False(result);
        Assert.Equal(1, _recorder.ActiveRecordingCount);
    }

    [Fact]
    public async Task StartRecordingAsync_WhenDisabled_ReturnsFalse()
    {
        // Arrange
        _recorder.Enabled = false;
        var callId = "test-call-1";
        uint radioId = 1234567;
        uint talkGroupId = 9;
        byte slot = 0;

        // Act
        var result = await _recorder.StartRecordingAsync(callId, radioId, talkGroupId, slot);

        // Assert
        Assert.False(result);
        Assert.Equal(0, _recorder.ActiveRecordingCount);
    }

    [Fact]
    public async Task AppendFrame_ActiveRecording_AddsFrameToBuffer()
    {
        // Arrange
        var callId = "test-call-1";
        uint radioId = 1234567;
        uint talkGroupId = 9;
        byte slot = 0;

        await _recorder.StartRecordingAsync(callId, radioId, talkGroupId, slot);

        var ambeData = new byte[33];
        Array.Fill<byte>(ambeData, 0xAA);
        var frame = DMRVoiceFrame.FromAmbeData(ambeData);

        // Act
        _recorder.AppendFrame(callId, frame);

        // Assert - no exception means success
        Assert.Equal(1, _recorder.ActiveRecordingCount);
    }

    [Fact]
    public void AppendFrame_NoActiveRecording_DoesNotThrow()
    {
        // Arrange
        var callId = "non-existent-call";
        var ambeData = new byte[33];
        var frame = DMRVoiceFrame.FromAmbeData(ambeData);

        // Act & Assert - should not throw
        _recorder.AppendFrame(callId, frame);
    }

    [Fact]
    public async Task StopRecordingAsync_ActiveRecording_SavesFile()
    {
        // Arrange
        var callId = "test-call-1";
        uint radioId = 1234567;
        uint talkGroupId = 9;
        byte slot = 0;

        await _recorder.StartRecordingAsync(callId, radioId, talkGroupId, slot);

        // Add some frames
        for (int i = 0; i < 5; i++)
        {
            var ambeData = new byte[33];
            Array.Fill<byte>(ambeData, (byte)i);
            var frame = DMRVoiceFrame.FromAmbeData(ambeData);
            _recorder.AppendFrame(callId, frame);
        }

        // Small delay to ensure different timestamps
        await Task.Delay(100);

        // Act
        var filePath = await _recorder.StopRecordingAsync(callId);

        // Assert
        Assert.NotNull(filePath);
        Assert.True(File.Exists(filePath));
        Assert.Equal(0, _recorder.ActiveRecordingCount);

        // Verify metadata file exists
        var metadataPath = Path.ChangeExtension(filePath, ".json");
        Assert.True(File.Exists(metadataPath));

        // Verify metadata content
        var metadataJson = await File.ReadAllTextAsync(metadataPath);
        Assert.Contains($"\"callId\": \"{callId}\"", metadataJson);
        Assert.Contains($"\"radioId\": {radioId}", metadataJson);
        Assert.Contains($"\"talkGroup\": {talkGroupId}", metadataJson);
        Assert.Contains($"\"slot\": {slot + 1}", metadataJson);
        Assert.Contains("\"frameCount\": 5", metadataJson);
    }

    [Fact]
    public async Task StopRecordingAsync_NoActiveRecording_ReturnsNull()
    {
        // Arrange
        var callId = "non-existent-call";

        // Act
        var filePath = await _recorder.StopRecordingAsync(callId);

        // Assert
        Assert.Null(filePath);
    }

    [Fact]
    public async Task StopRecordingAsync_CreatesCorrectFilename()
    {
        // Arrange
        var callId = "test-call-1";
        uint radioId = 1234567;
        uint talkGroupId = 9;
        byte slot = 1; // Slot 2 (0-indexed)

        await _recorder.StartRecordingAsync(callId, radioId, talkGroupId, slot);
        await Task.Delay(100);

        // Act
        var filePath = await _recorder.StopRecordingAsync(callId);

        // Assert
        Assert.NotNull(filePath);
        var fileName = Path.GetFileName(filePath);
        
        // Verify format: yyyyMMdd_HHmmss_{dmrId}_{talkGroupId}_slot{N}.{format}
        Assert.Matches(@"\d{8}_\d{6}_1234567_9_slot2\.wav", fileName);
    }

    [Fact]
    public async Task StopRecordingAsync_WithMp3Format_CreatesMp3File()
    {
        // Arrange
        _recorder.Format = "mp3";
        var callId = "test-call-1";
        uint radioId = 1234567;
        uint talkGroupId = 9;
        byte slot = 0;

        await _recorder.StartRecordingAsync(callId, radioId, talkGroupId, slot);
        await Task.Delay(100);

        // Act
        var filePath = await _recorder.StopRecordingAsync(callId);

        // Assert
        Assert.NotNull(filePath);
        Assert.EndsWith(".mp3", filePath);
    }

    [Fact]
    public async Task ActiveRecordingCount_TracksActiveRecordings()
    {
        // Arrange & Act
        Assert.Equal(0, _recorder.ActiveRecordingCount);

        await _recorder.StartRecordingAsync("call-1", 1234567, 9, 0);
        Assert.Equal(1, _recorder.ActiveRecordingCount);

        await _recorder.StartRecordingAsync("call-2", 7654321, 10, 1);
        Assert.Equal(2, _recorder.ActiveRecordingCount);

        await _recorder.StopRecordingAsync("call-1");
        Assert.Equal(1, _recorder.ActiveRecordingCount);

        await _recorder.StopRecordingAsync("call-2");
        Assert.Equal(0, _recorder.ActiveRecordingCount);
    }

    [Fact]
    public async Task Dispose_StopsAllActiveRecordings()
    {
        // Arrange
        await _recorder.StartRecordingAsync("call-1", 1234567, 9, 0);
        await _recorder.StartRecordingAsync("call-2", 7654321, 10, 1);
        Assert.Equal(2, _recorder.ActiveRecordingCount);

        // Act
        _recorder.Dispose();

        // Assert
        Assert.Equal(0, _recorder.ActiveRecordingCount);
    }

    [Fact]
    public async Task StartRecordingAsync_AfterDispose_ReturnsFalse()
    {
        // Arrange
        _recorder.Dispose();

        // Act
        var result = await _recorder.StartRecordingAsync("call-1", 1234567, 9, 0);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task MultipleFrames_PreservesOrder()
    {
        // Arrange
        var callId = "test-call-1";
        await _recorder.StartRecordingAsync(callId, 1234567, 9, 0);

        // Act
        for (int i = 0; i < 10; i++)
        {
            var ambeData = new byte[33];
            Array.Fill<byte>(ambeData, (byte)i);
            var frame = DMRVoiceFrame.FromAmbeData(ambeData, (byte)i);
            _recorder.AppendFrame(callId, frame);
        }

        await Task.Delay(100);
        var filePath = await _recorder.StopRecordingAsync(callId);

        // Assert
        Assert.NotNull(filePath);
        var metadataPath = Path.ChangeExtension(filePath, ".json");
        var metadataJson = await File.ReadAllTextAsync(metadataPath);
        Assert.Contains("\"frameCount\": 10", metadataJson);
    }

    [Fact]
    public async Task StoragePath_CreatesDirectory()
    {
        // Arrange
        var customPath = Path.Combine(Path.GetTempPath(), $"CustomRecordings_{Guid.NewGuid()}");
        _recorder.StoragePath = customPath;

        // Act
        await _recorder.StartRecordingAsync("call-1", 1234567, 9, 0);
        await Task.Delay(100);
        await _recorder.StopRecordingAsync("call-1");

        // Assert
        Assert.True(Directory.Exists(customPath));

        // Cleanup
        Directory.Delete(customPath, true);
    }

    public void Dispose()
    {
        _recorder?.Dispose();

        // Cleanup test directory
        if (Directory.Exists(_testStoragePath))
        {
            try
            {
                Directory.Delete(_testStoragePath, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
