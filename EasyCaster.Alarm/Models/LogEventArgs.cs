using System;

namespace EasyCaster.Alarm.Models;

public class LogEventArgs
{
    public string Source { get; set; }
    public int LogLevel { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }
}
