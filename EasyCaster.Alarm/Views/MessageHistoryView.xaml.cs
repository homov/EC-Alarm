using EasyCaster.Alarm.ViewModels;
using System.Windows.Controls;

namespace EasyCaster.Alarm.Views
{
    public partial class MessageHistoryView : UserControl
    {
        public MessageHistoryView()
        {
            DataContext = new MessageHistoryViewModel();
            InitializeComponent();
        }
    }
}
