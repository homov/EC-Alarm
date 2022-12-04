using EasyCaster.Alarm.Core.Helpers;
using EasyCaster.Alarm.Core.Interfaces;
using EasyCaster.Alarm.Core.Models;

namespace EasyCaster.Alarm.Core.Services;

public class ScheduleHandler
{
    class ScheduledTask
    {
        private readonly EasyCasterTask easyCasterTask;
        private readonly ILogger logger;
        private readonly CancellationTokenSource cancellationTokenSource;

        public EasyCasterTask EasyCasterTask => easyCasterTask;
        
        public ScheduledTask(EasyCasterTask easyCasterTask, ILogger logger)
        {
            this.easyCasterTask = easyCasterTask;
            this.logger = logger;
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task Start()
        {
            while( !cancellationTokenSource.Token.IsCancellationRequested )
            {
                try
                {
                    ActionInvoker.Invoke(easyCasterTask.Action);
                }
                catch (Exception exception)
                {
                    var message = $"Error executing action: {easyCasterTask.Action.DisplayString}";
                    this.logger.Log(nameof(ScheduledTask), Constants.LogLevelError, message, exception);
                }
                
                if (cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                await Task.Delay( 
                    TimeSpan.FromSeconds((int)easyCasterTask.DelayPeriod),
                    cancellationTokenSource.Token
                );
            }
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }
    }

    private readonly IConfiguration configuration;
    private readonly ILogger logger;
    private readonly MessageHandler messageHandler;
    private List<ScheduledTask> scheduledTasks;
    private object lockScheduledTasks = new();

    public ScheduleHandler(IConfiguration configuration, ILogger logger, MessageHandler messageHandler)
    {
        this.configuration = configuration;
        this.logger = logger;
        this.messageHandler = messageHandler;
        this.scheduledTasks = new List<ScheduledTask>();
        this.messageHandler.HandleEvent += OnHandleEvent;
        this.configuration.ConfigurationChanged += RestartOnConfigurationChanged;
    }

    private void StopAll()
    {
        lock (lockScheduledTasks)
        {
            foreach (var task in scheduledTasks)
            {
                task.Stop();
                this.scheduledTasks.Remove(task);
            }
        }
    }

    private Task RestartOnConfigurationChanged()
    {
        if (this.scheduledTasks.Count >0)
        {
            StopAll();
        }
        return Task.CompletedTask;
    }

    private async Task OnHandleEvent(EasyCasterEvent easyCasterEvent, EasyCasterMessage easyCasterMessage)
    {
        try
        {
            await StartOrStopSchedule(easyCasterEvent.Id);
        }
        catch (Exception exception)
        {
            var errorMessage = $"Error in event schedule Event.Id={easyCasterEvent.Id}";
            logger.Log(nameof(ScheduleHandler), Constants.LogLevelError, errorMessage, exception);
        }
    }

    private Task StartOrStopSchedule(int eventId)
    {
        lock (lockScheduledTasks)
        {
            //Find tasks to stop
            var stopTasks = scheduledTasks.Where(it => it.EasyCasterTask.StopEventId == eventId).ToList();
            foreach (var task in stopTasks)
            {
                task.Stop();
                this.scheduledTasks.Remove(task);
            }
            //Find tasks to start
            var startTasks = configuration.PeriodicTasks.Where(it => it.StartEventId == eventId).ToList();
            foreach (var task in startTasks)
            {
                //Is taks valid?
                if (!task.IsValid)
                    continue;

                //Is task already started?
                if (scheduledTasks.Where(it => it.EasyCasterTask == task).Any())
                    continue;

                var scheduledTask = new ScheduledTask(task, logger);
                scheduledTasks.Add(scheduledTask);
                scheduledTask.Start();
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
        StopAll();
        return Task.CompletedTask;
    }
}
