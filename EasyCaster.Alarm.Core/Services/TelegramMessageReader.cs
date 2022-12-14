using EasyCaster.Alarm.Core.Enums;
using EasyCaster.Alarm.Core.Exceptions;
using EasyCaster.Alarm.Core.Interfaces;
using EasyCaster.Alarm.Core.Models;
using System.Buffers.Binary;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.Json;
using TL;

namespace EasyCaster.Alarm.Core.Services;

public class TelegramMessageReader : IMessageReader
{
    const string LogSource = "MessageReader";

    private WTelegram.Client telegramClient = null;

    private readonly IConfiguration configuration;
    private readonly ILogger logger;
    private readonly Dictionary<long, User> users = new();
    private readonly Dictionary<long, ChatBase> chats = new();
    private bool watchDogRunning = false;
    private CancellationTokenSource tokenSource;
    private Dictionary<string, int> lastGroupMessages = new();

    public event Func<EasyCasterMessage, Task> MessageArrived;
    public event Func<ConnectionState, Task> ConnectionStateChanged;

    public ConnectionState ConnectionState { get; private set; }

    public TelegramMessageReader(IConfiguration configuration, ILogger logger)
    {
        this.configuration = configuration;
        this.logger = logger;
        this.ConnectionState = ConnectionState.Disconnected;
        this.configuration.ConfigurationChanged += RestartOnConfigurationChanged;
    }

    private async Task RestartOnConfigurationChanged()
    {
        if (ConnectionState== ConnectionState.Connected || ConnectionState == ConnectionState.Connecting)
        {
            await Stop();
            await Start();
        }
    }

    private string TelegramUser(long id)
    {
        return users.TryGetValue(id, out var user) ? user.ToString() : $"User {id}";
    }

    private string TelegramChat(long id)
    {
        return chats.TryGetValue(id, out var chat) ? chat.ToString() : $"Chat {id}";
    }

    private string TelegramPeer(Peer peer)
    {
        return peer is null ? null : peer is PeerUser user ? TelegramUser(user.user_id)
            : peer is PeerChat or PeerChannel ? TelegramChat(peer.ID) : $"Peer {peer.ID}";
    }

    private async Task HandleUpdate(IObject updateObject)
    {
        if (updateObject is not UpdatesBase update)
            return;

        update.CollectUsersChats(users, chats);
        foreach (var updateItem in update.UpdateList)
        {
            switch (updateItem)
            {
                case UpdateNewMessage updateNewMessage:
                    await HandleMessageUpdate(updateNewMessage.message);
                    break;
            }
        }
    }

    private async Task HandleMessageUpdate(MessageBase messageBase)
    {
        if (messageBase is Message message)
        {
            var group = TelegramPeer(message.peer_id);
            var messageText = message.message;


            //Prevent duplicates
            int lastMessageId;
            if (lastGroupMessages.TryGetValue(group, out lastMessageId))
            {
                if (lastMessageId >= message.ID)
                    return;
            }
            lastGroupMessages[group] = message.ID;
            WriteDatabase();

            if (!string.IsNullOrEmpty(messageText))
            {
                var telegramMessage = new EasyCasterMessage()
                {
                    Group = group,
                    MessageText = messageText
                };
                if (MessageArrived != null)
                    await MessageArrived.Invoke(telegramMessage);
            }
        }
    }

    private void SetConnectionState(ConnectionState connectionState)
    {
        if (this.ConnectionState != connectionState)
        {
            this.ConnectionState = connectionState;
            if (ConnectionStateChanged != null)
            {
                ConnectionStateChanged.Invoke(this.ConnectionState);
            }
        }
    }

    private string GetTelegramConfig(string valueName)
    {
        var value = configuration.GetTelegramValue(valueName);
        if (value == Core.Constants.TelegramCancelValue)
        {
            logger.Log(LogSource, Constants.LogLevelError, "Login sequence canceled");
            throw new LoginSequenceCanceledException();
        }
        return value;
    }

    private async Task Login()
    {
        //Login using phone
        var valueName = "phone_number";
        var value = configuration.GetTelegramValue(valueName);
        if (value == Constants.TelegramCancelValue)
        {
            return;
        }
        valueName = await telegramClient.Login(value);

        //Other telegram data
        while (telegramClient.User == null)
        {
            value = configuration.GetTelegramValue(valueName);
            if (value == Constants.TelegramCancelValue)
            {
                break;
            }
            valueName = await telegramClient.Login(value);
        }
    }

