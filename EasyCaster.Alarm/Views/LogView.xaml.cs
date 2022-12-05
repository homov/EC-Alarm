using EasyCaster.Alarm.ViewModels;
using System.Linq;
using System.Windows.Controls;

namespace EasyCaster.Alarm.Views;


public partial class LogView : UserControl
{
    LogViewModel ViewModel => DataContext as LogViewModel;

    public LogView()
    {
        DataContext = new LogViewModel();
        
        InitializeComponent();

        ViewModel.LogRecords.CollectionChanged += LogRecords_CollectionChanged;
    }

    private void LogRecords_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        var last = ViewModel.LogRecords.LastOrDefault();
        if (last != null) return;
        LogListBox.ScrollIntoView(last);
    }
}
