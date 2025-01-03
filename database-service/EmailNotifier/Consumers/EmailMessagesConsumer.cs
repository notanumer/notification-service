using BaseMicroservice;
using DatabaseService.Models.Rabbit;
using EmailNotifier.Configuration;
using EmailNotifier.Senders;
using EmailNotifier.Services;
using RabbitMQ.Client.Events;

namespace EmailNotifier.Consumers;

public class EmailMessagesConsumer : BaseRabbitConsumer
{
    private readonly ISender _sender;
    private readonly IUserCredentialsService _credentialsService;
    private readonly ILogger<EmailMessagesConsumer> _logger;


    public EmailMessagesConsumer(
        IUserCredentialsService credentialsService,
        ISender sender,
        string rabbitUri,
        string queueName,
        ILogger<EmailMessagesConsumer> logger)
        : base(rabbitUri, queueName)
    {
        _sender = sender;
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

        var credential = await _credentialsService.GetCredentials(ev.Recipient, ChannelType.Email);
        if (credential == null)
        {
            LogFailedSending(ev, "Wrong credentials format");
            return;
        }

        var sendResult = await _sender.SendAsync(credential, ev);
        if (sendResult.IsSuccess)
            LogSuccessfulSending(ev);
        else
            LogFailedSending(ev, sendResult.Exception.ToString());

        LogReceivedMessage(ev);
        await Task.CompletedTask;
    }

    #region HandleSendResult

    private void LogSuccessfulSending(Event ev)
    {
        _logger.LogInformation($"Message with id - {ev.NotificationId} to the user {ev.Recipient} was sent by mail");
    }

    private void LogFailedSending(Event? ev, string exception)
    {
        if (ev is null)
            _logger.LogError($"Failed send message to user by Email:\n {exception}");
        else
            _logger.LogError(
                $"Failed send message with id - {ev.NotificationId} to user {ev.Recipient} by Email:\n {exception}");
    }

    private void LogReceivedMessage(Event ev)
    {
        _logger.LogInformation($"Received message with id - {ev.NotificationId}");
    }

    #endregion
}