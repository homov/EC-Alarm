using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasyCaster.Alarm.Helpers;
using EasyCaster.Alarm.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace EasyCaster.Alarm.ViewModels;

public partial class LogViewModel: ObservableObject
{
    const int MaxRecords = 100;
    const int MaxDelta = 10;

    [ObservableProperty]
    LogLevel currentLogLevel;

    public ObservableCollection<LogRecord> LogRecords { get; } = new();
    public ObservableCollection<LogLevel> LogLevels { get; } = new();

    private object lockObject = new();

    public LogViewModel()
    {
        LogLevels.Add(new LogLevel()
        {
            Level = Core.Constants.LogLevelInformation,
            LevelName = LocalizationResourceManager.Current.GetValue("Information")
        });
        LogLevels.Add(new LogLevel()
        {
            Level = Core.Constants.LogLevelDebug,
            LevelName = LocalizationResourceManager.Current.GetValue("Debug")
        });
        LogLevels.Add(new LogLevel()
        {
            Level = Core.Constants.LogLevelTrace,
            LevelName = LocalizationResourceManager.Current.GetValue("Trace")
        });

        CurrentLogLevel = LogLevels.First();

        LoggerService.Instance.LogMessageRecieved += (message) =>
        {
            Log(message.Source, message.LogLevel, message.Message, message.Exception);
        };
    }

    [RelayCommand]
    void Clear()
    {
        lock (lockObject)
        {
            LogRecords.Clear();
        }
    }

    public virtual void Log(string source, int logLevel, string message, Exception exception = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (CurrentLogLevel.Level <= logLevel)
            {
                lock (lockObject)
                {
                    var recordsToKeep = MaxRecords + MaxDelta;

                    if (LogRecords.Count >= recordsToKeep)
                    {
                        while (LogRecords.Count > MaxRecords)
                            LogRecords.RemoveAt(0);
                    }
                    LogRecords.Add(new LogRecord(source, logLevel, message, exception));
                }
            }
        });
    }

    public class LogRecord
    {
        static Dictionary<int, string> logLevelStrings = new Dictionary<int, string>();
        static Dictionary<int, Brush> logLevelColors = new Dictionary<int, Brush>();

        static LogRecord()
        {
            logLevelStrings[Core.Constants.LogLevelError] = "E";
            logLevelStrings[Core.Constants.LogLevelWarning] = "W";
            logLevelStrings[Core.Constants.LogLevelTrace] = "T";
            logLevelStrings[Core.Constants.LogLevelFatal] = "F";
            logLevelStrings[Core.Constants.LogLevelDebug] = "D";
            logLevelStrings[Core.Constants.LogLevelInformation] = "I";

            logLevelColors[Core.Constants.LogLevelError] = Brushes.Red;
            logLevelColors[Core.Constants.LogLevelWarning] = Brushes.Yellow;
            logLevelColors[Core.Constants.LogLevelTrace] = Brushes.Gray;
            logLevelColors[Core.Constants.LogLevelFatal] = Brushes.Purple;
            logLevelColors[Core.Constants.LogLevelDebug] = Brushes.Green;
            logLevelColors[Core.Constants.LogLevelInformation] = Brushes.White;
        }

        public LogRecord(string source, int logLevel, string message, Exception exception)
        {
            Source = source;
            LogLevel = logLevel;
            Message = message;
            Exception = exception;
            TimeStamp = DateTime.Now;
        }

        public string Source { get; }
        public int LogLevel { get; }
        public string Message { get; }
        public Exception Exception { get; }
        public DateTime TimeStamp { get; }

        public string LogString => $"{LogLevelString} | {TimeStamp:HH:mm:ss} | {Source}: {Message}";

        public string LogLevelString
        {
            get
            {
                string logLevelString;
                if (logLevelStrings.TryGetValue(LogLevel, out logLevelString))
                    return logLevelString;
                return "?";
            }
        }

        public Brush LogLevelColor
        {
            get
            {
                Brush logLevelBrush;
                if (logLevelColors.TryGetValue(LogLevel, out logLevelBrush))
                    return logLevelBrush;
                return Brushes.Black;
            }
        }
    }

    public partial class LogLevel : ObservableObject
    {
        [ObservableProperty]
        int level;

        [ObservableProperty]
        string levelName;
    }

}
