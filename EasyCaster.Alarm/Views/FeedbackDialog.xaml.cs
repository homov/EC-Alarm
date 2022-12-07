using System.Diagnostics;
using System.Windows;

namespace EasyCaster.Alarm.Views;

public partial class FeedbackDialog : Window
{
    public FeedbackDialog()
    {
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


