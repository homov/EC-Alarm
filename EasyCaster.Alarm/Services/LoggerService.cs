using EasyCaster.Alarm.Core.Interfaces;
using EasyCaster.Alarm.Models;
using System;

namespace EasyCaster.Alarm.Services;

public class LoggerService : ILogger
{
    private static LoggerService instance;

    public static LoggerService Instance
    {
        get 
        { 
            if (instance == null)
                instance = new LoggerService();
            return instance; 
        }
    }

    public event Action<LogEventArgs> LogMessageRecieved;

    public void Log(string source, int logLevel, string message, Exception exception = null)
    {
        switch(logLevel)
        {
            case Core.Constants.LogLevelInformation:
                Serilog.Log.Information($"{source}: {message}");
                break;
            case Core.Constants.LogLevelDebug:
                Serilog.Log.Debug($"{source}: {message}");
                break;
            case Core.Constants.LogLevelWarning:
                Serilog.Log.Warning($"{source}: {message}");
                break;
            case Core.Constants.LogLevelTrace:
                Serilog.Log.Verbose($"{source}: {message}");
                break;
            case Core.Constants.LogLevelError:
                Serilog.Log.Error(exception,$"{source}: {message}");
                break;
            case Core.Constants.LogLevelFatal:
                Serilog.Log.Fatal(exception, $"{source}: {message}");
                break;
        }

        LogMessageRecieved?.Invoke( 
            new LogEventArgs() { Source=source,  LogLevel = logLevel, Message= message, Exception = exception }
        );    
    }

    public void Error(string source, string message, Exception exception )
    {
        Log(source, Core.Constants.LogLevelError, message, exception);
    }

    public void Debug(string source, string message)
    {
        Log(source, Core.Constants.LogLevelDebug, message);
    }

    public void Information(string source, string message)
    {
        Log(source, Core.Constants.LogLevelInformation, message);
    }

    public void Warning(string source, string message)
    {
        Log(source, Core.Constants.LogLevelWarning, message);
    }

    public void Trace(string source, string message)
    {
        Log(source, Core.Constants.LogLevelTrace, message);
    }
}
