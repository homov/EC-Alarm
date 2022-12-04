using EasyCaster.Alarm.Core.Interfaces;
using EasyCaster.Alarm.Core.Models;
using EasyCaster.Alarm.Helpers;
using EasyCaster.Alarm.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EasyCaster.Alarm.Services;

public class ConfigurationService : IConfiguration
{
    static ConfigurationService instance;

    public static ConfigurationService Instance
    {
        get
        {
            if (instance == null)
                instance = new ConfigurationService();
            return instance;
        }
    }

    private Configuration configuration = new();

    public Configuration Configuration => configuration;

    public List<EasyCasterEvent> Events => configuration.Events;

    public string PrimaryChannel => configuration.PrimaryChannel;
    
    public string TestChannel => configuration.TestChannel;

    public List<EasyCasterTask> PeriodicTasks => configuration.PeriodicTasks;

    public List<string> ExcludeText => configuration.ExcludeText;

    public string WebHookUrl => configuration.WebHookUrl;

    public string HomeDirectory
    {
        get
        {
            var path= System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return System.IO.Path.Combine(path, "EasyCaster.Alarm");
        }
    }

    public event Func<Task> ConfigurationChanged;

    public string ConfigurationFileName
    {
        get
        {
            return System.IO.Path.Combine(HomeDirectory, "EasyCaster.Alarm.config");
        }
    }

    public string DatabaseFile => System.IO.Path.Combine(HomeDirectory, "EasyCaster.Alarm.db");

    public string SessionFileName
    {
        get
        {
            return System.IO.Path.Combine(HomeDirectory, $"{Configuration.Phone}.session");
        }
    }

    public string GetTelegramValue(string valueName)
    {
        switch(valueName)
        {
            case "api_id": 
                return Constants.TelegramApiId;
            case "api_hash": 
                return Constants.TelegramApiHash;

            case "first_name":
            case "last_name":
                MessageBox.Show(
                    "Telegram asks to register a new user. This program does not support this feature. If you are already registered, perhaps you have entered the wrong phone number?",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return Core.Constants.TelegramCancelValue;

            case "password":
                if (String.IsNullOrWhiteSpace(Configuration.Password))
                {
                    MessageBox.Show(
                        "The password is not specified in the program configuration. Please enter a valid password",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return Core.Constants.TelegramCancelValue;
                }
                return Configuration.Password;

            case "phone_number":
            case "session_pathname":
                if (String.IsNullOrWhiteSpace(Configuration.Phone))
                {
                    MessageBox.Show(
                        "The phone is not specified in the program configuration. Please enter a valid phone number",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return Core.Constants.TelegramCancelValue;
                }
                if (valueName == "session_pathname")
                    return this.SessionFileName;
                return Configuration.Phone;

            case "email_verification_code":
            case "verification_code":
                var value = Core.Constants.TelegramCancelValue;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    if (mainWindow != null)
                    {
                        value = mainWindow.GetTelegramValue(valueName);

                    }
                });
                return value;
            default:
                return null;
        }
    }

    public ConfigurationService()
    {
        IOHelpers.EnsureDirectoryExists(HomeDirectory);
        LoadConfiguration();
    }

    public void LoadConfiguration()
    {
        if (System.IO.File.Exists(ConfigurationFileName)) 
        {
            configuration = IOHelpers.ReadJson<Configuration>(ConfigurationFileName);
            configuration.AutoStart = RegistryHelper.IsAutoStart();
        }
    }

    public void SaveConfiguration()
    {
        IOHelpers.WriteJson( ConfigurationFileName, configuration );
    }

    public void UpdateConfiguration(Configuration newConfiguration)
    {
        configuration = newConfiguration;
        SaveConfiguration();
        try
        {
            RegistryHelper.SetAutoStart(configuration.AutoStart);
        }
        catch(Exception ) 
        { 
        }
        if (ConfigurationChanged != null)
            ConfigurationChanged.Invoke();
    }
}
