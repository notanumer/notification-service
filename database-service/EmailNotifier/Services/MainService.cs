using EmailNotifier.Configuration;
using EmailNotifier.Consumers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace EmailService.Services;

internal class MainService : IHostedService
{
    EmailMessagesConsumer? consumer;

    public MainService(MessageSettings messageSettings, string rabbitUri, string queueName)
    {
        consumer = new EmailMessagesConsumer(messageSettings, rabbitUri, queueName);
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