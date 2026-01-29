using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bots.Types;
using TelegramNewsBot.DataBase;
using TelegramNewsBot.RequestAndParcing.ModelBse;
using TelegramNewsBot.TelegramBotSet.InkineButtons;
using TelegramNewsBot.TelegramBotSet.ModelsTg;

namespace TelegramNewsBot.TelegramBotSet.Commands
{
    public class Commands
    {
        private readonly ITelegramBotClient _botClient;
        private readonly BotConfigModel _botConfig;
        private readonly Dictionary<long, UserDataModel> _userSession;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public Commands(ITelegramBotClient botClient, BotConfigModel config, Microsoft.Extensions.Logging.ILogger logger)
        {
            _botClient = botClient;
            _botConfig = config;
            _userSession = new Dictionary<long, UserDataModel>();
            _logger = logger;
        }

        public async Task FabricCommand(long chatId, string command, CancellationToken cancellationToken, string username)
        {
            switch (command)
            {
                case "/MainCommands":
                    string log3 = "Вызов команды /MainCommands ";
                    string loguser3 = username;
                    DateTime date3 = DateTime.UtcNow;
                    _logger.LogInformation(log3);
                    DbSaveCommands save3 = new DbSaveCommands(_logger);
                    await save3.Addcommands(log3, loguser3, date3.ToString());

                    await MainCommand(chatId, cancellationToken);
                    break;

                case "/start":
                    string log = "Вызов команды /Start ";
                    string loguser = username;
                    DateTime date = DateTime.UtcNow;
                    _logger.LogInformation(log);
                    DbSaveCommands save = new DbSaveCommands(_logger);
                    await save.Addcommands(log, loguser, date.ToString());


                    InlineButtons inl = new InlineButtons(_botClient);
                    await inl.InlineButtonss(chatId, cancellationToken);
                    break;

                case "/weather":
                    DbPathClass clas = new DbPathClass();
                    string path = clas.dbpath();
                    string log2 = "Вызов команды /Weather";
                    string loguser2 = username;
                    DateTime date2 = DateTime.UtcNow;
                    _logger.LogInformation(log2);
                    DbSaveCommands save2 = new DbSaveCommands(_logger);
                    await save2.Addcommands(log2,loguser2,date2.ToString());

                    await WeatherCommand(chatId, cancellationToken);
                    break;
                case "/UserActivity":
                    string usernames = "lilchicfgt";
                    UserSearchCommand command1 = new UserSearchCommand(_logger);
                    await command1.Command(usernames);
                    await _botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Выполняю",
                        cancellationToken: cancellationToken
                        );
                    break;
                case "/LastCommand":
                    DateLast datel = new DateLast(_logger);
                    await datel.LastDateSeqscrh();
                    await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Выполняю",
                    cancellationToken: cancellationToken
                    );
                    break;
            }
        }

        public async Task MainCommand(long chatId,CancellationToken cancellationToken)
        {
            await _botClient.SendTextMessageAsync
                (
                    chatId: chatId,
                    text: "Ищем самые последние новости!",
                    cancellationToken: cancellationToken
                );

            Program prog = new Program();
            var resultat = await prog.Httprogram();
            if (resultat != null)
            {
                var messagecount = 1;
                var newstextbuilder = new StringBuilder();
                const int maxsize = 4000;

                foreach (var news in resultat)
                {
                    var newsText = $"📰 *{news.Title}*\n🔗 {news.Link}\n⏰ {news.PublisDate}\n\n";

                    if (newstextbuilder.Length + newsText.Length > maxsize)
                    {
                        await _botClient.SendTextMessageAsync
                            (
                                chatId: chatId,
                                text: newstextbuilder.ToString(),
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );

                        newstextbuilder.Clear();
                        newstextbuilder.AppendLine($"*Часть {messagecount}*\n");
                        messagecount++;

                        await Task.Delay(1000, cancellationToken);
                    }
                    newstextbuilder.Append(newsText);
                }

                if (newstextbuilder.Length > 0)
                {
                    await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: newstextbuilder.ToString(),
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: cancellationToken);
                }
            }
            else
            {
                Console.WriteLine("Результат пустой");
                return;
            }
        }

        public async Task WeatherCommand(long chatId, CancellationToken cancellationToken)
        {
            await _botClient.SendTextMessageAsync
                (
                    chatId: chatId,
                    text: "Введите название вашего города c !перд названием города",
                    cancellationToken: cancellationToken
                );


        }
    }
}
