using EasyCaster.Alarm.Core.Interfaces;
using EasyCaster.Alarm.Models;
using System.Windows;

namespace EasyCaster.Alarm.Services
{
    public class ExceptionTrackService
    {
        private readonly IMessageReader messageReader;

        public ExceptionTrackService(IMessageReader messageReader)
        {
            this.messageReader = messageReader;
            LoggerService.Instance.LogMessageRecieved += (logRecord) =>
            {
                if (logRecord.Exception != null)
                    TrackException( logRecord);
            };
        }

        private string GetErrorMessage(TL.RpcException rpcEception)
        {
            switch (rpcEception.Message)
            {
                case "PHONE_NUMBER_FLOOD":
                    return Properties.Resources.PHONE_NUMBER_FLOOD;
                case "PHONE_NUMBER_INVALID":
                    return Properties.Resources.PHONE_NUMBER_INVALID;
                case "PHONE_CODE_EMPTY":
                case "PHONE_CODE_INVALID":
                    return Properties.Resources.PHONE_CODE_INVALID;
                case "PHONE_NUMBER_BANNED":
                    return Properties.Resources.PHONE_NUMBER_BANNED;
                default:
                    return $"Telegram reports error:\n\n{rpcEception.Message}";
            }
        }
        
        private void TrackException(LogEventArgs logEvent)
        {
            if (logEvent.Exception is TL.RpcException rpcEception)
            {
                if (Application.Current != null)
                {
                    messageReader.Stop();
                    if (rpcEception.Message.StartsWith("PHONE_NUMBER"))
                    {
                        ConfigurationService.Instance.Configuration.Phone = null;
                    }

                    var message = GetErrorMessage(rpcEception);

                    Application.Current.Dispatcher.Invoke(() => { 
                        MessageBox.Show(
                               Application.Current.MainWindow,
                               message,
                               "Telegram error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error
                        );
                    });
                }
            }
        }

        public void Start()
        {

        }
    }
}
