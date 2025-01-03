using System.Text;
using System.Text.Json;
using BaseMicroservice;
using DatabaseService.Models.Rabbit;
using EmailNotifier.Configuration;
using EmailNotifier.Consumers;
using EmailNotifier.Senders;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace UnitTests.EmailNotifier;

[TestFixture]
public class EmailMessageComsumer_Tests
{
    private EmailMessagesConsumer _consumer;
    private ISender _sender;
    private IUserCredentialsService _credentialsService;

    [SetUp]
    public void Setup()
    {
        var logger = Substitute.For<ILogger<EmailMessagesConsumer>>();
        _credentialsService = Substitute.For<IUserCredentialsService>();
        _sender = Substitute.For<ISender>();
        _consumer = new EmailMessagesConsumer(_credentialsService, _sender, "testrabbit", "testqueue", logger);
    }

    [Test]
    public async Task Should_not_send_messages_when_event_is_not_deserialized()
    {
        var args = CreateBasicDeliverEventArgs(true);

        await _consumer.Handler(new object(), args);

        await _credentialsService.DidNotReceiveWithAnyArgs().GetCredentials(Arg.Any<string>(), Arg.Any<ChannelType>());
        await _sender.DidNotReceiveWithAnyArgs().SendAsync(Arg.Any<string>(), Arg.Any<Event>());
    }

    [Test]
    public async Task Should_not_send_message_when_credentials_are_null()
    {
        var args = CreateBasicDeliverEventArgs(false);
        _credentialsService.GetCredentials(Arg.Any<string>(), Arg.Any<ChannelType>()).Returns((string?)null);

        await _consumer.Handler(new object(), args);

        await _sender.DidNotReceiveWithAnyArgs().SendAsync(Arg.Any<string>(), Arg.Any<Event>());
    }

    [Test]
    public async Task Should_send_message_when_all_data_is_correct()
    {
        var args = CreateBasicDeliverEventArgs(false);

        _credentialsService.GetCredentials(Arg.Any<string>(), Arg.Any<ChannelType>()).Returns((string?)"someone");
        _sender.SendAsync(Arg.Any<string>(), Arg.Any<Event>()).Returns(SendMessageResult.Success());

        await _consumer.Handler(new object(), args);

        await _sender.Received(1).SendAsync(Arg.Any<string>(), Arg.Any<Event>());
    }

    private BasicDeliverEventArgs CreateBasicDeliverEventArgs(bool isEmpty)
    {
        var ev = isEmpty
            ? default
            : CreateRandomEvent();

        return new BasicDeliverEventArgs(
            default,
            default,
            default,
            default,
            default,
            default,
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(ev)
            ));
    }

    private Event CreateRandomEvent()
    {
        return new Event()
        {
            ChannelType = ChannelType.Email,
            CreatedAt = DateTime.Now.ToString(),
            EventType = "Email",
            MessageContent = default,
            Metadata = default,
            NotificationId = Guid.NewGuid().ToString(),
            Priority = default,
            Recipient = Guid.NewGuid().ToString()
        };
    }
}