using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyCaster.Alarm.Core.Enums;
using EasyCaster.Alarm.Core.Interfaces;
using EasyCaster.Alarm.Helpers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EasyCaster.Alarm.ViewModels;

public partial class TelegramConnectionStateViewModel: ObservableObject
{
    [ObservableProperty]
    string connectionStateText;

    [ObservableProperty]
    ConnectionState connectionState;

    [ObservableProperty]
    bool canConnectOrDisconnect;

    [ObservableProperty]
    string actionText;

    [ObservableProperty]
    Brush currentBackground;

    private IMessageReader messageReader;

    public TelegramConnectionStateViewModel()
    {
        currentBackground = Brushes.WhiteSmoke;

        canConnectOrDisconnect = false;
        actionText = "CONNECT";
        messageReader = App.Resolve<IMessageReader>();
        messageReader.ConnectionStateChanged += UpdateConnectionState;
        UpdateConnectionState(messageReader.ConnectionState);
    }

    private Task UpdateConnectionState(ConnectionState newState)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            this.ConnectionState = newState;
            this.ConnectionStateText = GetConnectionStateText(newState);
            if (newState == ConnectionState.Connected)
            {
                this.CanConnectOrDisconnect = true;
                this.ActionText = LocalizationResourceManager.Current.GetValue("Stop");
                this.CurrentBackground = Brushes.LightGreen;
            }
            else if (newState == ConnectionState.Disconnected)
            {
                this.CanConnectOrDisconnect = true;
                this.ActionText = LocalizationResourceManager.Current.GetValue("Start");
                this.CurrentBackground = Brushes.WhiteSmoke;
            }
            else
            {
                this.CanConnectOrDisconnect = true;
                this.ActionText = LocalizationResourceManager.Current.GetValue("Cancel");
                this.CurrentBackground = Brushes.LightPink;
            }
        });
        return Task.CompletedTask;
    }

    private string GetConnectionStateText(ConnectionState newState)
    {
        switch( newState)
        {
            case ConnectionState.Connected:
                return LocalizationResourceManager.Current.GetValue("Connected");
            case ConnectionState.Connecting:
                return LocalizationResourceManager.Current.GetValue("Connecting");
            case ConnectionState.Disconnected:
                return LocalizationResourceManager.Current.GetValue("Disconnected");
            default:
                return "?";
        }
    }

    [RelayCommand]
    private void ConnectOrDisconnect()
    {
        if (messageReader.ConnectionState == ConnectionState.Connected 
            || messageReader.ConnectionState == ConnectionState.Connecting)
        {
            messageReader.Stop();
        }
        else if (messageReader.ConnectionState == ConnectionState.Disconnected)
        {
            messageReader.Start();
        }
    }
}
