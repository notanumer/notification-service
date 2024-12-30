using BaseMicroservice;
using DatabaseService.Models.Rabbit;
using DatabaseService.Services.Abstractions;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TelegramBot.Services;

namespace TelegramNotifier.Consumers
{
    public class TelegramMessagesConsumer : BaseRabbitConsumer
    {
        HttpClient client;
        UserCredentialsService _credentialsService;

        public TelegramMessagesConsumer(
            UserCredentialsService credentialsService,
            string telegramBotServiceAddress, 
            string rabbitUri, 
            string queueName
        ) : base(rabbitUri, queueName) {
            client = new HttpClient() { BaseAddress = new Uri(telegramBotServiceAddress)};
            _credentialsService = credentialsService;
        }

        public override async Task Handler(object model, BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var ev = JsonSerializer.Deserialize<Event>(message);

            var credential = await _credentialsService.GetCredentials(ev.Recipient);
            if ( credential == null )
            {
                Console.WriteLine("Wrong credentials format");
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine($"[x] Received message with id {ev.NotificationId}");
            await Task.CompletedTask;
        }
    }
}