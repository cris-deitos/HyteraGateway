using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HyteraGateway.UI.Services;
using HyteraGateway.UI.Models;

namespace HyteraGateway.UI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly SignalRService _signalR;
    private readonly ApiService _api;

    [ObservableProperty]
    private string _serviceStatus = "Offline";

    [ObservableProperty]
    private int _activeRadios;

    [ObservableProperty]
    private int _callsToday;

    [ObservableProperty]
    private string _recordingsSize = "0 MB";

    [ObservableProperty]
    private string _databaseStatus = "Disconnected";

    [ObservableProperty]
    private string _ftpStatus = "Disconnected";

    public ObservableCollection<RadioViewModel> Radios { get; } = new();

    public DashboardViewModel(SignalRService signalR, ApiService api)
    {
        _signalR = signalR;
        _api = api;
        
        // Initialize asynchronously with proper error handling
        InitializeAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                System.Diagnostics.Debug.WriteLine($"DashboardViewModel initialization failed: {task.Exception?.GetBaseException().Message}");
            }
        }, TaskScheduler.Default);
    }

    private async Task InitializeAsync()
    {
        await _signalR.ConnectAsync();
        _signalR.RadioEventReceived += OnRadioEvent;
        await RefreshDataAsync();
    }

    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        // TODO: Call API to get current status
        await Task.CompletedTask;
    }

    private void OnRadioEvent(object? sender, Core.Models.RadioEvent evt)
    {
        // Update UI based on radio events
    }
}
