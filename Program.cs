// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Caching;
using System.Reflection;
using System.Runtime;
using System.Security.Cryptography;
using Telegram.Bot;
using Telegram.Bots.Configs;
using TelegramNewsBot.RequestAndParcing.ModelBse;
using TelegramNewsBot.RequestAndParcing.ParsedBase;
using TelegramNewsBot.RequestAndParcing.RequestBse;
using TelegramNewsBot.TelegramBotSet.CommandHendler;
using TelegramNewsBot.TelegramBotSet.InkineButtons;
using TelegramNewsBot.TelegramBotSet.ModelsTg;
using TelegramNewsBot.TelegramBotSet.TelegramService;

class Program
{
    //Основной метод
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Запуск Telegram Converter Bot...");

        try

        {
            var host = CreateHostBuilder(args).Build();
            Console.WriteLine("✅ Конфигурация загружена");
            Console.WriteLine("✅ Сервисы зарегистрированы");
            Console.WriteLine("✅ Запускаем хост...");
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Критическая ошибка: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).
        ConfigureAppConfiguration((context, config) => // Создаем хост с аргументами командной строки
        {
            // настройка конфигурации
            //config.SetBasePath(Directory.GetCurrentDirectory());// базовая папка
            /*  config.AddJsonFile("jsconfig1.json", optional: false, reloadOnChange: true);*/// берем файл json если нет falseoptional: false
                                                                                              //reloadOnChange: если измнаенился во время работы - перезапускаем конфигурацию

            // добавление переменных окружения
            config.AddEnvironmentVariables();


            //Добавляем аргументы командной строки
            if (args != null)
            {
                config.AddCommandLine(args);
            }
        })
        .ConfigureServices((context, services) =>
        {
            // С помощью GetSElection выбираем токен из переменных окружения помещаем в бот конфиг
            services.Configure<BotConfigModel>(
                context.Configuration.GetSection("TelegramBotNews"));

            //Регистрируем бота как синглтон
            services.AddSingleton<ITelegramBotClient>(sp =>
            {
                var token = "";
                Console.WriteLine($"✅ Создаю TelegramBotClient...");

                // Проверяем токен здесь же
                var client = new TelegramBotClient(token);

                // Сразу тестируем
                try
                {
                    var me = client.GetMeAsync().GetAwaiter().GetResult();
                    Console.WriteLine($"✅ TelegramBotClient создан успешно! Бот: @{me.Username}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка при создании клиента: {ex.Message}");
                }

                return client;
            }); ;

            // Регистрируем обработчик команд
            services.AddSingleton<CommandHendler>();
            //Регистрируем фоновую службу, AddHostedService добавляем  - он служит как связь между тг апи и ботом
            //// и передает команды из тг в Main класс где обрабатываются файлы и команды
            services.AddHostedService<TelegramService>();

            // Регистрируем словарь для сессий пользователей
            services.AddSingleton<Dictionary<long, UserDataModel>>();

            Console.WriteLine("✅ Сервисы сконфигурированы");
        })
         .ConfigureLogging((context, logging) =>
         {

             logging.ClearProviders();
             logging.AddConfiguration(context.Configuration.GetSection("Logging"));

             logging.AddConsole();
             logging.AddDebug();

             Console.WriteLine("✅ Логирование настроено");

         })
        .UseConsoleLifetime();

    public async Task<List<ModelClassRss>> Httprogram()
    {
        var service = new ServiceCollection();
        service.AddLogging(building =>
        {
            building.AddConsole();
            building.SetMinimumLevel(LogLevel.Information);
        });

        //настроенный клиент под RSS
        service.AddHttpClient("RssCLient", rssclient =>
        {
            rssclient.Timeout = TimeSpan.FromSeconds(60);
            rssclient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            rssclient.DefaultRequestHeaders.Accept.ParseAdd("application/xml, text/xml, */*");
            rssclient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            rssclient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
        }).AddTransientHttpErrorPolicy(poly =>
        poly.WaitAndRetryAsync(3, retry =>
        TimeSpan.FromSeconds(Math.Pow(2, retry))));


        // настроенный клиент под Api
        service.AddHttpClient("ApiClient", apiclient =>
        {
            apiclient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            apiclient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            apiclient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
        }).AddTransientHttpErrorPolicy(poly =>
        poly.WaitAndRetryAsync(3, retry =>
        TimeSpan.FromSeconds(Math.Pow(2, retry))));

        service.AddHttpClient("RssClientReserve", rssclientreserve =>
        {
            rssclientreserve.Timeout = TimeSpan.FromSeconds(60);
            rssclientreserve.DefaultRequestHeaders.Accept.ParseAdd("application/xml, text/xml, */*");
            rssclientreserve.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            rssclientreserve.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }).AddTransientHttpErrorPolicy(polly =>
        polly.WaitAndRetryAsync(3, retry =>
        TimeSpan.FromSeconds(Math.Pow(2, retry))));

        //Добавление в DI
        service.AddScoped<RssRequests>();
        service.AddScoped<ParsedClass>();
        service.AddScoped<ApiRequests>();
        service.AddScoped<RssRequestsReserve>();
        //Построение
        var serviceProvider = service.BuildServiceProvider();


        //URL
        string url = "https://tass.com/rss/v2.xml";
        string url3 = "https://www.interfax.ru/rss.asp";
        //string url2 = "https://ipinfo.io/json";

        try
        {
            //Берем класс из DI и делаем запрос
            using var scope = serviceProvider.CreateScope();
            var main = scope.ServiceProvider.GetRequiredService<RssRequests>();
            Stream result = await main.RssRequestsMethod(url);


            // парсим результат запроса
            using var scope1 = serviceProvider.CreateScope();
            var Reflect = scope1.ServiceProvider.GetRequiredService<ParsedClass>();
            var resultt = await Reflect.ParseRss(result);

            // выводим в консоль 
            if (resultt != null)
            {
                foreach (var item in resultt)
                {
                    Console.WriteLine($"Title: {item.Title}");
                    Console.WriteLine($"Link: {item.Link}");
                    //Console.WriteLine($"Description: {item.Description}");
                    Console.WriteLine($"PublishDate: {item.PublisDate}");
                    Console.WriteLine($"ID: {item.ID}");
                    Console.WriteLine(new string('-', 40));
                }
                return resultt;
            }
            else
            {
                Console.WriteLine("Результат пустой");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Возникло исключение" + ex.Message + "Перехожу на резервный источник");
            using var scope = serviceProvider.CreateScope();
            var reserve = scope.ServiceProvider.GetRequiredService<RssRequestsReserve>();
            var resultats = await reserve.RssRequestRes(url3);

            using var scope1 = serviceProvider.CreateScope();
            var Reflect = scope1.ServiceProvider.GetRequiredService<ParsedClass>();
            var resultt = await Reflect.ParseRssReserve(resultats);

            if (resultt != null)
            {
                foreach (var item in resultt)
                {
                    Console.WriteLine($"Title: {item.Title}");
                    Console.WriteLine($"Link: {item.Link}");
                    //Console.WriteLine($"Description: {item.Description}");
                    Console.WriteLine($"PublishDate: {item.PublisDate}");
                    Console.WriteLine($"ID: {item.ID}");
                    Console.WriteLine(new string('-', 40));
                }
                return resultt;
            }
            else
            {
                Console.WriteLine("Результат пустой");
                return null;
            }
        }
    }
}

