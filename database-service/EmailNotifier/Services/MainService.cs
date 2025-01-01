using EmailNotifier.Configuration;
using EmailNotifier.Consumers;

namespace EmailNotifier.Services;

internal class MainService : IHostedService
{
    EmailMessagesConsumer? consumer;

    public MainService(
        UserCredentialsService credentialsService, 
        MessageSettings messageSettings, 
        string rabbitUri, 
        string queueName,
        IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger<EmailMessagesConsumer>>();
        consumer = new EmailMessagesConsumer(credentialsService, messageSettings, rabbitUri, queueName, logger);
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