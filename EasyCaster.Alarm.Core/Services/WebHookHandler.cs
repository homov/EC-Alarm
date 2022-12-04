using EasyCaster.Alarm.Core.Interfaces;
using EasyCaster.Alarm.Core.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace EasyCaster.Alarm.Core.Services;

public class WebHookHandler
{
    private readonly IConfiguration configuration;
    private readonly ILogger logger;
    private readonly MessageHandler messageHandler;

    public WebHookHandler(IConfiguration configuration, ILogger logger, MessageHandler messageHandler)

    {
        this.configuration = configuration;
        this.logger = logger;
        this.messageHandler = messageHandler;
        this.messageHandler.HandleEvent += OnHandleEvent;
    }

    private async Task OnHandleEvent(EasyCasterEvent easyCasterEvent, EasyCasterMessage easyCasterMessage)
    {
        if (!configuration.WebHookUrl.IsEmpty())
        {
            try
            {
                await InvokeWebHook(configuration.WebHookUrl, easyCasterEvent, easyCasterMessage);
            }
            catch (Exception exception)
            {
                var errorMessage = $"Error calling webhook {configuration.WebHookUrl}";
                logger.Log(nameof(WebHookHandler), Constants.LogLevelError, errorMessage, exception);

            }
        }
    }

    private async Task InvokeWebHook(string webHookUrl, EasyCasterEvent easyCasterEvent, EasyCasterMessage easyCasterMessage)
    {
        var webHookMessage = new EasyCasterWebHookMessage()
        {
            Id = easyCasterEvent.Id,
            Message = easyCasterEvent.TextToFind,
            TargetMessage = easyCasterMessage.MessageText,
            DateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        var webHookJson = JsonSerializer.Serialize(webHookMessage);
        var stringContent = new StringContent(webHookJson, Encoding.UTF8, "application/json");

        using (var httpClient = new HttpClient())
        {
            await httpClient.PostAsync(webHookUrl, stringContent);
        }
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
