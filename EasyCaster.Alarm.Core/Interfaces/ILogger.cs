namespace EasyCaster.Alarm.Core.Interfaces;

public interface ILogger
{
    void Log( string source, int logLevel, string message, Exception exception=null );
}
