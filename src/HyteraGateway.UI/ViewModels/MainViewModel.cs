using CommunityToolkit.Mvvm.ComponentModel;

namespace HyteraGateway.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _serviceStatus = "Offline";

    [ObservableProperty]
    private string _serviceStatusColor = "Red";
}
