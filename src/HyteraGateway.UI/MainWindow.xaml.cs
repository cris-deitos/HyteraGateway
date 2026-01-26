using System;
using System.Windows;
using System.Windows.Controls;
using HyteraGateway.UI.ViewModels;
using HyteraGateway.UI.Views;
using Hardcodet.Wpf.TaskbarNotification;

namespace HyteraGateway.UI;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private TaskbarIcon? _taskbarIcon;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        InitializeTaskbarIcon();
        NavigationListBox.SelectedIndex = 0;
    }

    private void InitializeTaskbarIcon()
    {
        try
        {
            _taskbarIcon = new TaskbarIcon
            {
                Icon = new System.Drawing.Icon("Assets/icon.ico"),
                ToolTipText = "HyteraGateway Control Panel"
            };
            
            _taskbarIcon.TrayMouseDoubleClick += (s, e) => 
            {
                Show();
                WindowState = WindowState.Normal;
            };
        }
        catch
        {
            // Icon file not found, system tray icon will not be available
        }
    }

    private void NavigationListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var index = NavigationListBox.SelectedIndex;
        ContentArea.Content = index switch
        {
            0 => new DashboardView(),
            1 => new StatisticsView(),
            2 => new LogsView(),
            3 => new SettingsView(),
            4 => new ToolsView(),
            _ => null
        };
    }

    private void MinimizeToTray_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        _taskbarIcon?.Dispose();
        base.OnClosing(e);
    }
}
