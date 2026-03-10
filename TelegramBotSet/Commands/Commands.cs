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
        private readonly RequestFromStream _requestFromStream;
        private readonly LogCommand _logcommand;
        private readonly DelegateFromBD _delegateFromBD;
        private readonly LINQ _linq;
        private readonly ExceptionClass _exceptionClass;

        public Commands(ITelegramBotClient botClient, BotConfigModel config, Microsoft.Extensions.Logging.ILogger logger, IServiceProvider serviceProvider, RssRequestsReserve rssRequestsReserve,
        ApiRequests apiRequests,
        ParsedClass parsedClass, RssRequests rssRequests, Program program, CryptoApiCourse modelcrypto, ValuteCourseRequest valute, RequestFromStream requestFromStream, LogCommand logcommand,
        DelegateFromBD delegateFromBD, LINQ linq, ExceptionClass exceptionClass)
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
            _program = program;
            _modelCrypto = modelcrypto;
            _valute = valute;
            _requestFromStream = requestFromStream;
            _logcommand = logcommand;
            _delegateFromBD = delegateFromBD;
            _linq = linq;
            _exceptionClass = exceptionClass;
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
                    _logcommand.LogginginBd += (sender,args) => _logcommand.SaveIndBd(log3, loguser3, date3).Wait();

                    await MainCommand(chatId, cancellationToken);
                    break;

                case "/start":
                    string log = "Вызов команды /Start ";
                    string loguser = username;
                    DateTime date = DateTime.UtcNow;
                    _logger.LogInformation(log);
                    _logcommand.LogginginBd += (sender, args) => _logcommand.SaveIndBd(log, loguser, date).Wait();

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
                    _logcommand.LogginginBd += (sender, args) => _logcommand.SaveIndBd(log2, loguser2, date2).Wait();

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
                    _logcommand.LogginginBd += (sender, args) => _logcommand.SaveIndBd(log1, loguser1, date1).Wait();

                    await Cource(chatId, cancellationToken);
                    break;
                case "/StartMonitor":
                    string log5 = "Вызов команды /StartMonitor";
                    string loguser5 = username;
                    DateTime date5 = DateTime.UtcNow;
                    _logger.LogInformation(log5);
                    _logcommand.LogginginBd += (sender, args) => _logcommand.SaveIndBd(log5, loguser5, date5).Wait();

                    await StartMonitor(chatId);
                    break;
                case "/StopMonitor":
                    string log6 = "Вызов команды /StopMonitor";
                    string loguser6 = username;
                    DateTime date6 = DateTime.UtcNow;
                    _logger.LogInformation(log6);
                    _logcommand.LogginginBd += (sender, args) => _logcommand.SaveIndBd(log6, loguser6, date6).Wait();

                    await StopMonitor(chatId);
                    break;
                case "/Statistic":
                    string log7 = "Вызов команды /StopMonitor";
                    string loguser7 = username;
                    DateTime date7 = DateTime.UtcNow;
                    _logger.LogInformation(log7);
                    _logcommand.LogginginBd += (sender, args) => _logcommand.SaveIndBd(log7, loguser7, date7).Wait();

                    LinqFilterCommands linqFilterCommands = new LinqFilterCommands(_logger, _delegateFromBD, _linq, _exceptionClass);
                    await linqFilterCommands.Command1();
                    await linqFilterCommands.CommandFroCommands("/Start");
                    await linqFilterCommands.CommandFro5Popular();
                    await linqFilterCommands.CommandFromWeekLast();
                    await linqFilterCommands.CommandFromLast10();
                    await linqFilterCommands.CommandfROMLastAndFirst();
                    await linqFilterCommands.CommandsFromUser(loguser7);
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
                    var result = await _valute.CachingRequest(URL1, cancellation);
                    if (result != null)
                    {
                        results[currency] = result;
                    }
                }
                var resultCryptoList = await _modelCrypto.CacheRequest(url, cancellation);
                if (resultCryptoList != null && resultCryptoList.Count > 0 && results != null)
                {
                    var result1 = resultCryptoList[0];
                    var usdRate = results["USD"].GetRate("RUB");
                    var eurRate = results["EUR"].GetRate("RUB");

                    var btcRate = result1.GetRate("bitcoin");
                    var ethRate = result1.GetRate("ethereum");
                    var usdtRate = result1.GetRate("tether");

                    string message = $@"💰 *КРИПТОВАЛЮТЫ К РУБЛЮ* 💰

                    ₿ Bitcoin  ➜ {btcRate:N0} ₽
                  Ξ Ethereum ➜ {ethRate:N0} ₽
                    ₮ Tether   ➜ {usdtRate:N2} ₽

                    💱 *КУРСЫ ВАЛЮТ* 💱

                    🇺🇸 USD  ➜ {usdRate:F2} ₽
                    🇪🇺 EUR  ➜ {eurRate:F2} ₽

                    📊 {DateTime.Now:dd.MM.yyyy HH:mm}";
                    await _botClient.SendTextMessageAsync
                      (
                          chatId: chatid,
                          text: $"💰 *КРИПТОВАЛЮТЫ К РУБЛЮ* 💰\n\n   \u20bf Bitcoin  ➜ {btcRate:N0} ₽\n   Ξ Ethereum ➜ {ethRate:N0} ₽\n   ₮ Tether   ➜ {usdtRate:N2} ₽\n \n " +
                          $"💱 *КУРСЫ ВАЛЮТ* 💱:\n   🇺🇸 USD  ➜ {usdRate:F2} ₽\n   🇪🇺 EUR  ➜ {eurRate:F2} ₽\n \n" +
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

        public static class Monitoring
        {
            public static Dictionary<long, CancellationTokenSource> _tokens = new();
        }

        public async Task StartMonitor(long chatid)
        {
            var cts = new CancellationTokenSource();
            Monitoring._tokens[chatid] = cts;


            _logger.LogInformation("Сервис мониторинга запущен!");

            await _botClient.SendTextMessageAsync(chatid, "✅ Мониторинг запущен!");

            _ = Task.Run(async () =>
            {
                await _requestFromStream.Request(
                    "https://tass.com/rss/v2.xml",
                    async (result) => 
                    {
                        foreach (var news in result)
                        {
                            if (cts.Token.IsCancellationRequested)
                            {
                                _logger.LogInformation("Отмена отправки новостей");
                                return;
                            }
                            await _botClient.SendTextMessageAsync(chatid,
                                $"📰 *{news.Title}*\n🔗 {news.Link}\n⏰ {news.PublisDate}\n\n");
                        }

                        _logger.LogInformation($"Отправлено {result.Count} новостей");
                    },
                    cts.Token
                );
            });
        }

        public async Task StopMonitor(long chatid)
        {

            if (Monitoring._tokens.TryGetValue(chatid, out var cts))
            {
                cts.Cancel(); 
                Monitoring._tokens.Remove(chatid); 
                _logger.LogInformation("Сервис мониторинга остановлен!");
                await _botClient.SendTextMessageAsync(chatid, "⏹️ Мониторинг остановлен");
            }
        }

    }
}
