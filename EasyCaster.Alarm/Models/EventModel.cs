using CommunityToolkit.Mvvm.ComponentModel;
using EasyCaster.Alarm.Core.Models;

namespace EasyCaster.Alarm.Models;

public partial class EventModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayString))]
    int id;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayString))]
    string textToFind;

    [ObservableProperty]
    ActionModel action;

    public string DisplayString => $"{id} ({textToFind})";

    public static EventModel FromEasyCasterEvent(EasyCasterEvent easyCasterEvent)
    {
        EventModel model = new()
        {
            Id = easyCasterEvent.Id,
            TextToFind = easyCasterEvent.TextToFind,
            Action = ActionModel.FromEasyCasterAction(easyCasterEvent.Action)
        };
        return model;
    }

    public EasyCasterEvent ToEasyCasterEvent()
    {
        EasyCasterEvent model = new(id)
        {
            TextToFind = textToFind,
            Action = action.ToEasyCasterAction()
        };
        return model;
    }
}
