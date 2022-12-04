using EasyCaster.Alarm.Core.Helpers;
using EasyCaster.Alarm.Core.Interfaces;
using EasyCaster.Alarm.Core.Models;
using InputSimulatorStandard;
using InputSimulatorStandard.Native;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace EasyCaster.Alarm.Core.Services;

public class ActionHandler
{
    const string LogSource = nameof(ActionHandler);

    private readonly IConfiguration configuration;
    private readonly ILogger logger;
    private readonly MessageHandler messageHandler;

    public ActionHandler(IConfiguration configuration, ILogger logger, MessageHandler messageHandler)
    {
        this.configuration = configuration;
        this.logger = logger;
        this.messageHandler = messageHandler;
        this.messageHandler.HandleEvent += OnHandleEvent;
    }

    private Task OnHandleEvent(EasyCasterEvent easyCasterEvent, EasyCasterMessage easyCasterMessage)
    {
        if (easyCasterEvent.Action.IsValid)
        {
            try
            {
                ActionInvoker.Invoke(easyCasterEvent.Action);
            }
            catch (Exception exception)
            {
                var message = $"Error executing action: {easyCasterEvent.Action.DisplayString}";
                logger.Log(LogSource, Constants.LogLevelError, message, exception);
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
