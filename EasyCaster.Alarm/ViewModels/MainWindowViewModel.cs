using CommunityToolkit.Mvvm.ComponentModel;
using EasyCaster.Alarm.Core.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;


namespace EasyCaster.Alarm.ViewModels;

public partial class MainWindowViewModel: ObservableObject
{
    [ObservableProperty]
    bool isMessageRecived = false;

    DispatcherTimer dispatcherTimer;

	public MainWindowViewModel()
	{
        var messageHandler = App.Resolve<MessageHandler>();
        messageHandler.HandleEvent += MessageHandler_HandleEvent;
        dispatcherTimer = new DispatcherTimer();
        dispatcherTimer.Interval = TimeSpan.FromSeconds(10);
        dispatcherTimer.Tick += (_, _) =>
        {
            Application.Current.Dispatcher.Invoke(() => IsMessageRecived = false);
            dispatcherTimer.Stop();
        };
    }

    private Task MessageHandler_HandleEvent(Core.Models.EasyCasterEvent easyCasterEvent, 
        Core.Models.EasyCasterMessage easyCasterMessage)
    {
        Application.Current.Dispatcher.Invoke(() => IsMessageRecived = true);
        dispatcherTimer.Stop();
        dispatcherTimer.Start();
        return Task.CompletedTask;
    }
}
