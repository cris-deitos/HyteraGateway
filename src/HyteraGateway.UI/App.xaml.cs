using System;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using HyteraGateway.UI.Services;
using HyteraGateway.UI.ViewModels;

namespace HyteraGateway.UI;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<SignalRService>();
        services.AddSingleton<ApiService>();
        services.AddSingleton<ConfigurationService>();
        services.AddSingleton<NetworkDiscoveryService>();
        services.AddSingleton<AudioService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<StatisticsViewModel>();
        services.AddSingleton<LogsViewModel>();
        services.AddSingleton<ToolsViewModel>();
        services.AddSingleton<AudioControlViewModel>();

        // Windows
        services.AddSingleton<MainWindow>();
    }
}
