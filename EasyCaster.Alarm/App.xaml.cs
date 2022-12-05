using Autofac;
using EasyCaster.Alarm.Core.Interfaces;
using EasyCaster.Alarm.Core.Services;
using EasyCaster.Alarm.Helpers;
using EasyCaster.Alarm.Services;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EasyCaster.Alarm
{
    public partial class App : Application
    {
        static readonly Lazy<IContainer> containerHolder = new(CreateContainer);
        static IContainer Container => containerHolder.Value;
        static bool isApplicationInitialized = false;

        private const string UniqueEventName = "bcdc10d6-5d85-492c-89cd-5a701598eb41";
        private const string UniqueMutexName = "4cc298ad-e015-4fec-99cb-0b8683f289c1";
        private EventWaitHandle eventWaitHandle;
        private Mutex mutex;



        public static T Resolve<T>()
        {
            return Container.Resolve<T>();
        }

        private static IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(ConfigurationService.Instance).As<IConfiguration>();
            builder.RegisterType<LanguageService>().AsSelf();
            builder.RegisterInstance(LoggerService.Instance).As<Core.Interfaces.ILogger>();
            builder.RegisterType<TelegramMessageReader>().As<IMessageReader>().SingleInstance();
            builder.RegisterType<MessageHandler>().AsSelf().SingleInstance();
            builder.RegisterType<ActionHandler>().AsSelf().SingleInstance();
            builder.RegisterType<ScheduleHandler>().AsSelf().SingleInstance();
            builder.RegisterType<WebHookHandler>().AsSelf().SingleInstance();
            builder.RegisterType<MessageWriterService>().AsSelf().SingleInstance();
            builder.RegisterType<ExceptionTrackService>().AsSelf().SingleInstance();

            return builder.Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //Check only one instance of application. If application already running activate it.
            bool isOwned;
            this.mutex = new Mutex(true, UniqueMutexName, out isOwned);
            this.eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            GC.KeepAlive(this.mutex);

            if (isOwned)
            {
                var thread = new Thread(
                    () =>
                    {
                        while (this.eventWaitHandle.WaitOne())
                        {
                            Current.Dispatcher.BeginInvoke(
                                (Action)(() => ((MainWindow)Current.MainWindow).BringToForeground()));
                        }
                    });
                thread.IsBackground = true;
                thread.Start();


                LocalizationResourceManager.Current.Init(
                    EasyCaster.Alarm.Properties.Resources.ResourceManager,
                    Thread.CurrentThread.CurrentUICulture);

                var languageService = Resolve<LanguageService>();
                languageService.Start();

                return;
            }

            this.eventWaitHandle.Set();
            this.Shutdown();
        }
       

        private void Application_Activated(object sender, EventArgs e)
        {
            if (!isApplicationInitialized)
            {
                isApplicationInitialized = true;

                //Setup file logger
                var logFile = Path.Combine(ConfigurationService.Instance.HomeDirectory, "Logs", "EasyCaster.Alarm.log");
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.File(logFile,
                        rollingInterval: RollingInterval.Day,
                        outputTemplate:"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        retainedFileCountLimit: 3)
                    .CreateLogger();

                this.Dispatcher.UnhandledException += Application_DispatcherUnhandledException;

                //Init GUI (log viewer, message viewer...)
                var mainWindow = App.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow!=null)
                {
                    mainWindow.Initialize();
                }

                ExceptionTrackService exceptionTrackService = Resolve<ExceptionTrackService>();
                exceptionTrackService.Start();

                //Autoconnect to the Telegram
                if (ConfigurationService.Instance.Configuration.AutoConnect)
                {
                    Task.Run(() =>
                    {
                        IMessageReader messageReader = Resolve<IMessageReader>();
                        messageReader.Start();

                    });
                }
                Task.Run(() =>
                {
                    ActionHandler actionHandler = Resolve<ActionHandler>();
                    actionHandler.Start();

                    ScheduleHandler scheduleHandler = Resolve<ScheduleHandler>();
                    scheduleHandler.Start();

                    WebHookHandler webHookHandler = Resolve<WebHookHandler>();
                    webHookHandler.Start();

                    MessageWriterService messageWriter = Resolve<MessageWriterService>();
                    messageWriter.Start();

                    
                });

                mainWindow.Closing += (_, e) =>
                {
                    var result = MessageBox.Show(
                       LocalizationResourceManager.Current.GetValue("ConfirmCloseApplication"),
                       LocalizationResourceManager.Current.GetValue("Confirm"),
                       MessageBoxButton.YesNo,
                       MessageBoxImage.Question
                    );
                    if (result == MessageBoxResult.Yes) return;
                    e.Cancel = true;
                };

                mainWindow.Closed += (_, _) =>
                {
                    Task.Run(() =>
                    {
                        IMessageReader messageReader = Resolve<IMessageReader>();
                        messageReader.Stop();
                    });
                };
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            LoggerService.Instance.Error("Application", e.Exception.Message, e.Exception);
        }
    }
}
