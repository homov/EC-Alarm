using EasyCaster.Alarm.ViewModels;
using System.Windows.Controls;

namespace EasyCaster.Alarm.Views
{
    public partial class TelegramConnectionStateView : UserControl
    {
        public TelegramConnectionStateView()
        {
            DataContext = new TelegramConnectionStateViewModel();
            InitializeComponent();
        }
    }
}
