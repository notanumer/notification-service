using BaseMicroservice;

namespace TelegramNotifier.Services
{
    public class MainService : IHostedService
    {
        private BaseRabbitConsumer? _consumer;
        private ILogger<MainService> _logger;

        public MainService(BaseRabbitConsumer consumer, ILogger<MainService> logger)
        {
            _consumer = consumer;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Telegram notifier started");

            await _consumer?.ExecuteAsync(cancellationToken);
            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Telegram Notifier stopped");

            _consumer?.Dispose();
            return Task.CompletedTask;
        }
    }
}