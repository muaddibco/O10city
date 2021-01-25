using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using O10.Client.Web.Common;
using O10.Client.Web.Common.Configuration;
using O10.Core.Architecture;

using O10.Core.Configuration;
using O10.Core.Logging;
using O10.Client.Web.Portal.Telegram.Configuration;

namespace O10.Client.Web.Portal.Telegram.Bots
{
    [RegisterDefaultImplementation(typeof(IO10BotService), Lifetime = LifetimeManagement.Singleton)]
    public class O10BotService : IO10BotService
    {
        private TelegramBotClient _bot;
        private readonly ITelegramConfiguration _telegramConfiguration;
        private readonly IAzureConfiguration _azureConfiguration;
        private readonly ILogger _logger;

        private CancellationToken _cancellationToken;

        public O10BotService(IConfigurationService configurationService, ILoggerService loggerService)
        {
            _telegramConfiguration = configurationService.Get<ITelegramConfiguration>();
            _azureConfiguration = configurationService.Get<IAzureConfiguration>();
            _logger = loggerService.GetLogger(nameof(O10BotService));
        }

        public async Task Initialize(CancellationToken ct)
        {
            _cancellationToken = ct;
            _cancellationToken.Register(() =>
            {
                _bot?.StopReceiving();
            });

            string apiKey = AzureHelper.GetSecretValue(_telegramConfiguration.ApiKeyName, _azureConfiguration.AzureADCertThumbprint, _azureConfiguration.AzureADApplicationId, _azureConfiguration.KeyVaultName);
            _bot = new TelegramBotClient(apiKey);
            var me = await _bot.GetMeAsync(ct).ConfigureAwait(false);
            _logger.Info($"Starting Telegram Bot Service with username '{me.Username}'");

            _bot.OnMessage += BotOnMessageReceived;
            _bot.OnMessageEdited += BotOnMessageReceived;
            _bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            _bot.OnInlineQuery += BotOnInlineQueryReceived;
            _bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            _bot.OnReceiveError += BotOnReceiveError;
        }

        public void Start()
        {
            _bot.StartReceiving(Array.Empty<UpdateType>());
        }

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            switch (message.Text.Split(' ')[0])
            {
                // Send inline keyboard
                case "/inline":
                    await SendInlineKeyboard(message);
                    break;

                // send custom keyboard
                case "/keyboard":
                    await SendReplyKeyboard(message);
                    break;

                // send a photo
                case "/photo":
                    await SendDocument(message);
                    break;

                // request location or contact
                case "/request":
                    await RequestContactAndLocation(message);
                    break;

                default:
                    await Usage(message);
                    break;
            }

            // Send inline keyboard
            // You can process responses in BotOnCallbackQueryReceived handler
            async Task SendInlineKeyboard(Message message)
            {
                await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                // Simulate longer running task
                await Task.Delay(500);

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", "11"),
                        InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    },
                    // second row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    }
                });
                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Choose",
                    replyMarkup: inlineKeyboard
                );
            }

            async Task SendReplyKeyboard(Message message)
            {
                var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                    new KeyboardButton[][]
                    {
                        new KeyboardButton[] { "1.1", "1.2" },
                        new KeyboardButton[] { "2.1", "2.2" },
                    },
                    resizeKeyboard: true
                );

                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Choose",
                    replyMarkup: replyKeyboardMarkup

                );
            }

            async Task SendDocument(Message message)
            {
                await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                const string filePath = @"Files/tux.png";
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();
                await _bot.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: new InputOnlineFile(fileStream, fileName),
                    caption: "Nice Picture"
                );
            }

            async Task RequestContactAndLocation(Message message)
            {
                var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact"),
                });
                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Who or Where are you?",
                    replyMarkup: RequestReplyKeyboard
                );
            }

            async Task Usage(Message message)
            {
                const string usage = "Usage:\n" +
                                        "/inline   - send inline keyboard\n" +
                                        "/keyboard - send custom keyboard\n" +
                                        "/photo    - send a photo\n" +
                                        "/request  - request location or contact";
                await _bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: usage,
                    replyMarkup: new ReplyKeyboardRemove()
                );
            }
        }

        // Process Inline Keyboard callback data
        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            await _bot.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Received {callbackQuery.Data}"
            );

            await _bot.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Received {callbackQuery.Data}"
            );
        }

        #region Inline Mode

        private async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            Console.WriteLine($"Received inline query from: {inlineQueryEventArgs.InlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent(
                        "hello"
                    )
                )
            };
            await _bot.AnswerInlineQueryAsync(
                inlineQueryId: inlineQueryEventArgs.InlineQuery.Id,
                results: results,
                isPersonal: true,
                cacheTime: 0
            );
        }

        private void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        #endregion

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message
            );
        }
    }
}
