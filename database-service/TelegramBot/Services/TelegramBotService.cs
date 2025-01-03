using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Services
{
    public class TelegramBotService : IHostedService
    {
        private TelegramBotClient _botClient;
        private CancellationTokenSource _token;
        private ILogger<TelegramBotService> _logger;

        public string ApiKey { get; }

        public TelegramBotService(string apiKey, ILogger<TelegramBotService> logger)
        {
            ApiKey = apiKey;
            _botClient = new TelegramBotClient(ApiKey);
            _token = new CancellationTokenSource();
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Main();
            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _token.Cancel();
            _token.Dispose();
            return Task.CompletedTask;
        }

        private async Task Main()
        {
            var _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                },
                DropPendingUpdates = true,
            };

            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, _token.Token);

            var me = await _botClient.GetMe();

            //await Task.Delay(10000);
        }

        private async Task UpdateHandler(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                    {
                        _logger.LogInformation($"Chat id: {update.Message.Chat.Id}");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            _logger.LogError(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}