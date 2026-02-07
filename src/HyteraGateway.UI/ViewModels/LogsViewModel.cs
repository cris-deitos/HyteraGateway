using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HyteraGateway.UI.Services;

namespace HyteraGateway.UI.ViewModels;

public partial class LogsViewModel : ObservableObject
{
    private readonly SignalRService _signalR;
    
    [ObservableProperty]
    private ObservableCollection<LogEntry> _logs = new();
    
    [ObservableProperty]
    private string _filterText = string.Empty;
    
    [ObservableProperty]
    private bool _showInfo = true;
    
    [ObservableProperty]
    private bool _showWarning = true;
    
    [ObservableProperty]
    private bool _showError = true;
    
    [ObservableProperty]
    private bool _autoScroll = true;
    
    [ObservableProperty]
    private int _maxLogEntries = 1000;
    
    public ObservableCollection<LogEntry> FilteredLogs => new(
        Logs.Where(l => 
            (string.IsNullOrEmpty(FilterText) || l.Message.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) &&
            ((ShowInfo && l.Level == LogLevel.Info) ||
             (ShowWarning && l.Level == LogLevel.Warning) ||
             (ShowError && l.Level == LogLevel.Error)))
    );

    public LogsViewModel(SignalRService signalR)
    {
        _signalR = signalR;
        _signalR.LogReceived += OnLogReceived;
    }

    private void OnLogReceived(object? sender, string logMessage)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var entry = ParseLogEntry(logMessage);
            Logs.Add(entry);
            
            // Trim old entries if exceeding max
            while (Logs.Count > MaxLogEntries)
            {
                Logs.RemoveAt(0);
            }
            
            OnPropertyChanged(nameof(FilteredLogs));
        });
    }
    
    private LogEntry ParseLogEntry(string message)
    {
        // Parse log level from message format: "[LEVEL] message"
        var level = LogLevel.Info;
        if (message.Contains("[ERROR]") || message.Contains("[ERR]"))
            level = LogLevel.Error;
        else if (message.Contains("[WARNING]") || message.Contains("[WARN]"))
            level = LogLevel.Warning;
            
        return new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Message = message
        };
    }

    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
        OnPropertyChanged(nameof(FilteredLogs));
    }
    
    [RelayCommand]
    private async Task ExportLogsAsync()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt",
            DefaultExt = ".log",
            FileName = $"HyteraGateway_{DateTime.Now:yyyyMMdd_HHmmss}.log"
        };
        
        if (dialog.ShowDialog() == true)
        {
            var lines = Logs.Select(l => $"[{l.Timestamp:yyyy-MM-dd HH:mm:ss}] [{l.Level}] {l.Message}");
            await System.IO.File.WriteAllLinesAsync(dialog.FileName, lines);
        }
    }
    
    partial void OnFilterTextChanged(string value) => OnPropertyChanged(nameof(FilteredLogs));
    partial void OnShowInfoChanged(bool value) => OnPropertyChanged(nameof(FilteredLogs));
    partial void OnShowWarningChanged(bool value) => OnPropertyChanged(nameof(FilteredLogs));
    partial void OnShowErrorChanged(bool value) => OnPropertyChanged(nameof(FilteredLogs));
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
}

public enum LogLevel
{
    Info,
    Warning,
    Error
}
