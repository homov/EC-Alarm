using System.Text.Json.Serialization;

namespace EasyCaster.Alarm.Core.Models;

public class EasyCasterMessage
{
    public DateTime TimeStamp { get; } = DateTime.Now;

    public string Group { get; set; }
    
    public string MessageText { get; set; }

    [JsonIgnore]
    public string NormalizedMessageText =>
        MessageText.IsEmpty() ? "" : MessageText.Trim().ToLower();
    
    [JsonIgnore]
    public string NormalizedGroup =>
        Group.IsEmpty() ? "" : Group.Trim().ToLower();
}
