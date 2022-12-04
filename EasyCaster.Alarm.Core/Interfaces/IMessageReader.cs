using EasyCaster.Alarm.Core.Enums;
using EasyCaster.Alarm.Core.Models;

namespace EasyCaster.Alarm.Core.Interfaces;

public interface IMessageReader
{
    event Func<EasyCasterMessage,Task> MessageArrived;
    event Func<ConnectionState, Task> ConnectionStateChanged;
    
    public ConnectionState ConnectionState { get; }
    Task Start();
    Task Stop();
}
