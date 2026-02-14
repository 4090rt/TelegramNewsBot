using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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

        public CryptoApiCourse(Microsoft.Extensions.Logging.ILogger<CryptoApiCourse> logger, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, IHttpClientFactory httpClientFactory, ParsedClass parsedClass, Microsoft.Extensions.Logging.ILogger<ParsedClass> loggerparsed)
        {
            _logger = logger;
            _loggerparsed = loggerparsed;
            _memoryCache = memoryCache;
            _parsedClass = parsedClass;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ModelCrypto> CacheRequest(string url, CancellationToken cancellation = default)
        {
            string key_cache = $"key_cache_{url}";

            if (_memoryCache.TryGetValue(key_cache, out object? cachedobject))
            {
                if (cachedobject is ModelCrypto cachde)
                {
                    _logger.LogInformation($"📦 Данные из кэша для {cachde}");
                    return cachde;
                }
            }
            try
            {
                _logger.LogInformation("Начинаю запрос данных");

                var result = await Request(url, cancellation).ConfigureAwait(false);

                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(8));

                _memoryCache.Set(key_cache, result, options);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return new ModelCrypto();
            }
        }

        public async Task<ModelCrypto> Request(string url, CancellationToken cancellation = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("CryptoApyCourse");

                var options = new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

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
                            return new ModelCrypto();
                        }
                    }
                    else
                    {
                        _logger.LogError("Объект не нейден");
                        return new ModelCrypto();
                    }
                }
                else
                {
                    _logger.LogError("Запрос завершился статус кодом:"  + recpon.StatusCode);
                    return new ModelCrypto();
                }
            }
            catch (TaskCanceledException ex) when (!cancellation.IsCancellationRequested)
            {
                _logger.LogError("Операция отменена" + ex.Message + ex.StackTrace);
                return new ModelCrypto();
            }
            catch (TaskCanceledException ex) when (cancellation.IsCancellationRequested)
            {
                _logger.LogError("Операция отменена пользователем" + ex.Message + ex.StackTrace);
                return new ModelCrypto();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Возникло исключение при выполнении запроса" + ex.Message, ex.StackTrace);
                return new ModelCrypto();
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return new ModelCrypto();
            }
        }
    }
}
