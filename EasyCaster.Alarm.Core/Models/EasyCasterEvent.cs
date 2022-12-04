using System.Text.Json.Serialization;

namespace EasyCaster.Alarm.Core.Models;

public class EasyCasterEvent
{
    public int Id { get; set; }
    
    public string TextToFind { get; set; }

    [JsonIgnore]
    public string NormalizedTextToFind =>
        TextToFind.IsEmpty() ? "" : TextToFind.Trim().ToLower();

    public EasyCasterAction Action { get; set; }

    [JsonIgnore]
    public bool IsValid => !NormalizedTextToFind.IsEmpty() && Action.IsValid;

    public EasyCasterEvent(int id)
    {
        Id = id;
        Action = new EasyCasterAction();
    }
}
