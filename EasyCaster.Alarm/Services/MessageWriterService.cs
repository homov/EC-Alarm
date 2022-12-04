using EasyCaster.Alarm.Core.Interfaces;
using EasyCaster.Alarm.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EasyCaster.Alarm.Services;

public class MessageWriterService
{
    private readonly ILogger logger;
    private readonly MessageHandler messageHandler;

    public MessageWriterService(ILogger logger, MessageHandler messageHandler)
	{
        this.logger = logger;
        this.messageHandler = messageHandler;
        messageHandler.HandleEvent += MessageHandler_HandleEvent;
    }

    private Task MessageHandler_HandleEvent(Core.Models.EasyCasterEvent easyCasterEvent, Core.Models.EasyCasterMessage easyCasterMessage)
    {
        if (!String.IsNullOrWhiteSpace( ConfigurationService.Instance.Configuration.SaveMessagesPath) )
        {
            var fileName = Path.Combine(ConfigurationService.Instance.Configuration.SaveMessagesPath, $"{easyCasterEvent.Id}.txt");
            try
            {
                File.WriteAllText(fileName, easyCasterMessage.MessageText);
            }
            catch(Exception ex)
            {
                logger.Log("MessageFileWriter",Core.Constants.LogLevelError, $"Unable to write message {fileName} ({ex.Message})");
            }
        }
        return Task.CompletedTask;
    }

    public Task Start()
    {
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }
}
