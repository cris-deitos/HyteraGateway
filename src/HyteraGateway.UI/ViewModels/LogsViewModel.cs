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

/// <summary>
/// ViewModel for the Logs view, providing real-time log display and filtering
/// </summary>
public partial class LogsViewModel : ObservableObject
{
    private readonly SignalRService _signalR;
    
    /// <summary>
    /// All log entries received from the system
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<LogEntry> _logs = new();
    
    /// <summary>
    /// Filtered log entries based on current filter criteria
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<LogEntry> _filteredLogs = new();
    
    /// <summary>
    /// Text filter for searching log messages and sources
    /// </summary>
    [ObservableProperty]
    private string _filterText = "";
    
    /// <summary>
    /// Indicates whether auto-scroll to new logs is enabled
    /// </summary>
    [ObservableProperty]
    private bool _autoScroll = true;
    
    /// <summary>
    /// Currently selected log level filter
    /// </summary>
    [ObservableProperty]
    private string _selectedLogLevel = "All";
    
    /// <summary>
    /// Available log level filter options
    /// </summary>
    public string[] LogLevels { get; } = { "All", "Debug", "Info", "Warning", "Error" };

    /// <summary>
    /// Initializes a new instance of the <see cref="LogsViewModel"/> class
    /// </summary>
    /// <param name="signalR">The SignalR service for receiving real-time log events</param>
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

    /// <summary>
    /// Clears all logs from the display
    /// </summary>
    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
        FilteredLogs.Clear();
    }

    /// <summary>
    /// Exports logs to a file (TXT or CSV format)
    /// </summary>
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
                        sb.AppendLine(FormatCsvLine(log));
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

    private static string FormatCsvLine(LogEntry log)
    {
        // Escape CSV fields properly
        var timestamp = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        var level = EscapeCsvField(log.Level);
        var source = EscapeCsvField(log.Source);
        var message = EscapeCsvField(log.Message);
        
        return $"{timestamp},{level},{source},{message}";
    }

    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";
        
        // If field contains comma, newline, or quote, wrap in quotes and escape quotes
        if (field.Contains(',') || field.Contains('\n') || field.Contains('"'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        
        return field;
    }

    /// <summary>
    /// Toggles the auto-scroll feature
    /// </summary>
    [RelayCommand]
    private void ToggleAutoScroll()
    {
        AutoScroll = !AutoScroll;
    }
}
