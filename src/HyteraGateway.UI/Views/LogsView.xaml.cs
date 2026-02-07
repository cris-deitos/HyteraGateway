using System;
using System.Windows.Controls;
using HyteraGateway.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HyteraGateway.UI.Views;

public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
        
        // Get ViewModel from DI
        if (System.Windows.Application.Current is App app)
        {
            DataContext = app.ServiceProvider.GetService<LogsViewModel>();
        }
    }
}
