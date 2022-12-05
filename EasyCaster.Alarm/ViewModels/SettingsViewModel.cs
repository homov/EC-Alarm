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
using System.Threading.Tasks;
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

    bool isLoading = false;
    bool isSaving = false;

    [ObservableProperty]
    bool isModified = false;

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
    string primaryChannelDescription;

    [ObservableProperty]
    string testChannel;

    [ObservableProperty]
    string testChannelDescription;

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
        configurationService.ConfigurationChanged += ConfigurationService_ConfigurationChanged;
        loggerService = LoggerService.Instance;
        UpdateHelpText();
        LoadConfiguration();
        PropertyChanged += SettingsViewModel_PropertyChanged;
    }

    private Task ConfigurationService_ConfigurationChanged()
    {
        if (!isSaving)
        {
            LoadConfiguration();
        }
        return Task.CompletedTask;
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

    private void SetModified()
    {
        if (!isLoading)
        {
            IsModified = true;
        }

    }

    private void SettingsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName!=nameof(IsModified))
        {
            if (e.PropertyName == nameof(Phone)
                || e.PropertyName == nameof(Password)
                || e.PropertyName == nameof(AutoStart)
                || e.PropertyName == nameof(AutoConnect)
                || e.PropertyName == nameof(PrimaryChannel)
                || e.PropertyName == nameof(PrimaryChannelDescription)
                || e.PropertyName == nameof(TestChannel)
                || e.PropertyName == nameof(TestChannelDescription)
                || e.PropertyName == nameof(WebHookUrl)
                || e.PropertyName == nameof(SaveMessagesPath)
                )
            {
                SetModified();
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
        configuration.PrimaryChannelDescription = this.PrimaryChannelDescription;
        configuration.TestChannel = this.TestChannel;
        configuration.TestChannelDescription = this.TestChannelDescription;
        configuration.WebHookUrl = this.WebHookUrl;
        configuration.SaveMessagesPath = this.SaveMessagesPath;

        foreach (var text in this.ExcludedText)
        {
            if (!String.IsNullOrWhiteSpace(text.Value))
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
        try
        {
            isSaving = true;
            configurationService.UpdateConfiguration(configuration);
        }
        finally
        {
            isSaving = false;
        }
        return true;
    }

    private void LoadConfiguration()
    {
        isLoading = true;
        var configuration = configurationService.Configuration;
        this.Phone = configuration.Phone;
        this.Password = configuration.Password;
        this.AutoStart = configuration.AutoStart;
        this.AutoConnect = configuration.AutoConnect;
        this.PrimaryChannel = configuration.PrimaryChannel;
        this.PrimaryChannelDescription = configuration.PrimaryChannelDescription;
        this.TestChannel = configuration.TestChannel;
        this.TestChannelDescription = configuration.TestChannelDescription;
        this.WebHookUrl = configuration.WebHookUrl;
        this.SaveMessagesPath = configuration.SaveMessagesPath;
        this.ExcludedText.Clear();
        this.Events.Clear();
        this.Tasks.Clear();

        foreach (var text in configuration.ExcludeText)
        {
            var item = new IndexedItem<string>(this.ExcludedText.Count + 1, text);
            item.PropertyChanged += (_, _) => SetModified();
            this.ExcludedText.Add(item);
        }
        foreach (var @event in configuration.Events)
        {
            var item = EventModel.FromEasyCasterEvent(@event);
            item.PropertyChanged += (_, _) => SetModified();
            item.Action.PropertyChanged += (_, _) => SetModified();
            this.Events.Add(item);
        }
        foreach (var task in configuration.PeriodicTasks)
        {
            var taskModel = new TaskModel();
            taskModel.DelayPeriod = task.DelayPeriod;
            taskModel.StartEvent = this.Events.Where(it => it.Id == task.StartEventId).FirstOrDefault();
            taskModel.StopEvent = this.Events.Where(it => it.Id == task.StopEventId).FirstOrDefault();
            taskModel.Action = ActionModel.FromEasyCasterAction(task.Action);
            taskModel.Action.PropertyChanged += (_, _) => SetModified();
            taskModel.PropertyChanged += (_, _) => SetModified();
            this.Tasks.Add(taskModel);
        }
        isLoading = false;
        IsModified = false;
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
        task.PropertyChanged += (_, _) => SetModified();
        this.Tasks.Add(task);
        IsModified = true;
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
            var result = MessageBox.Show(
                LocalizationResourceManager.Current.GetValue("ConfirmDeleteTask"),
                LocalizationResourceManager.Current.GetValue("Confirm"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (result != MessageBoxResult.Yes) return;
            this.Tasks.Remove(task);
            IsModified = true;
        }
        
    }

    [RelayCommand]
    private void AddEvent()
    {
        var @event = new EventModel() { Id = this.Events.Count + 1, Action = new ActionModel() };
        @event.PropertyChanged += (_, _) => SetModified();
        this.Events.Add(@event);
        IsModified = true;
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
            var result = MessageBox.Show(
               LocalizationResourceManager.Current.GetValue("ConfirmDeleteMessage"),
               LocalizationResourceManager.Current.GetValue("Confirm"),
               MessageBoxButton.YesNo,
               MessageBoxImage.Question
           );
            if (result != MessageBoxResult.Yes) return;
            this.Events.Remove(@event);
            var index = 1;
            foreach (var eventItem in this.Events)
            {
                eventItem.Id = index++;
            }
            IsModified = true;
        }
    }

    [RelayCommand]
    private void AddExludedText()
    {
        var lastIndex = this.ExcludedText.Count > 0 ? this.ExcludedText.Max(it => it.Index) + 1 : 0;
        var item = new IndexedItem<string>(lastIndex, string.Empty);
        item.PropertyChanged += (_, _) => SetModified();
        this.ExcludedText.Add(item);
        IsModified = true;
    }

    [RelayCommand]
    private void DelExludedText(object stringIndex)
    {
        var itemToDelete = this.ExcludedText.Where(it => it.Index == (int)stringIndex).FirstOrDefault();
        if (itemToDelete != null)
        {
            var result = MessageBox.Show(
                   LocalizationResourceManager.Current.GetValue("ConfirmDeleteExclusion"),
                   LocalizationResourceManager.Current.GetValue("Confirm"),
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Question
                );
            if (result != MessageBoxResult.Yes) return;
            this.ExcludedText.Remove(itemToDelete);
            IsModified = true;
        }
    }

    [RelayCommand]
    private void Edit()
    {
        IsModified = false;
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
            IsModified = false;
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
        IsModified = false;
        UpdateHelpText();
    }
}
