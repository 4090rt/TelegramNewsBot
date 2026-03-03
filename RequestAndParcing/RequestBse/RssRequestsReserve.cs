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
    public class RssRequestsReserve
    {
        private readonly Microsoft.Extensions.Logging.ILogger<RssRequestsReserve> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _memoryCache;
        private readonly Microsoft.Extensions.Logging.ILogger<ParsedClass> _loggerparsed;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);

        public RssRequestsReserve(IHttpClientFactory httpClientFactory, Microsoft.Extensions.Logging.ILogger<RssRequestsReserve> logger, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _memoryCache = memoryCache;
            _memoryCache = memoryCache;
        }

        public async Task<List<ModelClassRss>> ReserveRequestCache(string url, CancellationToken cancellation = default)
        {
            string keycache = $"cache_key" + DateTime.UtcNow;
            List<ModelClassRss> oldcache = null;
            if (_memoryCache.TryGetValue(keycache, out object? cacheobject))
            {
                if (cacheobject is List<ModelClassRss> cachelist)
                {
                    oldcache = cachelist;
                    _logger.LogInformation($"📦 Данные из кэша для {keycache}");
                    return cachelist;
                }
            }
            await _semaphore.WaitAsync(cancellation);
            try
            {
                if (_memoryCache.TryGetValue(keycache, out object? cachedobject))
                {
                    if (cachedobject is List<ModelClassRss> cached)
                    {
                        return cached;
                    }
                }

                var fallback = Policy<List<ModelClassRss>>
                    .Handle<Exception>()
                    .OrResult(r => r == null || r.Count > 0)
                    .FallbackAsync(
                     fallbackAction: async (outcome, context, cancellation) =>
                     {
                         var exeption = outcome.Exception;
                         var isEmpty = outcome.Result?.Count == 0;

                         if (exeption != null)
                         {
                             _logger.LogWarning($"⚠️ Fallback by exception: {exeption.Message}");
                         }
                         if (isEmpty)
                         {
                             _logger.LogWarning($"⚠️ Fallback by empty result");
                         }
                         if (oldcache != null)
                         {
                             _logger.LogInformation("✅ Fallback: возвращаю старые данные из кэша");
                             return oldcache;
                         }

                         var stalekey = $"stalekey:{keycache}";

                         if (_memoryCache.TryGetValue(stalekey, out List<ModelClassRss> cached))
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

                var fallbackresult = await fallback.ExecuteAsync(async () =>
                {
                    var request = await ReserveRequest(url);

                    if (request != null || request.Count > 0)
                    {

                        var options = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                            .SetSlidingExpiration(TimeSpan.FromMinutes(3));

                        _memoryCache.Set(keycache, request, options);

                        var StaleOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(25));

                        _memoryCache.Set($"stale: {keycache}", request, StaleOptions);
                        _logger.LogInformation($"✅ Cached fresh data for {keycache}");
                    }
                    return request ?? new List<ModelClassRss>();
                });
                return fallbackresult;
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

        public async Task<List<ModelClassRss>> ReserveRequest(string url, CancellationToken cancellation = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("RssClientReserv");

                var options = new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                _logger.LogInformation("Начинаю запрос");
                HttpResponseMessage respon = await client.SendAsync(options, cancellation).ConfigureAwait(false);
                _logger.LogInformation($"Запрос завершен статус код: {respon.StatusCode}");
                if (respon.IsSuccessStatusCode)
                {
                    if (respon != null)
                    {
                        try
                        {
                            _logger.LogInformation("Читаю ответ");
                            await using var content = await respon.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            _logger.LogInformation("Ответ прочитан");

                            _logger.LogInformation("Начинаю парсинг");
                            ParsedClass parsed = new ParsedClass(_loggerparsed);
                            var result = await parsed.ParseRss(content);
                            _logger.LogInformation("Ответ распаршен");

                            return result;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Возникло исключение при чтении ответа" + ex.Message + ex.StackTrace);
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
                    _logger.LogError("При выполнении запроса возникла ошибка" + respon.StatusCode);
                    return new List<ModelClassRss>();
                }
            }
            catch (TaskCanceledException ex) when (cancellation.IsCancellationRequested)
            {
                _logger.LogError("Опаерация отменена пользователем" + ex.Message + ex.StackTrace);
                return new List<ModelClassRss>();
            }
            catch (TaskCanceledException ex) when (!cancellation.IsCancellationRequested)
            {
                _logger.LogError("Опаерация отменена" + ex.Message  + ex.StackTrace);
                return new List<ModelClassRss>();
            }
            catch (RequestException ex)
            {
                _logger.LogError("При выполнени запроса возникло исключение" + ex.Message + ex.StackTrace);
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
