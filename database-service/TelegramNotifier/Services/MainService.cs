using TelegramNotifier.Consumers;

namespace TelegramNotifier.Services
{
    public class MainService : IHostedService
    {
        TelegramMessagesConsumer? consumer;

        public MainService(string telegramBotServiceAddress, string rabbitUri, string queueName)
        {
            consumer = new TelegramMessagesConsumer(telegramBotServiceAddress, rabbitUri, queueName);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await consumer?.ExecuteAsync(cancellationToken);
            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            consumer?.Dispose();
            return Task.CompletedTask;
        }
    }
}