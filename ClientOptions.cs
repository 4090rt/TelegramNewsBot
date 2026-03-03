using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot
{
    public delegate void Servicegelegat(IServiceCollection services);
    public class ClientOptions
    {
        public void serviceDescriptors(Servicegelegat delegat, IServiceCollection services)
        { 
               delegat?.Invoke(services);
        }
        // Клиент для API
        public void servicesFromApi(IServiceCollection services)
        {
            services.AddHttpClient("ApiClient", client =>
            {
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");


                client.DefaultRequestVersion = HttpVersion.Version20;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            }).AddTransientHttpErrorPolicy(policy =>
            policy.CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (outcome, timespan) =>
                {
                    Console.WriteLine($"🔌 Circuit opened for {timespan}");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("⚠️ Circuit half-open");
                },
                onReset: () =>
                {
                    Console.WriteLine("✅ Circuit reset");
                }))
            .AddTransientHttpErrorPolicy(polly =>
            polly.WaitAndRetryAsync(3, retryCount =>
            TimeSpan.FromSeconds(Math.Pow(2, retryCount)) +
            TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
            onRetryAsync: (outcome, timespan, retryCount, task) =>
            {
                Console.WriteLine($"⏰ Request timed out after {timespan}");
                return Task.CompletedTask;
            }))
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true,

                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(15),
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),

                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli,

                MaxConnectionsPerServer = 10,
                UseCookies = false,
                AllowAutoRedirect = false,
            })
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
                TimeSpan.FromSeconds(10),
                Polly.Timeout.TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (context, timespan, task) =>
                {
                    Console.WriteLine($"⏰ Request timed out after {timespan}");
                    return Task.CompletedTask;
                }));
        }
        // Клиент для RSS
        public void servicesFromRss(IServiceCollection services)
        {
            services.AddHttpClient("RssClient", client =>
            {
                client.DefaultRequestHeaders.Accept.ParseAdd("application/xml, text/xml, */*");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");

                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
                client.DefaultRequestVersion = HttpVersion.Version20;
            }).AddTransientHttpErrorPolicy(policy =>
            policy.CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (outcome, timespan) =>
                {
                    Console.WriteLine($"🔌 Circuit opened for {timespan}");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("⚠️ Circuit half-open");
                },
                onReset: () =>
                {
                    Console.WriteLine("✅ Circuit reset");
                })).AddTransientHttpErrorPolicy(polly =>
                polly.WaitAndRetryAsync(3, retryCount =>
                TimeSpan.FromSeconds(Math.Pow(2, retryCount)) +
                TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                onRetry: (outcome, timespan, retrycount, context) =>
                {
                    Console.WriteLine($"🔄 Retry {retrycount} after {timespan}");
                })).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,

                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(15),
                    PooledConnectionLifetime = TimeSpan.FromMinutes(10),

                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.Brotli | DecompressionMethods.GZip,

                    MaxConnectionsPerServer = 10,
                    UseCookies = false,
                    AllowAutoRedirect = false
                }).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
                    TimeSpan.FromSeconds(10),
                    Polly.Timeout.TimeoutStrategy.Pessimistic,
                    onTimeoutAsync: (context, timespan, task) =>
                    {
                        Console.WriteLine($"⏰ Request timed out after {timespan}");
                        return Task.CompletedTask;
              }));
        }
        // Клиент для Rss Reserve
        public void servicesFromRssReserve(IServiceCollection services)
        {
            services.AddHttpClient("RssClient", client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DefaultRequestHeaders.Accept.ParseAdd("application/xml, text/xml, */*");
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");

                client.DefaultRequestVersion = HttpVersion.Version20;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            }).AddTransientHttpErrorPolicy(poicy =>
            poicy.CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (outcome, timespan) =>
                {
                    Console.WriteLine($"🔌 Circuit opened for {timespan}");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("⚠️ Circuit half-open");
                },
                onReset: () =>
                {
                    Console.WriteLine("✅ Circuit reset");
                })).AddTransientHttpErrorPolicy(polly =>
                polly.WaitAndRetryAsync(3, retrycount =>
                TimeSpan.FromSeconds(Math.Pow(2, retrycount)) +
                TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                onRetry: async (outcome, timespan, retrycount, context) =>
                {
                    Console.WriteLine($"🔄 Retry {retrycount} after {timespan}");
                }))
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,

                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(15),
                    PooledConnectionLifetime = TimeSpan.FromMinutes(10),

                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,

                    MaxConnectionsPerServer = 10,
                    UseCookies = false,
                    AllowAutoRedirect = false,
                }).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
                    TimeSpan.FromSeconds(10),
                    Polly.Timeout.TimeoutStrategy.Pessimistic,
                    onTimeoutAsync: (context, timespan, task) =>
                    {
                        Console.WriteLine($"⏰ Timeout after {timespan}");
                        return Task.CompletedTask;
                    }));
        }
    }
}
