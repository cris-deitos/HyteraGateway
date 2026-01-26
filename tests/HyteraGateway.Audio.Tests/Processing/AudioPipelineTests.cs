using HyteraGateway.Audio.Processing;

namespace HyteraGateway.Audio.Tests.Processing;

public class AudioPipelineTests
{
    [Fact]
    public void Resample_SameSampleRate_ReturnsSameData()
    {
        // Arrange
        byte[] pcmData = CreatePcmData(8000, 160);
        
        // Act
        byte[] result = AudioPipeline.Resample(pcmData, 8000);
        
        // Assert
        Assert.Equal(pcmData, result);
    }

    [Fact]
    public void ApplyAgc_SilentAudio_ReturnsSilent()
    {
        // Arrange
        byte[] silentPcm = new byte[320]; // All zeros
        
        // Act
        byte[] result = AudioPipeline.ApplyAgc(silentPcm);
        
        // Assert
        Assert.All(result, b => Assert.Equal(0, b));
    }

    [Fact]
    public void ApplyAgc_LowVolumeAudio_IncreasesVolume()
    {
        // Arrange
        byte[] lowVolumePcm = CreateLowVolumeAudio(160);
        float originalPeak = GetPeakLevel(lowVolumePcm);
        
        // Act
        byte[] result = AudioPipeline.ApplyAgc(lowVolumePcm);
        float resultPeak = GetPeakLevel(result);
        
        // Assert
        Assert.True(resultPeak > originalPeak, "AGC should increase volume of low-volume audio");
    }

    [Fact]
    public void Mix_NoStreams_ReturnsEmpty()
    {
        // Act
        byte[] result = AudioPipeline.Mix();
        
        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Mix_SingleStream_ReturnsSameStream()
    {
        // Arrange
        byte[] stream = CreatePcmData(8000, 160);
        
        // Act
        byte[] result = AudioPipeline.Mix(stream);
        
        // Assert
        Assert.Equal(stream, result);
    }

    [Fact]
    public void Mix_MultipleStreams_ReturnsCorrectLength()
    {
        // Arrange
        byte[] stream1 = CreatePcmData(8000, 160);
        byte[] stream2 = CreatePcmData(8000, 160);
        
        // Act
        byte[] result = AudioPipeline.Mix(stream1, stream2);
        
        // Assert
        Assert.Equal(Math.Max(stream1.Length, stream2.Length), result.Length);
    }

    private byte[] CreatePcmData(int sampleRate, int sampleCount)
    {
        byte[] data = new byte[sampleCount * 2];
        Random random = new Random(42);
        random.NextBytes(data);
        return data;
    }

    private byte[] CreateLowVolumeAudio(int sampleCount)
    {
        byte[] data = new byte[sampleCount * 2];
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = (short)(100 * Math.Sin(2 * Math.PI * i / sampleCount));
            BitConverter.GetBytes(sample).CopyTo(data, i * 2);
        }
        return data;
    }

    private float GetPeakLevel(byte[] pcmData)
    {
        float peak = 0;
        for (int i = 0; i < pcmData.Length / 2; i++)
        {
            short sample = BitConverter.ToInt16(pcmData, i * 2);
            float level = Math.Abs(sample / 32768f);
            if (level > peak)
                peak = level;
        }
        return peak;
    }
}
