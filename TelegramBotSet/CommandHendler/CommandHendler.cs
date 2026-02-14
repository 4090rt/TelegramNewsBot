using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bots.Http;
using Telegram.Bots.Types;
using TelegramNewsBot.RequestAndParcing.ParsedBase;
using TelegramNewsBot.RequestAndParcing.RequestBse;
using TelegramNewsBot.TelegramBotSet.ModelsTg;
using static System.Net.Mime.MediaTypeNames;

namespace TelegramNewsBot.TelegramBotSet.CommandHendler
{
    // класс обработки команд
    public class CommandHendler
    {
        public readonly ITelegramBotClient _botClient;
        public readonly Dictionary<long, UserDataModel> _userSession;
        public readonly BotConfigModel _botConfig;
        public readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly RssRequests _rssRequests;
        private readonly RssRequestsReserve _rssRequestsReserve;
        private readonly ApiRequests _apiRequests;
        private readonly ParsedClass _parsedClass;
        private readonly Program _program;
        private readonly CryptoApiCourse _cryptoApiCourse;
        private readonly ValuteCourseRequest _valute;


        public CommandHendler(ITelegramBotClient botClient, IOptions<BotConfigModel> config, ILogger<CommandHendler> logger, IServiceProvider serviceProvider, RssRequestsReserve rssRequestsReserve,
        ApiRequests apiRequests,
        ParsedClass parsedClass,
        RssRequests rssRequests,
        Program program,
        CryptoApiCourse cryptoApiCourse, ValuteCourseRequest valute)
        {
            _botClient = botClient;
            _botConfig = config.Value;
            _logger = logger;
            _userSession = new Dictionary<long, UserDataModel>();
            _serviceProvider = serviceProvider;
            _rssRequests = rssRequests;
            _rssRequestsReserve = rssRequestsReserve;
            _apiRequests = apiRequests;
            _parsedClass = parsedClass;
            _logger = logger;
            _program = program;
            _cryptoApiCourse = cryptoApiCourse;
            _valute = valute;
        }

        // метод обработки команд
        public async Task HanderMessage(Telegram.Bot.Types.Message message, CancellationToken cancellation)
        {
            // из сообщения получаем данные пользователя: айди чата айди юзера и его имя
            var chatid = message.Chat.Id;
            var userId = message.From?.Id ?? 0;
            var username = message.From?.Username ?? "Anonumys";

            //Проверяем, создана ли сессия для данного пользователя - если нет то создаем

            if (!_userSession.ContainsKey(chatid))
            {
                _userSession[chatid] = new ModelsTg.UserDataModel { ChaId = chatid };
            }
            // определлили текущую сессию для использования в коде
            var session = _userSession[chatid];
            // поставили точку последний активности
            session.LastActivity = DateTime.UtcNow;

            // проверяем не пустое ли сообщение 
            if (!string.IsNullOrEmpty(message.Text))
            {
                await TextMessageAsync(message, chatid, cancellation);
                return;
            }

            //иначе
            await _botClient.SendTextMessageAsync
                (
                chatId: chatid,
                text: "Сделайте запрос новый новостей!",
                cancellationToken: cancellation
                );
        }
        // обработка текстовых комманд
        public async Task TextMessageAsync(Telegram.Bot.Types.Message message, long chaid, CancellationToken cancellation)
        {
            // убираем все лишние знаки из текста
            var messagetrimed = message.Text.Trim();
            // определлили текущую сессию для использования в коде
            var sessing = _userSession[chaid];
            var userid = message.From?.Id ?? 0;
            var username = message.From?.Username ?? "Anonumys";

            // если начинается с / - команда
            if (messagetrimed.StartsWith("/"))
            {
                // вызов команды обработки команды
                Commands.Commands commands = new(_botClient, _botConfig, _logger, _serviceProvider, _rssRequestsReserve, _apiRequests, _parsedClass, _rssRequests, _program, _cryptoApiCourse, _valute);
                await commands.FabricCommand(chaid, messagetrimed, cancellation, username);
                return;
            }

            if (messagetrimed.StartsWith("!"))
            {
                string city = messagetrimed;

                await _botClient.SendTextMessageAsync(
                     chatId: chaid,
                     text: $"✅ Город сохранен: {city}, Делаю запрос",
                     cancellationToken: cancellation);

                var result = await _program.GetTimeZoneAsync(city);

                foreach (var loc in result)
                {
                    await _botClient.SendTextMessageAsync
                         (
                       chatId: chaid,
                       text: loc.TelegramFormattedMessage,
                       parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                       cancellationToken: cancellation
                         );
                }
                return;
            }
            // иначе
            else
            {
                await _botClient.SendTextMessageAsync
                    (
                        chatId: chaid,
                        text: "Команда не распознана",
                        cancellationToken: cancellation
                    );
            }
        }
        // обработка нажатий по кнопке
        public async Task CallBackAsync(Telegram.Bot.Types.CallbackQuery callbackQuery, CancellationToken cancellation)
        { 
            // получили чат айди
            var chatid = callbackQuery.Message.Chat.Id;

            var callbackData = callbackQuery.Data;


            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellation);
            // определение команды
            switch (callbackData)
            {
                case "/MainCommands":
                    Commands.Commands com = new Commands.Commands(_botClient,_botConfig, _logger, _serviceProvider, _rssRequestsReserve, _apiRequests, _parsedClass, _rssRequests, _program, _cryptoApiCourse, _valute);
                    await com.MainCommand(chatid,cancellation);
                    break;
                case "/weather":
                    Commands.Commands com2 = new Commands.Commands(_botClient, _botConfig, _logger, _serviceProvider, _rssRequestsReserve, _apiRequests, _parsedClass, _rssRequests, _program, _cryptoApiCourse, _valute);
                    await com2.WeatherCommand(chatid,cancellation); 
                    break;
                case "/cource":
                    Commands.Commands com3 = new Commands.Commands(_botClient, _botConfig, _logger, _serviceProvider, _rssRequestsReserve, _apiRequests, _parsedClass, _rssRequests, _program, _cryptoApiCourse, _valute);
                    await com3.Cource(chatid, cancellation);
                    break;
            }
        }
    }
}
