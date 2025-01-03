using BaseMicroservice;
using EmailNotifier.Configuration;
using EmailNotifier.Consumers;

namespace EmailNotifier.Services;

public class MainService : IHostedService
{
    private BaseRabbitConsumer? _consumer;
    private readonly ILogger<MainService> _logger;

    public MainService(BaseRabbitConsumer consumer, ILogger<MainService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email Notifier started");

        await _consumer?.ExecuteAsync(cancellationToken);
        await Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email Notifier stopped");

        _consumer?.Dispose();
        return Task.CompletedTask;
    }
}