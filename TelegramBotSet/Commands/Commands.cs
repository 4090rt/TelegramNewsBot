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
using TelegramNewsBot.DataBase;
using TelegramNewsBot.RequestAndParcing.ModelBse;
using TelegramNewsBot.RequestAndParcing.ParsedBase;
using TelegramNewsBot.RequestAndParcing.RequestBse;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly RssRequests _rssRequests;
        private readonly RssRequestsReserve _rssRequestsReserve;
        private readonly ApiRequests _apiRequests;
        private readonly ParsedClass _parsedClass;
        private readonly Program _program;
        private readonly CryptoApiCourse _modelCrypto;
        private readonly ValuteCourseRequest _valute;
        
        public Commands(ITelegramBotClient botClient, BotConfigModel config, Microsoft.Extensions.Logging.ILogger logger, IServiceProvider serviceProvider, RssRequestsReserve rssRequestsReserve,
        ApiRequests apiRequests,
        ParsedClass parsedClass, RssRequests rssRequests, Program program, CryptoApiCourse modelcrypto, ValuteCourseRequest valute)
        {
            _botClient = botClient;
            _botConfig = config;
            _userSession = new Dictionary<long, UserDataModel>();
            _logger = logger;
            _serviceProvider = serviceProvider;
            _rssRequests = rssRequests;
            _rssRequestsReserve = rssRequestsReserve;
            _apiRequests = apiRequests;
            _parsedClass = parsedClass;
            _logger = logger;
            _program = program;
            _modelCrypto = modelcrypto;
            _valute = valute;
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
                case "/cource":
                    string log1 = "Вызов команды /Cource ";
                    string loguser1 = username;
                    DateTime date1 = DateTime.UtcNow;
                    _logger.LogInformation(log1);
                    DbSaveCommands save1 = new DbSaveCommands(_logger);
                    await save1.Addcommands(log1, loguser1, date1.ToString());

                    await Cource(chatId, cancellationToken);
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
            var resultat = await _program.GetNewsAsync();
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
                    text: "Введите название вашего города c !перед названием города",
                    cancellationToken: cancellationToken
                );
        }

        public async Task Cource(long chatid, CancellationToken cancellation = default)
        {
            try
            {
                string url = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin,ethereum,tether&vs_currencies=rub";
                string Apikey = "cf64a04e84d8235680fdfa09";

                string[] baseCurrencies = { "USD", "EUR" };
                string[] targetCurrencies = { "RUB" };
                Dictionary<string, ModelValute> results = new Dictionary<string, ModelValute>();

                foreach (string currency in baseCurrencies)
                {
                    string URL1 = $"https://v6.exchangerate-api.com/v6/{Apikey}/latest/{currency}";
                    var result  = await _valute.CachingRequest(URL1, cancellation);
                    if (result != null)
                    {
                        results[currency] = result; 
                    }
                }
                ModelCrypto result1 = await _modelCrypto.CacheRequest(url, cancellation);
                if (result1 != null && results != null)
                {
                    string message = $@"💰 *КРИПТОВАЛЮТЫ К РУБЛЮ* 💰

                    ₿ Bitcoin  ➜ {result1.bitcoin.rub:N0} ₽
                  Ξ Ethereum ➜ {result1.ethereum.rub:N0} ₽
                    ₮ Tether   ➜ {result1.tether.rub:N2} ₽

                    💱 *КУРСЫ ВАЛЮТ* 💱

                    🇺🇸 USD  ➜ {results["USD"].ConversionRates["RUB"]:F2} ₽
                    🇪🇺 EUR  ➜ {results["EUR"].ConversionRates["RUB"]:F2} ₽

                    📊 {DateTime.Now:dd.MM.yyyy HH:mm}";
                    await _botClient.SendTextMessageAsync
                      (
                          chatId: chatid,
                          text: $"💰 *КРИПТОВАЛЮТЫ К РУБЛЮ* 💰\n\n   \u20bf Bitcoin  ➜ {result1.bitcoin.rub:N0} ₽\n   Ξ Ethereum ➜ {result1.ethereum.rub:N0} ₽\n   ₮ Tether   ➜ {result1.tether.rub:N2} ₽\n \n " +
                          $"💱 *КУРСЫ ВАЛЮТ* 💱:\n   🇺🇸 USD  ➜ {results["USD"].ConversionRates["RUB"]:F2} ₽\n   🇪🇺 EUR  ➜ {results["EUR"].ConversionRates["RUB"]:F2} ₽\n \n" +
                          $"Обновлено: 📊 {DateTime.Now:dd.MM.yyyy HH:mm}",
                          cancellationToken: cancellation
                      );
                }
                else
                {
                    await _botClient.SendTextMessageAsync
                      (
                          chatId: chatid,
                          text: $"Курс валют не удалось обновить... Чиним...",
                          cancellationToken: cancellation
                      );
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return;
            }
        }
    }
}
