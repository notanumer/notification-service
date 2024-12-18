using BaseMicroservice;
using DatabaseService.Models.Rabbit;
using EmailNotifier.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace EmailNotifier.Senders;

internal interface ISender
{
    Task<SendMessageResult> SendAsync(Event eventData);

    MimeMessage CreateMessage(Event eventData);
}

internal class EmailSender : ISender
{
    private readonly MessageSettings _settings;

    public EmailSender(MessageSettings settings)
    {
        _settings = settings;
    }

    public async Task<SendMessageResult> SendAsync(Event eventData)
    {
        try
        {
            var message = CreateMessage(eventData);

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_settings.Host, _settings.Port, _settings.SecureSocketOption);
                await client.AuthenticateAsync(_settings.SmtpEmailAddress, _settings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            return SendMessageResult.Success();
        }
        catch (Exception ex)
        {
            return SendMessageResult.Fail(ex);
        }
    }

    public MimeMessage CreateMessage(Event eventData)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress("Служебное сообщение", _settings.SmtpEmailAddress));
        emailMessage.To.Add(new MailboxAddress(null, eventData.Recipient));

        if (eventData.MessageContent.Subject is not null)
            emailMessage.Subject = eventData.MessageContent.Subject;

        emailMessage.Body = new TextPart(TextFormat.Text)
        {
            Text = eventData.MessageContent.Text
        };

        return emailMessage;
    }
}