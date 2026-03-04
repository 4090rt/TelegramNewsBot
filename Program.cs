// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Caching;
using System;
using System.Net;
using System.Reflection;
using System.Runtime;
using System.Security.Cryptography;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bots.Configs;
using TelegramNewsBot;
using TelegramNewsBot.RequestAndParcing.ModelBse;
using TelegramNewsBot.RequestAndParcing.ParsedBase;
using TelegramNewsBot.RequestAndParcing.RequestBse;
using TelegramNewsBot.TelegramBotSet.CommandHendler;
using TelegramNewsBot.TelegramBotSet.InkineButtons;
using TelegramNewsBot.TelegramBotSet.ModelsTg;
using TelegramNewsBot.TelegramBotSet.TelegramService;

public class Program
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RssRequests _rssRequests;
    private readonly RssRequestsReserve _rssRequestsReserve;
    private readonly ApiRequests _apiRequests;
    private readonly ParsedClass _parsedClass;
    private readonly ILogger<Program> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly ClientOptions _clientOptions;

    public Program(
        RssRequests rssRequests,
        IServiceProvider serviceProvider,
        RssRequestsReserve rssRequestsReserve,
        ApiRequests apiRequests,
        ParsedClass parsedClass,
        ILogger<Program> logger,
        IMemoryCache memoryCache,
        ClientOptions clientOptions)  // Добавили кэш
    {
        _rssRequests = rssRequests;
        _rssRequestsReserve = rssRequestsReserve;
        _apiRequests = apiRequests;
        _parsedClass = parsedClass;
        _logger = logger;
        _clientOptions = clientOptions;
        _serviceProvider = serviceProvider;
        _memoryCache = memoryCache;
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Запуск Telegram Converter Bot...");

        try
        {
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            var program = scope.ServiceProvider.GetRequiredService<Program>();

            // Пример вызова методов (можно вызывать где нужно)
            // await program.GetTimeZoneAsync("Moscow");
            // await program.GetNewsAsync();

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

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddEnvironmentVariables();
                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            })
            .ConfigureServices((context, services) =>
            {
                // Настройка конфигурации бота
                services.Configure<BotConfigModel>(
                    context.Configuration.GetSection("TelegramBotNews"));

                // Регистрируем бота
                services.AddSingleton<ITelegramBotClient>(sp =>
                {
                    var token = "8157960747:AAFVNCK_BUosOgLiFYwXQb9vdET8qcsOpjY";
                    Console.WriteLine($"✅ Создаю TelegramBotClient...");

                    var client = new TelegramBotClient(token);

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
                });

                // Регистрируем HTTP клиенты
                ConfigureHttpClients(services);

                // Регистрируем сервисы
                services.AddSingleton<CommandHendler>();
                services.AddHostedService<TelegramService>();
                services.AddSingleton<Dictionary<long, UserDataModel>>();

                // ВАЖНО: Регистрируем все наши сервисы
                services.AddScoped<RssRequests>();
                services.AddScoped<RssRequestsReserve>();
                services.AddScoped<ApiRequests>();
                services.AddScoped<ParsedClass>();
                services.AddScoped<ModelCrypto>();
                services.AddScoped<CryptoApiCourse>();
                services.AddScoped<ModelValute>();
                services.AddScoped<ValuteCourseRequest>();
                services.AddScoped<RequestFromStream>();
                services.AddScoped<FallBackPolitic>();

                // Добавляем кэширование (одна инстанция на всё приложение)
                services.AddMemoryCache();

                // Регистрируем Program
                services.AddScoped<Program>();

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

    private static void ConfigureHttpClients(IServiceCollection services)
    {
        //// Клиент для API
        //services.AddHttpClient("ApiClient", client =>
        //{
        //    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        //    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        //    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
        //    client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");

        //    client.DefaultRequestVersion = HttpVersion.Version20;
        //    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        //}).AddTransientHttpErrorPolicy(policy =>
        //policy.CircuitBreakerAsync(
        //    handledEventsAllowedBeforeBreaking: 5,
        //    durationOfBreak: TimeSpan.FromSeconds(30),
        //    onBreak: (outcome, timespan) =>
        //    {
        //        Console.WriteLine($"🔌 Circuit opened for {timespan}");
        //    },
        //    onHalfOpen: () =>
        //    {
        //        Console.WriteLine("⚠️ Circuit half-open");
        //    },
        //    onReset: () =>
        //    {
        //        Console.WriteLine("✅ Circuit reset");
        //    }))
        //.AddTransientHttpErrorPolicy(polly =>
        //    polly.WaitAndRetryAsync(3, retryCount =>
        //    TimeSpan.FromSeconds(Math.Pow(2, retryCount)) +
        //    TimeSpan.FromMicroseconds(Random.Shared.Next(0, 100)),
        //    onRetryAsync: (outcome, timespan, retryCount, task) =>
        //    {
        //        Console.WriteLine($"⏰ Request timed out after {timespan}");
        //        return Task.CompletedTask;
        //    }))
        //.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        //{
        //    EnableMultipleHttp2Connections = true,

        //    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
        //    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),

        //    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,

        //    MaxConnectionsPerServer = 10,
        //    UseCookies = false,
        //    AllowAutoRedirect = false,
        //})
        //.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
        //    TimeSpan.FromSeconds(10),
        //    Polly.Timeout.TimeoutStrategy.Pessimistic,
        //    onTimeoutAsync: (context, timespan, task) =>
        //    {
        //        Console.WriteLine($"⏰ Request timed out after {timespan}");
        //        return Task.CompletedTask;
        //    }));

        //// Клиент для RSS
        //services.AddHttpClient("RssClient", client =>
        //{
        //    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        //    client.DefaultRequestHeaders.Accept.ParseAdd("application/xml, text/xml, */*");
        //    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
        //    client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");

        //    client.DefaultRequestVersion = HttpVersion.Version20;
        //    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        //}).AddTransientHttpErrorPolicy(policy =>
        //policy.CircuitBreakerAsync(
        //    handledEventsAllowedBeforeBreaking: 5,
        //    durationOfBreak: TimeSpan.FromSeconds(30),
        //    onBreak: (outcome, timespan) =>
        //    {
        //        Console.WriteLine($"🔌 Circuit opened for {timespan}");
        //    },
        //    onHalfOpen: () =>
        //    {
        //        Console.WriteLine("⚠️ Circuit half-open");
        //    },
        //    onReset: () =>
        //    {
        //        Console.WriteLine("✅ Circuit reset");
        //    }))
        //.AddTransientHttpErrorPolicy(polly =>
        //polly.WaitAndRetryAsync(3, retryCount =>
        //TimeSpan.FromSeconds(Math.Pow(2, retryCount)) +
        //TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
        //onRetry: (outcome, timespan, retrycount, context) =>
        //{
        //    Console.WriteLine($"🔄 Retry {retrycount} after {timespan}");
        //}))
        //.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        //{
        //    EnableMultipleHttp2Connections = true,

        //    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
        //    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(8),

        //    MaxConnectionsPerServer = 10,
        //    UseCookies = false,
        //    AllowAutoRedirect = false
        //}).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
        //    TimeSpan.FromSeconds(10),
        //    Polly.Timeout.TimeoutStrategy.Pessimistic,
        //    onTimeoutAsync: (outcome, timespan, task) =>
        //    {
        //        Console.WriteLine($"⏰ Timeout after {timespan}");
        //        return Task.CompletedTask;
        //    }));
        //// Резервный клиент для RSS
        //services.AddHttpClient("RssClientReserve", client =>
        //{
        //    client.Timeout = TimeSpan.FromSeconds(60);
        //    client.DefaultRequestHeaders.Accept.ParseAdd("application/xml, text/xml, */*");
        //    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
        //    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        //    client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");

        //    client.DefaultRequestVersion = HttpVersion.Version20;
        //    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        //}).AddTransientHttpErrorPolicy(policy =>
        //  policy.CircuitBreakerAsync(
        //    handledEventsAllowedBeforeBreaking: 5,
        //    durationOfBreak: TimeSpan.FromSeconds(30)
        //    ))
        //.AddTransientHttpErrorPolicy(polly =>
        //    polly.WaitAndRetryAsync(3, retryCount =>
        //    TimeSpan.FromSeconds(Math.Pow(2, retryCount))))
        //.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        //{
        //    EnableMultipleHttp2Connections = true,

        //    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
        //    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(8),

        //    MaxConnectionsPerServer = 10,
        //    UseCookies = false,
        //    AllowAutoRedirect = false
        //});

        ClientOptions options = new ClientOptions();

        options.serviceDescriptors(options.servicesFromApi, services);
        options.serviceDescriptors(options.servicesFromRss, services);
        options.serviceDescriptors(options.servicesFromRssReserve, services);
        options.serviceDescriptors(options.servicesFromHttp2Client, services);
        options.serviceDescriptors(options.servicesFromCryptocource, services);
        options.serviceDescriptors(options.servicesFromValuteCource, services);
    }

    // Метод для получения временной зоны
    public async Task<List<ModelTestApi>> GetTimeZoneAsync(string city)
    {
        try
        {
            _logger.LogInformation("Запрос временной зоны для города: {City}", city);

            string apiKey = "818684b83cb44c9f87e6a189bf48bf83";
            string url = $"https://api.ipgeolocation.io/timezone?apiKey={apiKey}&location=";

            var result = await _apiRequests.CachingApiRequests(url, city);

            if (result != null && result.Count > 0)
            {
                return result;
            }

            return new List<ModelTestApi>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении временной зоны для города {City}", city);
            return new List<ModelTestApi>();
        }
    }


    public async Task<List<ModelClassRss>> GetNewsAsync()
    {
        string url = "https://tass.com/rss/v2.xml";
        string reserveUrl = "https://tass.com/rss/v2.xm";

        try
        {
            _logger.LogInformation("Запрос новостей из основного источника");

            var news = await _rssRequests.CacheRequest(url);

            if (news != null && news.Any())
            {
                //foreach (var item in news)
                //{
                //    Console.WriteLine($"Title: {item.Title}");
                //    Console.WriteLine($"Link: {item.Link}");
                //    Console.WriteLine($"PublishDate: {item.PublisDate}");
                //    Console.WriteLine($"ID: {item.ID}");
                //    Console.WriteLine(new string('-', 40));
                //}
                return news;
            }

            // Если основные новости не получены, пробуем резервный источник
            _logger.LogWarning("Основной источник недоступен, пробуем резервный");
            var news2 = await GetReserveNewsAsync(reserveUrl);
            return news2;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении новостей, переходим на резервный источник");
            var news2 = await GetReserveNewsAsync(reserveUrl);
            return news2;
        }
    }

    private async Task<List<ModelClassRss>> GetReserveNewsAsync(string url)
    {
        try
        {
            var reserveNews = await _rssRequestsReserve.ReserveRequestCache(url);

            if (reserveNews != null && reserveNews.Any())
            {
                //foreach (var item in reserveNews)
                //{
                //    Console.WriteLine($"Title: {item.Title}");
                //    Console.WriteLine($"Link: {item.Link}");
                //    Console.WriteLine($"PublishDate: {item.PublisDate}");
                //    Console.WriteLine($"ID: {item.ID}");
                //    Console.WriteLine(new string('-', 40));
                //}
                return reserveNews;
            }
            else
            {
                _logger.LogWarning("Резервный источник тоже не вернул данных");
                return new List<ModelClassRss>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении новостей из резервного источника");
            return new List<ModelClassRss>();
        }
    }
}
