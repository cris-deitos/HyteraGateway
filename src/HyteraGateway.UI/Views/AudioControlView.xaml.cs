using System.Windows.Controls;
using System.Windows.Input;
using HyteraGateway.UI.ViewModels;

namespace HyteraGateway.UI.Views;

public partial class AudioControlView : UserControl
{
    public AudioControlView()
    {
        InitializeComponent();
        
        var app = (App)System.Windows.Application.Current;
        DataContext = app.ServiceProvider.GetService(typeof(AudioControlViewModel));
    }

    private void PttButton_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is AudioControlViewModel vm)
        {
            vm.StartPttCommand.Execute(null);
        }
    }

    private void PttButton_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is AudioControlViewModel vm)
        {
            vm.StopPttCommand.Execute(null);
        }
    }
}
