using EasyCaster.Alarm.Core.Interfaces;
using EasyCaster.Alarm.Core.Models;

namespace EasyCaster.Alarm.Core.Services;

public class MessageHandler
{
    private readonly IConfiguration configuration;
    private readonly IMessageReader messageReader;
    private readonly ILogger logger;

    public event Func<EasyCasterEvent, EasyCasterMessage,Task> HandleEvent;

    public MessageHandler(IConfiguration configuration, IMessageReader messageReader, ILogger logger)
	{
        this.configuration = configuration;
        this.messageReader = messageReader;
        this.logger = logger;
        this.messageReader.MessageArrived += OnMessageArrived;
    }

    private Task OnMessageArrived(EasyCasterMessage message)
    {
        var isMonitoringGroup = IsMonitoringGroup(message);
        if (isMonitoringGroup)
        {
            foreach( var easyCasterEvent in GetMatchedEvents(message) )
            {
                HandleEvent?.Invoke(easyCasterEvent,message);
            }
        }
        return Task.CompletedTask;
    }

    private bool IsMonitoringGroup(EasyCasterMessage message) 
    {
        var groups = new List<string>();
        if (!configuration.PrimaryChannel.IsEmpty())
            groups.Add(configuration.PrimaryChannel.Split("/").Last().Trim().ToLower());
        if (!configuration.TestChannel.IsEmpty())
            groups.Add(configuration.TestChannel.Split("/").Last().Trim().ToLower());

        var groupName = message.NormalizedGroup;
        return groups.Any(it=> groupName.IndexOf(it)>=0);
    }

    private bool HasExcludedText(string message)
    {
        foreach(var exludeTextItem in configuration.ExcludeText)
        {
            if (message.IndexOf(exludeTextItem.Trim().ToLower()) >= 0)
                return true;
        }
        return false;
    }

    private IEnumerable<EasyCasterEvent> GetMatchedEvents(EasyCasterMessage message)
    {
        var messageText = message.NormalizedMessageText;
        return configuration.Events.Where(eventItem =>
            messageText.IndexOf(eventItem.NormalizedTextToFind) >= 0
            && !HasExcludedText(messageText));
    }
}
