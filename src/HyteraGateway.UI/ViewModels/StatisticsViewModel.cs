using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HyteraGateway.UI.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace HyteraGateway.UI.ViewModels;

public partial class StatisticsViewModel : ObservableObject
{
    private readonly ApiService _api;
    
    [ObservableProperty]
    private ISeries[] _callsPerDaySeries = Array.Empty<ISeries>();
    
    [ObservableProperty]
    private ISeries[] _callTypePieSeries = Array.Empty<ISeries>();
    
    [ObservableProperty]
    private int _totalCalls;
    
    [ObservableProperty]
    private string _totalDuration = "00:00:00";
    
    [ObservableProperty]
    private int _activeRadios;
    
    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(-7);
    
    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;
    
    [ObservableProperty]
    private bool _isLoading;

    public StatisticsViewModel(ApiService api)
    {
        _api = api;
        
        // Initialize with sample data
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        // Sample calls per day data
        var callsData = new List<int> { 45, 52, 38, 67, 48, 55, 42 };
        var dates = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-6 + i).ToString("MM/dd")).ToList();

        CallsPerDaySeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "Calls",
                Values = callsData,
                Fill = new SolidColorPaint(SKColor.Parse("#2196F3")),
                Stroke = null,
                MaxBarWidth = 50
            }
        };

        // Sample call type pie data
        CallTypePieSeries = new ISeries[]
        {
            new PieSeries<int>
            {
                Name = "Private Calls",
                Values = new[] { 156 },
                Fill = new SolidColorPaint(SKColor.Parse("#4CAF50"))
            },
            new PieSeries<int>
            {
                Name = "Group Calls",
                Values = new[] { 187 },
                Fill = new SolidColorPaint(SKColor.Parse("#2196F3"))
            },
            new PieSeries<int>
            {
                Name = "Emergency",
                Values = new[] { 4 },
                Fill = new SolidColorPaint(SKColor.Parse("#F44336"))
            }
        };

        TotalCalls = 347;
        TotalDuration = "05:47:23";
        ActiveRadios = 12;
    }

    [RelayCommand]
    private async Task RefreshStatistics()
    {
        IsLoading = true;
        try
        {
            // In a real implementation, this would fetch from the API
            // await _api.GetAsync<StatisticsData>($"/api/statistics?start={StartDate:yyyy-MM-dd}&end={EndDate:yyyy-MM-dd}");
            
            await Task.Delay(500); // Simulate API call
            InitializeSampleData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to refresh statistics: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ExportToExcel()
    {
        // In a real implementation, this would export data to Excel
        System.Diagnostics.Debug.WriteLine("Exporting statistics to Excel...");
    }

    partial void OnStartDateChanged(DateTime value)
    {
        if (value > EndDate)
        {
            EndDate = value;
        }
    }

    partial void OnEndDateChanged(DateTime value)
    {
        if (value < StartDate)
        {
            StartDate = value;
        }
    }
}
