using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyCaster.Alarm.Core.Models;
using EasyCaster.Alarm.Core.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace EasyCaster.Alarm.ViewModels;

public partial class MessageHistoryViewModel : ObservableObject
{
    const int MaxRecords = 20;
    const int MaxDelta = 5;
    private object lockObject = new();

    public ObservableCollection<EasyCasterMessage> Messages { get; } = new();

    MessageHandler messageHandler;

    public MessageHistoryViewModel()
    {
        messageHandler = App.Resolve<MessageHandler>();
        messageHandler.HandleEvent += MessageHandler_HandleEvent;

        //for (int i = 0; i <= 10; i++)
        //    AddMessage(new EasyCasterMessage() { Group = "Test Group 1", MessageText = $"Message {i}" });
    }

    private void AddMessage(EasyCasterMessage message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            lock (lockObject)
            {
                var recordsToKeep = MaxRecords + MaxDelta;

                if (Messages.Count >= recordsToKeep)
                {
                    while (Messages.Count > MaxRecords)
                        Messages.RemoveAt(0);
                }
                Messages.Add(message);
            }
        });
    }

    private Task MessageHandler_HandleEvent(EasyCasterEvent easyCasterEvent, EasyCasterMessage easyCasterMessage)
    {
        AddMessage(easyCasterMessage);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void Clear()
    {
        lock (lockObject)
        {
            Messages.Clear();
        }
    }
}
