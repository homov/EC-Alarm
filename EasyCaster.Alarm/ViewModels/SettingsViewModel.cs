using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyCaster.Alarm.Core.Helpers;
using EasyCaster.Alarm.Exceptions;
using EasyCaster.Alarm.Helpers;
using EasyCaster.Alarm.Models;
using EasyCaster.Alarm.Services;
using EasyCaster.Alarm.Views;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace EasyCaster.Alarm.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private bool isEditing = false;

    public bool IsNotEditing => !IsEditing;

    public bool IsEditing
    {
        get => isEditing;
        set
        {
            if (SetProperty(ref isEditing, value))
                OnPropertyChanged(nameof(IsNotEditing));
        }
    }

    [ObservableProperty]
    string helpText;

    [ObservableProperty]
    string phone;

    [ObservableProperty]
    string password;

    [ObservableProperty]
    bool autoStart;

    [ObservableProperty]
    bool autoConnect;

    [ObservableProperty]
    string primaryChannel;

    [ObservableProperty]
    string testChannel;

    [ObservableProperty]
    ObservableCollection<IndexedItem<string>> excludedText = new();

    [ObservableProperty]
    ObservableCollection<EventModel> events = new();

    [ObservableProperty]
    ObservableCollection<TaskModel> tasks = new();

    [ObservableProperty]
    string webHookUrl;

    [ObservableProperty]
    string saveMessagesPath;

    ConfigurationService configurationService;
    LoggerService loggerService;

    public SettingsViewModel()
    {
        configurationService = ConfigurationService.Instance;
        loggerService = LoggerService.Instance;
        UpdateHelpText();
        LoadConfiguration();
    }

    private void ValidateConfiguration()
    {
        if (String.IsNullOrWhiteSpace(Phone))
        {
            throw new Exception(LocalizationResourceManager.Current.GetValue("PleaseEnterPhone"));
        }

        bool hasWarnins = false;
        StringBuilder warnings = new();

        warnings.AppendLine(LocalizationResourceManager.Current.GetValue("WarningsFound"));
        if (String.IsNullOrWhiteSpace(PrimaryChannel) && String.IsNullOrWhiteSpace(TestChannel))
        {
            hasWarnins = true;
            warnings.AppendLine(LocalizationResourceManager.Current.GetValue("NoTelegramChannels"));
        }
        else if (String.IsNullOrWhiteSpace(PrimaryChannel))
        {
            hasWarnins = true;
            warnings.AppendLine(LocalizationResourceManager.Current.GetValue("NoPrimaryTelegramChannel"));
        }
        if (Events.Count == 0)
        {
            hasWarnins = true;
            warnings.AppendLine(LocalizationResourceManager.Current.GetValue("NoNessagesToProcess"));
            if (Tasks.Count > 0)
            {
                warnings.Append(LocalizationResourceManager.Current.GetValue("ScheduleWillNotWork"));
            }
        }
        if (hasWarnins)
        {
            warnings.AppendLine("");
            warnings.AppendLine(LocalizationResourceManager.Current.GetValue("SaveConfiguration"));
            var result = MessageBox.Show(
                Application.Current.MainWindow,
                warnings.ToString(),
                LocalizationResourceManager.Current.GetValue("Warning"),
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Cancel)
            {
                throw new UserCancelException();
            }
        }
    }

    private bool SaveConfiguration()
    {
        try
        {
            ValidateConfiguration();
        }
        catch (UserCancelException)
        {
            return false;
        }
        catch (Exception exception)
        {
            MessageBox.Show(Application.Current.MainWindow, 
                exception.Message, 
                LocalizationResourceManager.Current.GetValue("Error"), 
                MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var configuration = new Configuration();
        configuration.Phone = this.Phone;
        configuration.Password = this.Password;
        configuration.AutoStart = this.AutoStart;
        configuration.AutoConnect = this.AutoConnect;
        configuration.PrimaryChannel = this.PrimaryChannel;
        configuration.TestChannel = this.TestChannel;
        configuration.WebHookUrl = this.WebHookUrl;
        configuration.SaveMessagesPath = this.SaveMessagesPath;

        foreach (var text in this.ExcludedText)
        {
            configuration.ExcludeText.Add(text.Value);
        }
        foreach (var item in this.Events)
        {
            configuration.Events.Add(item.ToEasyCasterEvent());
        }
        foreach (var task in this.Tasks)
        {
            configuration.PeriodicTasks.Add(task.ToEasyCasterTask());
        }
        configurationService.UpdateConfiguration(configuration);
        return true;
    }

    private void LoadConfiguration()
    {
        var configuration = configurationService.Configuration;
        this.Phone = configuration.Phone;
        this.Password = configuration.Password;
        this.AutoStart = configuration.AutoStart;
        this.AutoConnect = configuration.AutoConnect;
        this.PrimaryChannel = configuration.PrimaryChannel;
        this.TestChannel = configuration.TestChannel;
        this.WebHookUrl = configuration.WebHookUrl;
        this.SaveMessagesPath = configuration.SaveMessagesPath;
        this.ExcludedText.Clear();
        this.Events.Clear();
        this.Tasks.Clear();

        foreach (var text in configuration.ExcludeText)
        {
            this.ExcludedText.Add(new IndexedItem<string>(this.ExcludedText.Count + 1, text));
        }
        foreach (var @event in configuration.Events)
        {
            this.Events.Add(EventModel.FromEasyCasterEvent(@event));
        }
        foreach (var task in configuration.PeriodicTasks)
        {
            var taskModel = new TaskModel();
            taskModel.DelayPeriod = task.DelayPeriod;
            taskModel.StartEvent = this.Events.Where(it => it.Id == task.StartEventId).FirstOrDefault();
            taskModel.StopEvent = this.Events.Where(it => it.Id == task.StopEventId).FirstOrDefault();
            taskModel.Action = ActionModel.FromEasyCasterAction(task.Action);
            this.Tasks.Add(taskModel);
        }
    }

    private void UpdateHelpText()
    {
        if (IsEditing)
        {
            HelpText = LocalizationResourceManager.Current.GetValue("PressSAVEorCANCEL");
        }
        else
        {
            HelpText = LocalizationResourceManager.Current.GetValue("PressEDIT");
        }
    }

    [RelayCommand]
    private void SelectSaveMessagesPath()
    {
        var dialog = new VistaFolderBrowserDialog();
        dialog.Description = LocalizationResourceManager.Current.GetValue("SelectFolder");
        dialog.UseDescriptionForTitle = true;
        if ((bool)dialog.ShowDialog(Application.Current.MainWindow))
        {
            SaveMessagesPath = dialog.SelectedPath;
        }
    }

    [RelayCommand]
    private void AddTask()
    {
        var task = new TaskModel() { Action = new ActionModel(),DelayPeriod=5 };
        this.Tasks.Add(task);
    }

    [RelayCommand]
    private void TestTask(object taskObject)
    {
        var task = taskObject as TaskModel;
        if (task  != null)
        {
            var easyCasterAction = task.Action.ToEasyCasterAction();
            try
            {
                ActionInvoker.Invoke(easyCasterAction);
            }
            catch (Exception exception)
            {
                LoggerService.Instance.Error("TestTaskAction", exception.Message, exception);
                MessageBox.Show(
                    exception.Message, 
                    LocalizationResourceManager.Current.GetValue("TestError"), 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void SelectTaskProcess(object taskObject)
    {
        var task = taskObject as TaskModel;
        if (task != null)
        {
            var processSelectDialog = new ProcessSelectDialog();
            processSelectDialog.Owner = Application.Current.MainWindow;
            if (processSelectDialog.ShowDialog() == true)
            {
                task.Action.ApplicationName = processSelectDialog.SelectedProcess.Name;
            }
        }
    }

    [RelayCommand]
    private void SelectTaskCommand(object taskObject)
    {
        var task = taskObject as TaskModel;
        if (task != null)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = LocalizationResourceManager.Current.GetValue("SelectExecutableFile");
            openFileDialog.Filter = LocalizationResourceManager.Current.GetValue("Files");
            if (openFileDialog.ShowDialog() == true)
            {
                task.Action.CommandLine = openFileDialog.FileName;
            }
        }
    }

    [RelayCommand]
    private void DelTask(object taskObject)
    {
        var task = taskObject as TaskModel;
        if (task != null)
        {
            this.Tasks.Remove(task);
        }
    }

    [RelayCommand]
    private void AddEvent()
    {
        var @event = new EventModel() { Id = this.Events.Count + 1, Action = new ActionModel() };
        this.Events.Add(@event);
    }

    [RelayCommand]
    private void TestEvent(object eventObject)
    {
        var @event = eventObject as EventModel;
        if (@event != null)
        {
            var easyCasterAction = @event.Action.ToEasyCasterAction();
            try
            {
                ActionInvoker.Invoke(easyCasterAction);
            }
            catch( Exception exception )
            {
                LoggerService.Instance.Error("TestEventAction", exception.Message, exception);
                MessageBox.Show(
                    exception.Message, 
                    LocalizationResourceManager.Current.GetValue("TestError"), 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void SelectEventProcess(object eventObject)
    {
        var @event = eventObject as EventModel;
        if (@event != null)
        {
            var processSelectDialog = new ProcessSelectDialog();
            processSelectDialog.Owner = Application.Current.MainWindow;
            if (processSelectDialog.ShowDialog() == true)
            {
                @event.Action.ApplicationName = processSelectDialog.SelectedProcess.Name;
            }
        }
    }

    [RelayCommand]
    private void SelectEventCommand(object eventObject)
    {
        var @event = eventObject as EventModel;
        if (@event != null)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = LocalizationResourceManager.Current.GetValue("SelectExecutableFile");
            openFileDialog.Filter = LocalizationResourceManager.Current.GetValue("Files");
            if (openFileDialog.ShowDialog() == true)
            {
                @event.Action.CommandLine = openFileDialog.FileName;
            }
        }
    }

    [RelayCommand]
    private void DelEvent(object eventObject)
    {
        var @event = eventObject as EventModel;
        if (@event != null)
        {
            this.Events.Remove(@event);
            var index = 1;
            foreach (var eventItem in this.Events)
            {
                eventItem.Id = index++;
            }
        }
    }

    [RelayCommand]
    private void AddExludedText()
    {
        var lastIndex = this.ExcludedText.Count > 0 ? this.ExcludedText.Max(it => it.Index) + 1 : 0;
        this.ExcludedText.Add(new IndexedItem<string>(lastIndex, string.Empty));
    }

    [RelayCommand]
    private void DelExludedText(object stringIndex)
    {
        var itemToDelete = this.ExcludedText.Where(it => it.Index == (int)stringIndex).FirstOrDefault();
        if (itemToDelete != null)
            this.ExcludedText.Remove(itemToDelete);
    }

    [RelayCommand]
    private void Edit()
    {
        IsEditing = true;
        UpdateHelpText();
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            if (!this.SaveConfiguration())
                return;
        }
        catch (Exception ex)
        {
            var format = LocalizationResourceManager.Current.GetValue("SavingConfigurationError");
            var message = String.Format(format, ex.Message);
            loggerService.Error("ConfigurationEditor", message, ex);
            MessageBox.Show(
                Application.Current.MainWindow, 
                message,
                LocalizationResourceManager.Current.GetValue("Error"), 
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        IsEditing = false;
        UpdateHelpText();
    }

    [RelayCommand]
    private void Cancel()
    {
        LoadConfiguration();
        IsEditing = false;
        UpdateHelpText();
    }
}
