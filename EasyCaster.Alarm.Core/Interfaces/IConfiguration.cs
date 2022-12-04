using EasyCaster.Alarm.Core.Models;

namespace EasyCaster.Alarm.Core.Interfaces;

public interface IConfiguration
{
    string GetTelegramValue(string valueName);

    List<EasyCasterEvent> Events { get; }
    
    string PrimaryChannel { get; }
    
    string TestChannel { get; }

    List<EasyCasterTask> PeriodicTasks { get; }

    List<string> ExcludeText { get; }

    string WebHookUrl { get; }

    event Func<Task> ConfigurationChanged;

    string DatabaseFile { get; }
}
