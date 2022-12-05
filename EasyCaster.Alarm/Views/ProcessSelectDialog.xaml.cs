using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyCaster.Alarm.Views;

public partial class ProcessSelectDialog : Window
{
    public List<ProcessModel> ProcessList { get; } = new();

    public ProcessModel SelectedProcess { get; private set; }

    public ProcessSelectDialog()
    {
        LoadProcessList();
        DataContext = this;
        InitializeComponent();
    }

    private void LoadProcessList()
    {
        Process.GetProcesses().Where(it=>!String.IsNullOrWhiteSpace(it.MainWindowTitle)).ToList().ForEach(p =>
        {
            ProcessList.Add(new ProcessModel()
            {
                Name = p.ProcessName,
                Title = p.MainWindowTitle
            }); ;
        });
    }


    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedProcess = ProcessListBox.SelectedItem as ProcessModel;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }


    public class ProcessModel
    {
        public string Name { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
