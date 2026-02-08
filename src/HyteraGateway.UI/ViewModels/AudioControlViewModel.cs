using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HyteraGateway.UI.Services;

namespace HyteraGateway.UI.ViewModels;

public partial class AudioControlViewModel : ObservableObject
{
    private readonly IAudioService _audioService;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private bool _isTransmitting;

    [ObservableProperty]
    private bool _isReceiving;

    [ObservableProperty]
    private float _volume = 0.8f;

    [ObservableProperty]
    private bool _isMuted;

    [ObservableProperty]
    private float _audioLevel;

    [ObservableProperty]
    private string _audioState = "Disconnected";

    [ObservableProperty]
    private int _selectedRadioId;

    [ObservableProperty]
    private int _selectedTalkGroupId = 9; // Default TG

    public AudioControlViewModel(IAudioService audioService)
    {
        _audioService = audioService;
        _audioService.StateChanged += OnStateChanged;
        _audioService.AudioLevelChanged += OnAudioLevelChanged;
    }

    private void OnStateChanged(object? sender, AudioStateChangedEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            AudioState = e.State;
            IsConnected = _audioService.IsConnected;
            IsTransmitting = _audioService.IsTransmitting;
            IsReceiving = _audioService.IsReceiving;
        });
    }

    private void OnAudioLevelChanged(object? sender, float level)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            AudioLevel = level;
        });
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        await _audioService.ConnectAsync();
        if (SelectedTalkGroupId > 0)
        {
            await _audioService.SubscribeToTalkGroupAsync(SelectedTalkGroupId);
        }
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        await _audioService.DisconnectAsync();
    }

    [RelayCommand]
    private async Task StartPttAsync()
    {
        if (SelectedRadioId > 0)
        {
            await _audioService.StartTransmitAsync(SelectedRadioId);
        }
    }

    [RelayCommand]
    private void StopPtt()
    {
        _audioService.StopTransmit();
    }

    [RelayCommand]
    private void ToggleMute()
    {
        IsMuted = !IsMuted;
        _audioService.IsMuted = IsMuted;
    }

    partial void OnVolumeChanged(float value)
    {
        _audioService.Volume = value;
    }
}
