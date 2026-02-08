using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using NAudio.Wave;
using Concentus.Structs;
using Concentus.Enums;

namespace HyteraGateway.UI.Services;

public class AudioService : IAudioService
{
    private HubConnection? _hubConnection;
    private WaveOutEvent? _waveOut;
    private BufferedWaveProvider? _bufferedWaveProvider;
    private WaveInEvent? _waveIn;
    private bool _isTransmitting;
    private bool _isReceiving;
    private readonly ConfigurationService _configurationService;
    private OpusDecoder? _opusDecoder;
    
    public event EventHandler<AudioStateChangedEventArgs>? StateChanged;
    public event EventHandler<float>? AudioLevelChanged;
    
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    public bool IsTransmitting => _isTransmitting;
    public bool IsReceiving => _isReceiving;
    public float Volume { get; set; } = 1.0f;
    public bool IsMuted { get; set; }

    public AudioService(ConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public async Task ConnectAsync()
    {
        var audioHubUrl = _configurationService.Configuration.ApiBaseUrl + "/hubs/audio";
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(audioHubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<byte[], int, int, long>("ReceiveAudio", OnAudioReceived);
        
        // Initialize audio output (48kHz Opus decoded to PCM)
        _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 1))
        {
            BufferDuration = TimeSpan.FromSeconds(5),
            DiscardOnBufferOverflow = true
        };
        
        // Initialize Opus decoder (48kHz, mono)
        _opusDecoder = new OpusDecoder(48000, 1);
        
        _waveOut = new WaveOutEvent();
        _waveOut.Init(_bufferedWaveProvider);

        try
        {
            await _hubConnection.StartAsync();
            StateChanged?.Invoke(this, new AudioStateChangedEventArgs("Connected"));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Audio hub connection failed: {ex.Message}");
        }
    }

    public async Task SubscribeToRadioAsync(int radioId)
    {
        if (_hubConnection != null)
        {
            await _hubConnection.InvokeAsync("SubscribeToAudio", radioId, null);
        }
    }

    public async Task SubscribeToTalkGroupAsync(int talkGroupId)
    {
        if (_hubConnection != null)
        {
            await _hubConnection.InvokeAsync("SubscribeToAudio", null, talkGroupId);
        }
    }

    public void StartPlayback()
    {
        if (_waveOut?.PlaybackState != PlaybackState.Playing)
        {
            _waveOut?.Play();
            _isReceiving = true;
            StateChanged?.Invoke(this, new AudioStateChangedEventArgs("Receiving"));
        }
    }

    public void StopPlayback()
    {
        _waveOut?.Stop();
        _isReceiving = false;
        StateChanged?.Invoke(this, new AudioStateChangedEventArgs("Idle"));
    }

    public Task StartTransmitAsync(int targetRadioId)
    {
        if (_isTransmitting) return Task.CompletedTask;
        
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(48000, 16, 1),
            BufferMilliseconds = 20
        };
        
        _waveIn.DataAvailable += async (s, e) =>
        {
            if (_hubConnection != null && _isTransmitting)
            {
                // Send raw PCM to server (server will encode to Opus then AMBE)
                await _hubConnection.InvokeAsync("SendAudioToRadio", e.Buffer, targetRadioId);
                
                // Calculate audio level for UI meter
                float level = CalculateAudioLevel(e.Buffer, e.BytesRecorded);
                AudioLevelChanged?.Invoke(this, level);
            }
        };
        
        _waveIn.StartRecording();
        _isTransmitting = true;
        StateChanged?.Invoke(this, new AudioStateChangedEventArgs("Transmitting"));
        return Task.CompletedTask;
    }

    public void StopTransmit()
    {
        _waveIn?.StopRecording();
        _waveIn?.Dispose();
        _waveIn = null;
        _isTransmitting = false;
        StateChanged?.Invoke(this, new AudioStateChangedEventArgs("Idle"));
    }

    private void OnAudioReceived(byte[] opusData, int radioId, int talkGroupId, long timestamp)
    {
        if (IsMuted || _bufferedWaveProvider == null || _opusDecoder == null) return;
        
        try
        {
            // Decode Opus to PCM
            // Opus frame at 48kHz can contain up to 5760 samples (120ms)
            short[] pcmBuffer = new short[5760];
            int samplesDecoded = _opusDecoder.Decode(opusData, 0, opusData.Length, pcmBuffer, 0, pcmBuffer.Length, false);
            
            if (samplesDecoded > 0)
            {
                // Convert short[] to byte[] for NAudio using Buffer.BlockCopy for efficiency
                byte[] pcmBytes = new byte[samplesDecoded * 2];
                Buffer.BlockCopy(pcmBuffer, 0, pcmBytes, 0, samplesDecoded * 2);
                
                // Apply volume
                byte[] adjusted = ApplyVolume(pcmBytes, Volume);
                _bufferedWaveProvider.AddSamples(adjusted, 0, adjusted.Length);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Opus decode error: {ex.Message}");
            // On decode error, skip this packet
        }
        
        if (!_isReceiving)
        {
            StartPlayback();
        }
    }

    private byte[] ApplyVolume(byte[] pcmData, float volume)
    {
        if (Math.Abs(volume - 1.0f) < 0.01f) return pcmData;
        
        byte[] result = new byte[pcmData.Length];
        for (int i = 0; i < pcmData.Length; i += 2)
        {
            short sample = (short)(pcmData[i] | (pcmData[i + 1] << 8));
            int amplified = (int)(sample * volume);
            sample = (short)Math.Clamp(amplified, short.MinValue, short.MaxValue);
            result[i] = (byte)(sample & 0xFF);
            result[i + 1] = (byte)((sample >> 8) & 0xFF);
        }
        return result;
    }

    private float CalculateAudioLevel(byte[] buffer, int bytesRecorded)
    {
        float max = 0;
        for (int i = 0; i < bytesRecorded; i += 2)
        {
            short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
            float abs = Math.Abs(sample / 32768f);
            if (abs > max) max = abs;
        }
        return max;
    }

    public async Task DisconnectAsync()
    {
        StopTransmit();
        StopPlayback();
        
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }

    public void Dispose()
    {
        _waveIn?.Dispose();
        _waveOut?.Dispose();
        _opusDecoder = null; // OpusDecoder doesn't implement IDisposable
        _hubConnection?.DisposeAsync().AsTask().Wait();
    }
}

public class AudioStateChangedEventArgs : EventArgs
{
    public string State { get; }
    public AudioStateChangedEventArgs(string state) => State = state;
}
