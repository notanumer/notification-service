using BaseMicroservice;
using DatabaseService.Models.Rabbit;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using DatabaseService.Services.Abstractions;

namespace TelegramNotifier.Consumers
{
    public class TelegramMessagesConsumer : BaseRabbitConsumer
    {
        private readonly HttpClient client;
        private readonly IUserCredentialsService _credentialsService;
        private readonly ILogger<TelegramMessagesConsumer> _logger;

        public TelegramMessagesConsumer(
            IUserCredentialsService credentialsService,
            string telegramBotServiceAddress,
            string rabbitUri,
            string queueName,
            ILogger<TelegramMessagesConsumer> logger
        ) : base(rabbitUri, queueName)
        {
            client = new HttpClient() { BaseAddress = new Uri(telegramBotServiceAddress) };
            _credentialsService = credentialsService;
            _logger = logger;
        }

        public override async Task Handler(object model, BasicDeliverEventArgs args)
        {
            var ev = EventDeserializer.Deserialize(args);
            if (ev == null)
            {
                LogFailedSending(null, "Event is null");
                return;
            }

            var credential = await _credentialsService.GetCredentials(ev.Recipient, ChannelType.Telegram);
            if (credential == null)
            {
                LogFailedSending(ev, "Wrong credentials format");
                return;
            }

            JsonContent content = JsonContent.Create(new
            {
                UserId = long.Parse(credential),
                Text = ev.MessageContent.Text!
            });
            try
            {
                var resp = await client.PostAsync($"/bot/send", content);
                LogSuccessfulSending(ev);
            }
            catch (Exception ex)
            {
                LogFailedSending(ev, ex.Message);
            }

            LogReceivedMessage(ev);
            await Task.CompletedTask;
        }
        
        #region HandleSendResult

        private void LogSuccessfulSending(Event ev)
        {
            _logger.LogInformation($"Message with id - {ev.NotificationId} to the user {ev.Recipient} was sent by telegram");
        }

        private void LogFailedSending(Event? ev, string exception)
        {
            if (ev is null)
                _logger.LogError($"Failed send message to user by Telegram:\n {exception}");
            else 
                _logger.LogError($"Failed send message with id - {ev.NotificationId} to user {ev.Recipient} by Telegram:\n {exception}");
        }

        private void LogReceivedMessage(Event ev)
        {
            _logger.LogInformation($"Received message with id - {ev.NotificationId}");
        }

        #endregion
    }
}