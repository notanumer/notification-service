using BaseMicroservice;
using EmailNotifier.Configuration;
using EmailNotifier.Senders;
using RabbitMQ.Client.Events;

namespace EmailNotifier.Consumers;

internal class EmailMessagesConsumer : BaseRabbitConsumer
{
    private readonly ISender _sender;

    public EmailMessagesConsumer(
        MessageSettings messageSettings,
        string rabbitUri,
        string queueName)
        : base(rabbitUri, queueName)
    {
        _sender = new EmailSender(messageSettings);
    }

    public override async Task Handler(object model, BasicDeliverEventArgs args)
    {
        var ev = EventDeserializer.Deserialize(args);

        if (ev == null)
        {
            HandledFailedSending(args, "Event is null");
            return;
        }

        var sendResult = await _sender.SendAsync(ev);
        if (sendResult.IsSuccess)
            HandledSuccessfulSending(args);
        else 
            HandledFailedSending(args, sendResult.Exception.ToString());

        await Task.CompletedTask;
    }

    #region HandleSendResult

    private void HandledSuccessfulSending(BasicDeliverEventArgs eventArgs)
    {
        Console.WriteLine("Sent message");
    }

    private void HandledFailedSending(BasicDeliverEventArgs eventArgs, string exception)
    {
        Console.WriteLine($"Failed to send message:\n {exception}");
    }

    #endregion
}