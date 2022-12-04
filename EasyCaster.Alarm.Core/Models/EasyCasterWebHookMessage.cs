using System.Text.Json.Serialization;

namespace EasyCaster.Alarm.Core.Models;

public class EasyCasterWebHookMessage
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("targetMessage")]
    public string TargetMessage { get; set; }

    [JsonPropertyName("dateTime")]
    public string DateTime { get; set; }
}
