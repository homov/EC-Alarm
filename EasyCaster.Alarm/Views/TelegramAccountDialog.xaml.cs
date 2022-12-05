using EasyCaster.Alarm.ViewModels;
using System.Windows;

namespace EasyCaster.Alarm.Views;

public partial class TelegramAccountDialog : Window
{
    public TelegramAccountViewModel ViewModel => DataContext as TelegramAccountViewModel;

    public TelegramAccountDialog()
    {
        DataContext = new TelegramAccountViewModel()
        {
            CloseRequest = this.CloseRequest
        };
        InitializeComponent();
        Loaded += TelegramAccountDialog_Loaded;
    }

    private void TelegramAccountDialog_Loaded(object sender, RoutedEventArgs e)
    {
        PhoneTextBox.Focus();
    }

    private void CloseRequest()
    {
        this.DialogResult = ViewModel.IsOk;
        this.Close();
    }
}
