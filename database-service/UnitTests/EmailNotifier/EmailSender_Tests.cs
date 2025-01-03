using DatabaseService.Models.Rabbit;
using EmailNotifier.Configuration;
using EmailNotifier.Senders;
using FluentAssertions;
using MimeKit;

namespace UnitTests.EmailNotifier;

[TestFixture]
public class EmailSender_Tests
{
    private EmailSender _emailSender;
    private MessageSettings _messageSettings;

    [OneTimeSetUp]
    public void Setup()
    {
        _messageSettings = new MessageSettings
        {
            SmtpEmailAddress = "no-reply@example.com"
        };
        _emailSender  = new EmailSender(_messageSettings);
    }
    
    [Test]
    public void Should_create_message_with_minial_data()
    {
        var userAddress = "recipient@example.com";
        var eventData = new Event
        {
            NotificationId = default,
            ChannelType = ChannelType.Email,
            EventType = "Email",
            Recipient = "scdsdcsdsdc",
            CreatedAt = DateTime.Now.ToString(),
            MessageContent = new MessageContent
            {
                Text = "Test message content"
            }
        };

        var message = _emailSender.CreateMessage(userAddress, eventData);

        message.To.Mailboxes.Single().Address.Should().Be(userAddress);
        message.From.Mailboxes.Single().Address.Should().Be(_messageSettings.SmtpEmailAddress);
        message.Subject.Should().BeEmpty();
        ((TextPart)message.Body).Text.Should().Be(eventData.MessageContent.Text);
    }

    [Test]
    public void Should_create_message_with_subject()
    {
        var eventData = new Event
        {
            NotificationId = default,
            ChannelType = ChannelType.Email,
            EventType = "Email",
            Recipient = "scdsdcsdsdc",
            CreatedAt = DateTime.Now.ToString(),
            MessageContent = new MessageContent
            {
                Subject = "Test Subject",
                Text = "Test message content"
            }
        };

        var message = _emailSender.CreateMessage("recipient@example.com", eventData);

        message.Subject.Should().Be(eventData.MessageContent.Subject);
    }
}