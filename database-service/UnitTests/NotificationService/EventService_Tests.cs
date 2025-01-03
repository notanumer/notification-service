using DatabaseService.DataAccess.Abstractions;
using DatabaseService.DataAccess.RabbitMq;
using DatabaseService.Models.Postgres;
using DatabaseService.Models.Rabbit;
using DatabaseService.Services;

namespace UnitTests.NotificationService;

[TestFixture]
public class EventService_Tests
{
    private EventService _eventService;
    private IRabbitMqService _rabbitMqServiceMock;
    private IAppDbContext _appDbContextMock;

    [SetUp]
    public void SetUp()
    {
        _rabbitMqServiceMock = Substitute.For<IRabbitMqService>();
        _appDbContextMock = Substitute.For<IAppDbContext>();
        _eventService = new EventService(_rabbitMqServiceMock, _appDbContextMock);
    }

    [Test]
    public async Task Should_save_notifivation_with_IsSuccess_that_depends_on_saving_to_rabbit([Values]bool isSuccess)
    {
        var notificationEvent = new Event
        {
            NotificationId = Guid.NewGuid().ToString(),
            ChannelType = ChannelType.Email,
            CreatedAt = DateTime.Now.ToString(),
            MessageContent = new MessageContent { Text = "Test Message" },
            EventType = "Email",
            Recipient = "test@example.com"
        };
        
        _rabbitMqServiceMock.SendMessageAsync(notificationEvent, Arg.Any<CancellationToken>()).Returns(isSuccess);

        await _eventService.SendEventAsync(notificationEvent);

        await _rabbitMqServiceMock.Received(1).SendMessageAsync(notificationEvent, Arg.Any<CancellationToken>());
        await _appDbContextMock.Notifications.Received(1).AddAsync(
            Arg.Is<Notification>(n =>
                n.MessageContent == notificationEvent.MessageContent.Text &&
                n.MessageType == notificationEvent.EventType &&
                n.Recipient == notificationEvent.Recipient &&
                n.IsSuccess == isSuccess),
            Arg.Any<CancellationToken>());
    }
}