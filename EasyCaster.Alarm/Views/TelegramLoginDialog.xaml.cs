using EasyCaster.Alarm.Helpers;
using System.Windows;

namespace EasyCaster.Alarm.Views
{
    public partial class TelegramLoginDialog : Window
    {
        public string Prompt { get; set; }
        public string Value { get; set; }
        
        public TelegramLoginDialog(string telegramValue)
        {
            Prompt = GetPrompt(telegramValue);
            this.DataContext = this;
            InitializeComponent();
        }

        private string GetPrompt(string telegramValue)
        {
            switch(telegramValue)
            {
                case "email_verification_code":
                    return LocalizationResourceManager.Current.GetValue("EnterEmailVerificationCode");
                case "phone_number":
                    return LocalizationResourceManager.Current.GetValue("EnterPhoneNumber");
                case "password":
                    return LocalizationResourceManager.Current.GetValue("EnterPassword"); 
                case "verification_code":
                    return LocalizationResourceManager.Current.GetValue("EnterVerificationCode");
            }
            return "?";
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Value)) 
            {
                MessageBox.Show(LocalizationResourceManager.Current.GetValue("EnterRequiredValue"),
                    LocalizationResourceManager.Current.GetValue("InvalidValue"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Value = Core.Constants.TelegramCancelValue;
            this.DialogResult = false;
        }
    }
}
