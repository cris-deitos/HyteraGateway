using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HyteraGateway.UI.Models;
using HyteraGateway.UI.Services;
using Microsoft.Win32;

namespace HyteraGateway.UI.ViewModels;

public partial class LogsViewModel : ObservableObject
{
    private readonly SignalRService _signalR;
    
    [ObservableProperty]
    private ObservableCollection<LogEntry> _logs = new();
    
    [ObservableProperty]
    private ObservableCollection<LogEntry> _filteredLogs = new();
    
    [ObservableProperty]
    private string _filterText = "";
    
    [ObservableProperty]
    private bool _autoScroll = true;
    
    [ObservableProperty]
    private string _selectedLogLevel = "All";
    
    public string[] LogLevels { get; } = { "All", "Debug", "Info", "Warning", "Error" };

    public LogsViewModel(SignalRService signalR)
    {
        _signalR = signalR;
        _signalR.LogReceived += OnLogReceived;
        
        // Add some sample logs for testing
        AddSampleLogs();
    }
    
    private void AddSampleLogs()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Logs.Add(new LogEntry
            {
                Timestamp = DateTime.Now.AddMinutes(-5),
                Level = "Info",
                Source = "System",
                Message = "Application started"
            });
            Logs.Add(new LogEntry
            {
                Timestamp = DateTime.Now.AddMinutes(-4),
                Level = "Debug",
                Source = "Radio",
                Message = "Attempting connection to 192.168.1.1:50000"
            });
            Logs.Add(new LogEntry
            {
                Timestamp = DateTime.Now.AddMinutes(-3),
                Level = "Warning",
                Source = "Network",
                Message = "Connection timeout, retrying..."
            });
            FilterLogs();
        });
    }

    private void OnLogReceived(object? sender, LogEntry log)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Logs.Add(log);
            FilterLogs();
        });
    }

    partial void OnFilterTextChanged(string value)
    {
        FilterLogs();
    }

    partial void OnSelectedLogLevelChanged(string value)
    {
        FilterLogs();
    }

    private void FilterLogs()
    {
        var filtered = Logs.AsEnumerable();

        // Filter by log level
        if (SelectedLogLevel != "All")
        {
            filtered = filtered.Where(l => l.Level.Equals(SelectedLogLevel, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by text
        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            filtered = filtered.Where(l =>
                l.Message.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                l.Source.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
        }

        FilteredLogs.Clear();
        foreach (var log in filtered.OrderByDescending(l => l.Timestamp))
        {
            FilteredLogs.Add(log);
        }
    }

    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
        FilteredLogs.Clear();
    }

    [RelayCommand]
    private void ExportLogs()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".txt",
                FileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (dialog.ShowDialog() == true)
            {
                var sb = new StringBuilder();
                
                if (dialog.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine("Timestamp,Level,Source,Message");
                    foreach (var log in FilteredLogs)
                    {
                        sb.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.Level},{log.Source},\"{log.Message.Replace("\"", "\"\"")}\"");
                    }
                }
                else
                {
                    foreach (var log in FilteredLogs)
                    {
                        sb.AppendLine($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss}] [{log.Level}] [{log.Source}] {log.Message}");
                    }
                }

                File.WriteAllText(dialog.FileName, sb.ToString());
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to export logs: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ToggleAutoScroll()
    {
        AutoScroll = !AutoScroll;
    }
}
