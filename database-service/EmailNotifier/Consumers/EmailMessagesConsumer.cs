using BaseMicroservice;
using EmailNotifier.Configuration;
using EmailNotifier.Senders;
using EmailNotifier.Services;
using RabbitMQ.Client.Events;

namespace EmailNotifier.Consumers;

internal class EmailMessagesConsumer : BaseRabbitConsumer
{
    private readonly ISender _sender;
    private readonly UserCredentialsService _credentialsService;
    private readonly ILogger<EmailMessagesConsumer> _logger;
    

    public EmailMessagesConsumer(
        UserCredentialsService credentialsService,
        MessageSettings messageSettings,
        string rabbitUri,
        string queueName,
        ILogger<EmailMessagesConsumer> logger)
        : base(rabbitUri, queueName)
    {
        _sender = new EmailSender(messageSettings);
        _credentialsService = credentialsService;
        _logger = logger;
    }

    public override async Task Handler(object model, BasicDeliverEventArgs args)
    {
        var ev = EventDeserializer.Deserialize(args);
        if (ev == null)
        {
            LogFailedSending(ev?.Recipient, "Event is null");
            return;
        }
        
        var credential = await _credentialsService.GetCredentials(ev.Recipient);
        if (credential == null)
        {
            LogFailedSending(ev.Recipient,"Wrong credentials format");
            return;
        }

        var sendResult = await _sender.SendAsync(credential, ev);
        if (sendResult.IsSuccess)
            LogSuccessfulSending(ev.Recipient);
        else 
            LogFailedSending(ev.Recipient, sendResult.Exception.ToString());

        await Task.CompletedTask;
    }

    #region HandleSendResult

    private void LogSuccessfulSending(string userName)
    {
        _logger.LogInformation($"Message to the user {userName} was sent by mail");
    }

    private void LogFailedSending(string userName, string exception)
    {
        _logger.LogError($"Failed send message to user {userName} by Email:\n {exception}");
    }

    #endregion
}