using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace HyteraGateway.Audio.Processing;

/// <summary>
/// Audio processing pipeline for format conversion and effects
/// </summary>
public class AudioPipeline
{
    private const int WAV_HEADER_SIZE = 44; // Standard WAV file header size in bytes
    
    /// <summary>
    /// Resample PCM from 8kHz to target sample rate
    /// </summary>
    public static byte[] Resample(byte[] input8kHz, int targetSampleRate)
    {
        if (targetSampleRate == 8000)
            return input8kHz;
        
        using var inputStream = new RawSourceWaveStream(
            new MemoryStream(input8kHz), 
            new WaveFormat(8000, 16, 1));
        
        var resampler = new WdlResamplingSampleProvider(
            inputStream.ToSampleProvider(), 
            targetSampleRate);
        
        using var outputStream = new MemoryStream();
        WaveFileWriter.WriteWavFileToStream(outputStream, resampler.ToWaveProvider16());
        
        // Extract PCM data (skip WAV header)
        outputStream.Position = WAV_HEADER_SIZE;
        return outputStream.ToArray()[WAV_HEADER_SIZE..];
    }
    
    /// <summary>
    /// Apply Automatic Gain Control (AGC)
    /// </summary>
    public static byte[] ApplyAgc(byte[] pcmData, float targetLevel = 0.8f)
    {
        // Convert bytes to float samples
        int sampleCount = pcmData.Length / 2;
        float[] samples = new float[sampleCount];
        
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(pcmData, i * 2);
            samples[i] = sample / 32768f;
        }
        
        // Find peak
        float peak = samples.Max(Math.Abs);
        
        // Calculate gain
        float gain = peak > 0 ? Math.Min(targetLevel / peak, 4.0f) : 1.0f;
        
        // Apply gain
        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] *= gain;
            samples[i] = Math.Clamp(samples[i], -1.0f, 1.0f);
        }
        
        // Convert back to bytes
        byte[] output = new byte[pcmData.Length];
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = (short)(samples[i] * 32767);
            BitConverter.GetBytes(sample).CopyTo(output, i * 2);
        }
        
        return output;
    }
    
    /// <summary>
    /// Mix multiple audio streams
    /// </summary>
    public static byte[] Mix(params byte[][] streams)
    {
        if (streams.Length == 0)
            return Array.Empty<byte>();
        
        if (streams.Length == 1)
            return streams[0];
        
        int maxLength = streams.Max(s => s.Length);
        float[] mixed = new float[maxLength / 2];
        
        foreach (var stream in streams)
        {
            for (int i = 0; i < stream.Length / 2; i++)
            {
                short sample = BitConverter.ToInt16(stream, i * 2);
                mixed[i] += sample / 32768f;
            }
        }
        
        // Normalize
        float peak = mixed.Max(Math.Abs);
        if (peak > 1.0f)
        {
            for (int i = 0; i < mixed.Length; i++)
                mixed[i] /= peak;
        }
        
        // Convert to bytes
        byte[] output = new byte[maxLength];
        for (int i = 0; i < mixed.Length; i++)
        {
            short sample = (short)(Math.Clamp(mixed[i], -1.0f, 1.0f) * 32767);
            BitConverter.GetBytes(sample).CopyTo(output, i * 2);
        }
        
        return output;
    }
}
