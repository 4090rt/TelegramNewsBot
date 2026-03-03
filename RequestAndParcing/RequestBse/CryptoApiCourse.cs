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
    public class CryptoApiCourse
    {
        private readonly Microsoft.Extensions.Logging.ILogger<CryptoApiCourse> _logger;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ParsedClass _parsedClass;
        private readonly Microsoft.Extensions.Logging.ILogger<ParsedClass> _loggerparsed;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ExceptionClass _exceptionclass;
        private readonly Httpoptions _httpoptions;

        public CryptoApiCourse(Microsoft.Extensions.Logging.ILogger<CryptoApiCourse> logger, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, IHttpClientFactory httpClientFactory, ParsedClass parsedClass, Microsoft.Extensions.Logging.ILogger<ParsedClass> loggerparsed)
        {
            _logger = logger;
            _loggerparsed = loggerparsed;
            _memoryCache = memoryCache;
            _parsedClass = parsedClass;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<ModelCrypto>> CacheRequest(string url, CancellationToken cancellation = default)
        {
            string key_cache = $"key_cache_{url}";
            List<ModelCrypto> oldcache = null;
            if (_memoryCache.TryGetValue(key_cache, out List<ModelCrypto>? cached) && cached != null)
            {
                oldcache = cached;
                _logger.LogInformation($"📦 Данные из кэша для {key_cache}");
                return cached;
            }
            await _semaphore.WaitAsync(cancellation);
            try
            {
                if (_memoryCache.TryGetValue(key_cache, out List<ModelCrypto>? cached2) && cached2 != null)
                {
                    return cached2;
                }

                var fallback = Policy<List<ModelCrypto>>
                    .Handle<Exception>()
                    .OrResult(r => r == null || r.Count > 0)
                    .FallbackAsync(fallbackAction: async (outcome, context, cancellation) =>
                    {
                        var exception = outcome.Exception;
                        var isEmpty = outcome.Result?.Count == 0;

                        if (exception != null)
                        {
                            _logger.LogWarning($"⚠️ Fallback by empty result");
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

                        var stalekey = $"stalekey:{key_cache}";

                        if (_memoryCache.TryGetValue(stalekey, out List<ModelCrypto> cached))
                        {
                            _logger.LogInformation($"✅ Returning stale copy for {cached}");
                            return cached;
                        }
                        _logger.LogWarning("⚠️ Fallback: кэш пуст, возвращаю пустой список");
                        return new List<ModelCrypto>();
                    },
                    onFallbackAsync: async (outcome, ctx) =>
                    {
                        _logger.LogError($"🆘 Fallback сработал: {outcome.Exception?.Message}");
                        await Task.CompletedTask;
                    });
                _logger.LogInformation("Начинаю запрос данных");

                var fallbackresult = await  fallback.ExecuteAsync(async () =>
                {
                    var result = await Request(url, cancellation).ConfigureAwait(false);

                    if (result != null || result.Count > 0)
                    {
                        var options = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                            .SetSlidingExpiration(TimeSpan.FromMinutes(8));

                        _memoryCache.Set(key_cache, result, options);

                        var StaleOptions = new MemoryCacheEntryOptions()
                         .SetAbsoluteExpiration(TimeSpan.FromMinutes(25));

                        _memoryCache.Set($"stale: {key_cache}", result, StaleOptions);
                        _logger.LogInformation($"✅ Cached fresh data for {key_cache}");
                    }
                    return result ?? new List<ModelCrypto>();
                });
                return fallbackresult;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return new List<ModelCrypto>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<ModelCrypto>> Request(string url, CancellationToken cancellation = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CryptoApyCourse");

                var options = _httpoptions.OptionsComleted(_httpoptions.Options, url);

                _logger.LogInformation("Начинаю запрос");
                var timer = System.Diagnostics.Stopwatch.StartNew();
                HttpResponseMessage recpon = await client.SendAsync(options, cancellation).ConfigureAwait(false);
                timer.Stop();
                _logger.LogInformation($"Запрос завешен за {timer}" + recpon.StatusCode);
                if (recpon.IsSuccessStatusCode)
                {
                    if (recpon != null)
                    {
                        try
                        {
                            _logger.LogInformation("Читаю ответ");
                            await using var content = await recpon.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            _logger.LogInformation("Ответ прочитан");

                            _logger.LogInformation("Начинаю парсинг");
                            var result = await _parsedClass.PaedesCrypto(content);
                            _logger.LogInformation("Парсинг закончпен");

                            return result;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                            return new List<ModelCrypto>();
                        }
                    }
                    else
                    {
                        _logger.LogError("Объект не нейден");
                        return new List<ModelCrypto>();
                    }
                }
                else
                {
                    _logger.LogError("Запрос завершился статус кодом:"  + recpon.StatusCode);
                    return new List<ModelCrypto>();
                }
            }
            catch (TaskCanceledException ex) when (!cancellation.IsCancellationRequested)
            {
                _exceptionclass.Send1(_exceptionclass.Exceptions, ex);
                return new List<ModelCrypto>();
            }
            catch (TaskCanceledException ex) when (cancellation.IsCancellationRequested)
            {
                _exceptionclass.Send1(_exceptionclass.Exceptions, ex);
                return new List<ModelCrypto>();
            }
            catch (HttpRequestException ex)
            {
                _exceptionclass.Send1(_exceptionclass.Exceptions, ex);
                return new List<ModelCrypto>();
            }
            catch (Exception ex)
            {
                _exceptionclass.Send1(_exceptionclass.Exceptions, ex);
                return new List<ModelCrypto>();
            }   
        }
    }
}
