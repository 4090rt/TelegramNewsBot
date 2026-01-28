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
using Telegram.Bots.Types;
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

        public CommandHendler(ITelegramBotClient botClient, IOptions<BotConfigModel> config, ILogger<CommandHendler> logger)
        {
            _botClient = botClient;
            _botConfig= config.Value;
            _logger = logger;
            _userSession = new Dictionary<long, UserDataModel>();
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

            // если начинается с / - команда
            if (messagetrimed.StartsWith("/"))
            {
                // вызов команды обработки команды
                Commands.Commands commands = new Commands.Commands(_botClient,_botConfig, _logger);
                await commands.FabricCommand(chaid, messagetrimed, cancellation);
                return;
            }

            if (messagetrimed.StartsWith("!"))
            {
                string city = messagetrimed;

                await _botClient.SendTextMessageAsync(
                     chatId: chaid,
                     text: $"✅ Город сохранен: {city}, Делаю запрос",
                     cancellationToken: cancellation);

                Program prog = new Program();
                var result = await prog.aPIHTTPROGRAM(city);

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
        public async Task CallBackAsync(Telegram.Bot.Types.CallbackQuery callback, CancellationToken cancellation)
        { 
            // получили чат айди
            var chatid = callback.Message.Chat.Id;

            var callbackData = callback.Data;

            // определение команды
            switch (callbackData)
            {
                case "/MainCommands":
                    Commands.Commands com = new Commands.Commands(_botClient,_botConfig, _logger);
                    await com.MainCommand(chatid,cancellation);
                    break;
            }
        }
    }
}
