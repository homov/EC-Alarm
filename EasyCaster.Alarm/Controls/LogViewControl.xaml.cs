using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using EasyCaster.Alarm.Core.Interfaces;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using EasyCaster.Alarm.Helpers;

namespace EasyCaster.Alarm.Controls
{
    public partial class LogViewControl : UserControl, ILogger
    {
        const int MaxRecords = 100;
        const int MaxDelta = 10;

        public int CurrentLogLevel { get; set; } = Core.Constants.LogLevelInformation;

        public ObservableCollection<LogRecord> LogRecords { get; } = new();
        public ObservableCollection<LogLevel> LogLevels { get; } = new();

        private object lockObject = new();

        public LogViewControl()
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

            InitializeComponent();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            lock (lockObject)
            {
                LogRecords.Clear();
            }
        }
        private void LogLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LogLevelComboBox.SelectedIndex >=0)
                CurrentLogLevel = LogLevels[LogLevelComboBox.SelectedIndex].Level;
        }

        public virtual void Log(string source, int logLevel, string message, Exception exception = null)
        {
            this.Dispatcher.Invoke(() => 
            {
                if (CurrentLogLevel <= logLevel)
                {
                    lock (lockObject)
                    {
                        var recordsToKeep = MaxRecords + MaxDelta;

                        if (LogRecords.Count >= recordsToKeep)
                        {
                            while (LogRecords.Count > MaxRecords)
                                LogRecords.RemoveAt(0);
                        }
                        LogRecords.Add(new LogRecord(source,logLevel, message, exception));
                        LogListBox.ScrollIntoView(LogRecords.Last());
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
                    return Brushes.White;
                }
            }
        }

        public partial class LogLevel: ObservableObject
        {
            [ObservableProperty]
            int level;

            [ObservableProperty]
            string levelName;
        }
    }
}
