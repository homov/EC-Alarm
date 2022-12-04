using CommunityToolkit.Mvvm.ComponentModel;
using EasyCaster.Alarm.Core.Models;

namespace EasyCaster.Alarm.Models
{
    public partial class TaskModel: ObservableObject
    {
        [ObservableProperty]
        EventModel startEvent;
        
        [ObservableProperty]
        EventModel stopEvent;

        [ObservableProperty]
        int delayPeriod;

        [ObservableProperty]
        ActionModel action;

        public EasyCasterTask ToEasyCasterTask()
        {
            EasyCasterTask task = new();
            if (startEvent!= null)
            {
                task.StartEventId = startEvent.Id;
            }
            if (stopEvent != null)
            {
                task.StopEventId = stopEvent.Id;
            }
            task.DelayPeriod = delayPeriod;
            if (action != null)
            {
                task.Action = action.ToEasyCasterAction();
            }
            return task;
        }
    }
}
