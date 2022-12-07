using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace EasyCaster.Alarm.Views;

public partial class AboutDialog : Window
{

    public string Version { get; }
    
    public AboutDialog()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        Version = version.ToString();
        DataContext = this;
        InitializeComponent();
    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        var processInfo = new ProcessStartInfo(e.Uri.ToString())
        {
            UseShellExecute = true
        };
        System.Diagnostics.Process.Start(processInfo);
        e.Handled = true;
    }
}
