namespace TelegramBot.Models
{
    public class Payload
    {
        public long UserId { get; set; }
        public required string Text { get; set; }
    }
}
