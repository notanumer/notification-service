using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using BaseMicroservice;
using DatabaseService.Models.Rabbit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using TelegramNotifier.Consumers;

namespace UnitTests.TelegramNotifier;

public class TelegramMessageConsumer_Tests
{
    private TelegramMessagesConsumer _consumer;
    private IUserCredentialsService _credentialsService;
    private ILogger<TelegramMessagesConsumer> _logger;
    private MockHttpMessageHandler _handler;
    private string _baseUrl;

    [SetUp]
    public void Setup()
    {
        _baseUrl = "http://test-telegram-bot-service.com";
        _logger = Substitute.For<ILogger<TelegramMessagesConsumer>>();
        _credentialsService = Substitute.For<IUserCredentialsService>();

        _handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(_handler)
        {
            BaseAddress = new Uri(_baseUrl)
        };

        _consumer = new TelegramMessagesConsumer(
            _credentialsService,
            _baseUrl,
            "testrabbit",
            "testqueue",
            _logger
        );

        ReplaceHttpClient(_consumer, httpClient);
    }

    [Test]
    public async Task Should_not_send_messages_when_event_is_not_deserialized()
    {
        var args = CreateBasicDeliverEventArgs(true);

        await _consumer.Handler(new object(), args);

        await _credentialsService.DidNotReceiveWithAnyArgs().GetCredentials(Arg.Any<string>(), Arg.Any<ChannelType>());
        _handler.Requests.Should().HaveCount(0);
    }

    [Test]
    public async Task Should_not_send_message_when_credentials_are_null()
    {
        var args = CreateBasicDeliverEventArgs(false);
        _credentialsService.GetCredentials(Arg.Any<string>(), ChannelType.Telegram).Returns((string?)null);

        await _consumer.Handler(new object(), args);

        await _credentialsService.Received(1).GetCredentials(Arg.Any<string>(), ChannelType.Telegram);
        _handler.Requests.Should().HaveCount(0);
    }

    [Test]
    public async Task Should_send_message_when_all_data_is_correct()
    {
        var args = CreateBasicDeliverEventArgs(false);

        _credentialsService.GetCredentials(Arg.Any<string>(), ChannelType.Telegram).Returns("123456789");

        await _consumer.Handler(new object(), args);

        await _credentialsService.Received(1).GetCredentials(Arg.Any<string>(), ChannelType.Telegram);
        _handler.Requests.Should().HaveCount(1);
        _handler.Requests.First().RequestUri.ToString().Should()
            .Be("http://test-telegram-bot-service.com/bot/send");
    }

    private BasicDeliverEventArgs CreateBasicDeliverEventArgs(bool isEmpty)
    {
        var ev = isEmpty
            ? null
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
        return new Event
        {
            ChannelType = ChannelType.Telegram,
            CreatedAt = DateTime.UtcNow.ToString("O"),
            EventType = "Telegram",
            MessageContent = new MessageContent { Text = "Test Message" },
            Metadata = null,
            NotificationId = Guid.NewGuid().ToString(),
            Priority = "High",
            Recipient = "recipient-id"
        };
    }

    private void ReplaceHttpClient(TelegramMessagesConsumer consumer, HttpClient newHttpClient)
    {
        var httpClientField = typeof(TelegramMessagesConsumer)
            .GetField("client", BindingFlags.NonPublic | BindingFlags.Instance);

        if (httpClientField == null)
            throw new InvalidOperationException("Field 'client' not found in TelegramMessagesConsumer.");

        httpClientField.SetValue(consumer, newHttpClient);
    }
}