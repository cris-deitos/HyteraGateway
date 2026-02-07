using System.Windows.Controls;
using HyteraGateway.UI.ViewModels;

namespace HyteraGateway.UI.Views;

public partial class LogsView : UserControl
{
    public LogsView()
    {
        InitializeComponent();
        
        // Get ViewModel from DI
        var app = (App)System.Windows.Application.Current;
        DataContext = app.ServiceProvider.GetService(typeof(LogsViewModel));
    }
}
