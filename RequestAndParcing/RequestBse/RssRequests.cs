using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bots.Types;
using TelegramNewsBot.RequestAndParcing.ModelBse;
using TelegramNewsBot.RequestAndParcing.ParsedBase;

namespace TelegramNewsBot.RequestAndParcing.RequestBse
{
    public class RssRequests
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<RssRequests> _logger;
        private readonly ILogger<ParsedClass> _loggerparce;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _memoryCache;
        private readonly ParsedClass _parsedClass;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);
        public RssRequests(IHttpClientFactory factory,ILogger<RssRequests> logger, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, ILogger<ParsedClass> loggerparce)
        {
            _factory = factory;
            _logger = logger;
            _memoryCache = memoryCache;
            _loggerparce = loggerparce;

        }

        public async Task<List<ModelClassRss>> CacheRequest(string url, CancellationToken cancellation = default)
        {
            string keycache = $"key_request_news{url}";
            List<ModelClassRss>? oldcache = null;
            if (_memoryCache.TryGetValue(keycache, out object? cacheobject) &&
               cacheobject is List<ModelClassRss> cached)
            {
                    oldcache = cached;
                    _logger.LogInformation($"📦 Данные из кэша для {keycache}");
                    return cached;
            }
            await _semaphore.WaitAsync(cancellation);
            try
            {
                if (_memoryCache.TryGetValue(keycache, out object? cachedobject))
                {
                    if (cachedobject is List<ModelClassRss> cached2)
                    {
                        return cached2;
                    }
                }

                var fallback = Policy<List<ModelClassRss>>
                    .Handle<Exception>()
                    .OrResult(r => r == null || r.Count == 0)
                    .FallbackAsync(
                    fallbackAction: async (outcome, context, cancellation) =>
                    {
                        var exception = outcome.Exception;
                        var isempty = outcome.Result?.Count == 0;

                        if (exception != null)
                        {
                            _logger.LogWarning($"⚠️ Fallback by exception: {exception.Message}");
                        }
                        else if (isempty)
                        {
                            _logger.LogWarning($"⚠️ Fallback by empty result");
                        }
                        if (oldcache != null)
                        {
                            _logger.LogInformation("✅ Fallback: возвращаю старые данные из кэша");
                            return oldcache;
                        }
                        var stalekey = $"stalekey:{keycache}";

                        if (_memoryCache.TryGetValue(stalekey, out object? cacheobject) && cacheobject is List<ModelClassRss> cached)
                        {
                            _logger.LogInformation($"✅ Returning stale copy for {cached}");
                            return cached;
                        }
                        _logger.LogWarning("⚠️ Fallback: кэш пуст, возвращаю пустой список");
                        return new List<ModelClassRss>();
                    },
                     onFallbackAsync: async (outcome, ctx) =>
                     {
                         _logger.LogError($"🆘 Fallback сработал: {outcome.Exception?.Message}");
                         await Task.CompletedTask;
                     });

                _logger.LogInformation("Делаю Запрос новостей");
                var resultfallback = await fallback.ExecuteAsync(async () =>
                {
                    var result = await Request(url, cancellation).ConfigureAwait(false);

                    if (result != null && result.Count > 0)
                    {
                        var options = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                            .SetSlidingExpiration(TimeSpan.FromMinutes(3));

                        _memoryCache.Set(keycache, result, options);

                        var StaleOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(25));

                        _memoryCache.Set($"stale: {keycache}", result, StaleOptions);
                        _logger.LogInformation($"✅ Cached fresh data for {keycache}");
                    }
                    return result ?? new List<ModelClassRss>();
                });
                return resultfallback;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении Информации");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public async Task<List<ModelClassRss>> Request(string URL, CancellationToken cancellation = default)
        {
            try
            {
                var client = _factory.CreateClient("RssCLient");

                var options = new HttpRequestMessage(HttpMethod.Get, URL)
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                _logger.LogInformation("Делаю запрос");
                var timer = System.Diagnostics.Stopwatch.StartNew();    
                HttpResponseMessage response = await client.SendAsync(options, cancellation).ConfigureAwait(false);
                timer.Stop();
                _logger.LogInformation($"Запрос завершен статус код: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    if (response != null)
                    {
                        try
                        {
                            _logger.LogInformation("Читаю ответ");
                            await using var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            _logger.LogInformation("Ответ прочитан");

                            _logger.LogInformation("Начинаю парсинг");
                            ParsedClass parsed = new ParsedClass(_loggerparce);
                            var result = await parsed.ParseRss(content);
                            _logger.LogInformation("Ответ распаршен");
                            return result;
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError("ВОзникло исключение" + ex.Message + ex.StackTrace);
                            return new List<ModelClassRss>();
                        }
                       
                    }
                    else
                    {
                        _logger.LogError("Ответ пуст");
                        return new List<ModelClassRss>();
                    }
                }
                else
                {
                    _logger.LogError("При запаросе возникла ошибка" + response.StatusCode);
                    return new List<ModelClassRss>();
                }
            }
            catch (TaskCanceledException ex) when (cancellation.IsCancellationRequested)
            {
                _logger.LogError("Запрос отменен пользователем" + ex.Message, ex.StackTrace);
                return new List<ModelClassRss>();
            }
            catch (TaskCanceledException ex) when (!cancellation.IsCancellationRequested)
            {
                _logger.LogError("Запрос отменен" + ex.Message, ex.StackTrace);
                return new List<ModelClassRss>();
            }
            catch (RequestException ex)
            {
                _logger.LogError("Возникло исключение при выполнении запроса" + ex.Message, ex.StackTrace);
                return new List<ModelClassRss>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return new List<ModelClassRss>();
            }
        }
    }
}
