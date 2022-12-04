using System.Text.Json.Serialization;

namespace EasyCaster.Alarm.Core.Models;

public class EasyCasterAction
{
    //ApplicationName to activate
    public string ApplicationName { get; set; }

    //VirtualKeyCode to send after activation
    public EasyCasterKey EasyCasterKey { get; set; }

    //CommandLine to start
    public string CommandLine { get; set; }

    [JsonIgnore]
    public bool IsInvalid =>
        ApplicationName.IsEmpty()
        &&
        CommandLine.IsEmpty();

    [JsonIgnore]
    public bool IsValid => !IsInvalid;

    [JsonIgnore]
    public string DisplayString =>
        $"ApplicationName='{ApplicationName}',CommandLine='{CommandLine}'";

    
}
