using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TelegramNewsBot.RequestAndParcing.ModelBse;
using TelegramNewsBot.RequestAndParcing.ParsedBase;

namespace TelegramNewsBot.RequestAndParcing.RequestBse
{
    public class ValuteCourseRequest
    {
        private readonly Microsoft.Extensions.Logging.ILogger<ValuteCourseRequest> _logger;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ParsedClass _parsedClass;
        private readonly Microsoft.Extensions.Logging.ILogger<ParsedClass> _loggerparsed;
        private readonly SemaphoreSlim _semaphoreslin = new SemaphoreSlim(1,1);
        private readonly FallBackPolitic _fallbackPolitic;

        public ValuteCourseRequest(Microsoft.Extensions.Logging.ILogger<ValuteCourseRequest> logger, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, IHttpClientFactory httpClientFactory, ParsedClass parsedClass, Microsoft.Extensions.Logging.ILogger<ParsedClass> loggerparsed, FallBackPolitic fallbackPolitic)
        {
            _logger = logger;
            _loggerparsed = loggerparsed;
            _memoryCache = memoryCache;
            _parsedClass = parsedClass;
            _httpClientFactory = httpClientFactory;
            _fallbackPolitic = fallbackPolitic;
        }

        public async Task<ModelValute> CachingRequest(string url, CancellationToken cancellation = default)
        {
            string cache_memory = $"memory_cached{url}";
            ModelValute? oldcache = null;
            if (_memoryCache.TryGetValue(cache_memory, out ModelValute? cached) && cached != null)
            {
                oldcache = cached;
                _logger.LogInformation($"📦 Данные из кэша для {cache_memory}");
                return cached;
            }
            await _semaphoreslin.WaitAsync(cancellation);
            try
            {
                if (_memoryCache.TryGetValue(cache_memory, out ModelValute? cached2) && cached2 != null)
                {
                    return cached2;
                }

                var fallback = _fallbackPolitic.fallbackPolicyS(
                    _fallbackPolitic.Proverka,
                    oldcache,
                    cache_memory,
                    cancellation);

                _logger.LogInformation("Делаю Запрос валют");
                var resultfallback = await fallback.ExecuteAsync(async () =>
                {
                    var result = await Request(url, cancellation).ConfigureAwait(false);

                    if (result != null)
                    {
                        var options = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                            .SetSlidingExpiration(TimeSpan.FromMinutes(3));

                        _memoryCache.Set(cache_memory, result, options);

                        var StaleOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(25));

                        _memoryCache.Set($"stale: {cache_memory}", result, StaleOptions);
                        _logger.LogInformation($"✅ Cached fresh data for {cache_memory}");
                    }
                    return result;
                });
                return resultfallback;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return null;
            }
            finally
            {
                _semaphoreslin.Release();
            }
        }

        public async Task<ModelValute> Request(string url, CancellationToken cancellation = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ValuteCourse");

                var options = new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };
                _logger.LogInformation("Начинаю запрос");
                var timer = System.Diagnostics.Stopwatch.StartNew();
                HttpResponseMessage responce = await client.SendAsync(options, cancellation).ConfigureAwait(false);
                timer.Stop();
                _logger.LogInformation($"Запрос завершен за {timer}:" + responce.StatusCode);
                if (responce.IsSuccessStatusCode)
                {
                    if (responce != null)
                    {
                        try
                        {
                            _logger.LogInformation("Читаю ответ");
                            await using var content = await responce.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            _logger.LogInformation("Отве прочитан");

                            _logger.LogInformation("Начниаю парсинг");
                            var result = await _parsedClass.ParsedValute(content).ConfigureAwait(false);
                            _logger.LogInformation("Парсинг завершен");

                            return result;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                            return null;
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Тело ответа не найдено");
                        return null;
                    }
                }
                else
                {
                    _logger.LogInformation($"Запрос завершился ошибкой, статус код:" + responce.StatusCode);
                    return null;
                }

            }
            catch (TaskCanceledException ex) when (!cancellation.IsCancellationRequested)
            {
                _logger.LogError("Операция отменена" + ex.Message + ex.StackTrace);
                return null;
            }
            catch (TaskCanceledException ex) when (cancellation.IsCancellationRequested)
            {
                _logger.LogError("Операция отменена пользователем" + ex.Message + ex.StackTrace);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Возникло исключение при выполнении запроса" + ex.Message, ex.StackTrace);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return null;
            }
        }
    }
}
