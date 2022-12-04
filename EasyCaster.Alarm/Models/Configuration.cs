using EasyCaster.Alarm.Core.Models;
using System.Collections.Generic;
using System;

namespace EasyCaster.Alarm.Models;

public class Configuration
{
    public List<EasyCasterEvent> Events { get; set; } = new();

    public string PrimaryChannel { get; set; }
    
    public string TestChannel { get; set; }

    public List<EasyCasterTask> PeriodicTasks { get; set; } = new();

    public List<string> ExcludeText { get; set; } = new();

    public string WebHookUrl { get; set; } = String.Empty;

    public bool AutoStart { get; set; } = false;

    public bool AutoConnect { get; set; } = false;

    public string Phone { get; set; }
    
    public string Password { get; set; }

    public string SaveMessagesPath { get; set; }

    public string Language { get; set; } = "uk";

}
