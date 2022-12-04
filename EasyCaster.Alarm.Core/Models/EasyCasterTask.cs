using System.Text.Json.Serialization;

namespace EasyCaster.Alarm.Core.Models;

public class EasyCasterTask
{
    public int? StartEventId { get; set; } = null;

    public int? StopEventId { get; set; } = null;

    public int DelayPeriod { get; set; } = 0; // Delay period of task in seconds

    public EasyCasterAction Action { get; set; }

    [JsonIgnore]
    public bool IsValid => 
        StartEventId!=null && StopEventId!=null && DelayPeriod >0 && Action.IsValid;

    public EasyCasterTask()
    {
        Action = new EasyCasterAction();
    }
}
