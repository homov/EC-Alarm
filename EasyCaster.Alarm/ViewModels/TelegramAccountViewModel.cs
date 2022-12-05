using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyCaster.Alarm.Helpers;
using System;
using System.Windows;

namespace EasyCaster.Alarm.ViewModels;

public partial class TelegramAccountViewModel: ObservableObject
{
    [ObservableProperty]
    string phone;

    [ObservableProperty]
    string password;

    [ObservableProperty]
    bool isOk = false;

    public Action CloseRequest { get; set; }

    [RelayCommand]
    void OK()
    {
        if (String.IsNullOrWhiteSpace(Phone))
        {
            MessageBox.Show(Application.Current.MainWindow,
                LocalizationResourceManager.Current.GetValue("PleaseEnterPhone"),
                LocalizationResourceManager.Current.GetValue("Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        isOk = true;
        CloseRequest?.Invoke();
    }

    [RelayCommand]
    void Cancel()
    {
        isOk = false;
        CloseRequest?.Invoke();
    }
}
