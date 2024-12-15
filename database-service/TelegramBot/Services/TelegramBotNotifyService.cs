using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Services
{
    public class TelegramBotNotifyService
    {
        public string ApiKey { get; }
        private TelegramBotClient _botClient;
        private CancellationTokenSource _token;

        public TelegramBotNotifyService(string apiKey)
        {
            ApiKey = apiKey;
            _botClient = new TelegramBotClient(apiKey);
            _token = new CancellationTokenSource();
        }

        ~TelegramBotNotifyService() {
            _token.Cancel();
            _token.Dispose();
        }

        public async Task SendMessage(long identifier, string text)
        {
            var id = new ChatId(identifier);
            await _botClient.SendMessage(id, text);
        }
    }
}