    private bool isInternetConnectionFound()
    {
        try
        {
            Ping myPing = new Ping();
            String host = "8.8.8.8";
            byte[] buffer = new byte[32];
            int timeout = 1000;
            PingOptions pingOptions = new PingOptions();
            PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
            return (reply.Status == IPStatus.Success);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task WatchDog(CancellationToken cancellationToken)
    {
        bool firstConnect = true;
        await Task.Delay(2000);

        try
        {
            watchDogRunning = true;
            logger.Log(LogSource, Constants.LogLevelTrace, "Watch dog started");

            while (!cancellationToken.IsCancellationRequested)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                if (telegramClient != null)
                {
                    if (telegramClient.Disconnected || telegramClient.User == null)
                    {
                        SetConnectionState(ConnectionState.Connecting);
                        logger.Log(LogSource, Constants.LogLevelInformation, "Reconnecting...");
                        if (isInternetConnectionFound())
                        {

                            try
                            {
                                if (!firstConnect)
                                {
                                    telegramClient.Reset(false, true);
                                    if (cancellationToken.IsCancellationRequested)
                                        break;
                                }

                                await telegramClient.LoginUserIfNeeded();
                                firstConnect = false;

                                if (cancellationToken.IsCancellationRequested)
                                    break;
                                if (!telegramClient.Disconnected)
                                {
                                    SetConnectionState(ConnectionState.Connected);
                                    logger.Log(LogSource, Constants.LogLevelInformation, "Reconnected");
                                }
                                else
                                {
                                    logger.Log(LogSource, Constants.LogLevelInformation, "Reconnection failed");
                                }
                            }
                            catch (Exception exeception)
                            {
                                logger.Log(LogSource,
                                    Constants.LogLevelError,
                                    $"Error while reconnecting {exeception.Message}",
                                    exeception);
                            }
                        }
                        else
                        {
                            logger.Log(LogSource, Constants.LogLevelInformation, "Not connected to internet");
                        }
                    }
                }
                await Task.Delay(6000, cancellationToken);
            }
        }
        finally
        {
            watchDogRunning = false;
            logger.Log(LogSource, Constants.LogLevelTrace, "Watch dog stopped");
        }
    }

    private void ReadDatabase()
    {
        if (File.Exists(configuration.DatabaseFile))
        {
            try
            {
                var stringContent = System.IO.File.ReadAllText(configuration.DatabaseFile);
                var dataBase = JsonSerializer.Deserialize<Dictionary<string, int>>(stringContent);
                lastGroupMessages = dataBase;
            }
            catch(Exception)
            {

            }
        }
    }

    private void WriteDatabase()
    {
        try
        {
            var stringContent = JsonSerializer.Serialize(lastGroupMessages);
            System.IO.File.WriteAllText(configuration.DatabaseFile, stringContent);
        }
        catch (Exception)
        {

        }
    }

    public async Task Start()
    {
        var isCanceled = false;
        try
        {
            ReadDatabase();
            SetConnectionState(ConnectionState.Connecting);

            if (telegramClient == null)
            {
                
                WTelegram.Helpers.Log = (level, message) => logger.Log(LogSource, level, message);
                telegramClient = new WTelegram.Client(GetTelegramConfig, new SessionStore(configuration.GetTelegramValue("session_pathname") ));
                telegramClient.OnUpdate += HandleUpdate;

                tokenSource = new CancellationTokenSource();
                WatchDog(tokenSource.Token);

                //try
                //{
                //    await telegramClient.LoginUserIfNeeded();
                //}
                //catch (LoginSequenceCanceledException e)
                //{
                //    await Stop();
                //    isCanceled = true;
                //}

                //catch (Exception)
                //{

                //}
                //if (!isCanceled)
                //{
                //    tokenSource = new CancellationTokenSource();
                //    WatchDog(tokenSource.Token);
                //}
            }
        }
        catch (LoginSequenceCanceledException)
        {
            await Stop();
        }
        catch (Exception exception)
        {
            logger.Log(LogSource, Constants.LogLevelError, exception.Message, exception);
        }
        finally
        {
            if (telegramClient != null && telegramClient.User != null && !telegramClient.Disconnected)
            {
                SetConnectionState(ConnectionState.Connected);
            }
        }
    }

    public async Task Stop()
    {
        try
        {
            if (telegramClient != null)
            {
                //Stop watch dog
                if (watchDogRunning)
                {
                    tokenSource.Cancel();

                    while (watchDogRunning)
                    {
                        await Task.Delay(100);
                    }
                    tokenSource.Dispose();
                    tokenSource = null;
                }

                telegramClient.OnUpdate -= HandleUpdate;
                telegramClient.Dispose();
                WTelegram.Helpers.Log = (lvl, str) => { };
                telegramClient = null;
            }
        }
        finally
        {
            SetConnectionState(ConnectionState.Disconnected);
        }
    }

    internal class SessionStore : FileStream
    {
        public override long Length { get; }
        public override long Position { get => base.Position; set { } }
        public override void SetLength(long value) { }
        private readonly byte[] _header = new byte[8];
        private int _nextPosition = 8;

        public SessionStore(string pathname)
            : base(pathname, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1) // no in-app buffering
        {
            if (base.Read(_header, 0, 8) == 8)
            {
                var position = BinaryPrimitives.ReadInt32LittleEndian(_header);
                var length = BinaryPrimitives.ReadInt32LittleEndian(_header.AsSpan(4));
                base.Position = position;
                Length = length;
                _nextPosition = position + length;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_nextPosition > count * 3) _nextPosition = 8;
            base.Position = _nextPosition;
            base.Write(buffer, offset, count);
            BinaryPrimitives.WriteInt32LittleEndian(_header, _nextPosition);
            BinaryPrimitives.WriteInt32LittleEndian(_header.AsSpan(4), count);
            _nextPosition += count;
            base.Position = 0;
            base.Write(_header, 0, 8);
            base.Flush(true);
        }
    }

}
