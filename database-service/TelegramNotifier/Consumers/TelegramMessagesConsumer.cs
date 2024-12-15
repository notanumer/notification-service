using BaseMicroservice;
using DatabaseService.Models.Rabbit;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace TelegramNotifier.Consumers
{
    public class TelegramMessagesConsumer : BaseRabbitConsumer
    {
        HttpClient client;
        public TelegramMessagesConsumer(string telegramBotServiceAddress, string rabbitUri, string queueName) : base(rabbitUri, queueName) {
            client = new HttpClient() { BaseAddress = new Uri(telegramBotServiceAddress)};
        }

        public override async Task Handler(object model, BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var ev = JsonSerializer.Deserialize<Event>(message);
            JsonContent content = JsonContent.Create(new
            {
                UserId = long.Parse(ev.Recipient),
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