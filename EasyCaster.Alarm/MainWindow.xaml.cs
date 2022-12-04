using EasyCaster.Alarm.Core.Enums;
using EasyCaster.Alarm.Core.Interfaces;
using EasyCaster.Alarm.Services;
using EasyCaster.Alarm.Views;
using System.Threading.Tasks;
using System.Windows;

namespace EasyCaster.Alarm;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void Initialize()
    {
        LoggerService.Instance.LogMessageRecieved += (message) =>
        {
            LogView.Log(message.Source, message.LogLevel, message.Message, message.Exception);
        };
        var messageReader = App.Resolve<IMessageReader>();
        messageReader.ConnectionStateChanged += MessageReader_ConnectionStateChanged;
    }

    public void BringToForeground()
    {
        if (this.WindowState == WindowState.Minimized || this.Visibility == Visibility.Hidden)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }
        this.Activate();
        this.Topmost = true;
        this.Topmost = false;
        this.Focus();
    }


    private Task MessageReader_ConnectionStateChanged(ConnectionState connectionState)
    {
        if(connectionState == ConnectionState.Connecting)
        {
            this.Dispatcher.Invoke(() =>
            {
                BringToForeground();
            });
        }
        return Task.CompletedTask;
    }

    public string GetTelegramValue(string valueName)
    {
        var valueDialog = new TelegramLoginDialog(valueName)
        {
            Owner = this
        };
        if (valueDialog.ShowDialog() == true)
        {
            return valueDialog.Value;
        }
        return Core.Constants.TelegramCancelValue;
    }
}
