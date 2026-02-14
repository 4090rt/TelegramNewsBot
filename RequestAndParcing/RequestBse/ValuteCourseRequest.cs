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
    public class ValuteCourseRequest
    {
        private readonly Microsoft.Extensions.Logging.ILogger<ValuteApiRequest> _logger;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ParsedClass _parsedClass;
        private readonly Microsoft.Extensions.Logging.ILogger<ParsedClass> _loggerparsed;

        public ValuteCourseRequest(Microsoft.Extensions.Logging.ILogger<ValuteApiRequest> logger, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, IHttpClientFactory httpClientFactory, ParsedClass parsedClass, Microsoft.Extensions.Logging.ILogger<ParsedClass> loggerparsed)
        {
            _logger = logger;
            _loggerparsed = loggerparsed;
            _memoryCache = memoryCache;
            _parsedClass = parsedClass;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ModelValute> CachingRequest(string url, CancellationToken cancellation = default)
        {
            string cache_memory = $"memory_cached{url}";

            if (_memoryCache.TryGetValue(cache_memory, out ModelValute cached))
            {
                _logger.LogInformation($"📦 Данные из кэша для {cache_memory}");
                return cached;
            }
            try
            {
                _logger.LogInformation("Запрашвиваю данные о валютах");

                var result = await Request(url, cancellation);

                var optioms = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(8));

                _memoryCache.Set(cache_memory, result, optioms);
                  return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return new ModelValute();
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
                            var result =await _parsedClass.ParsedValute(content).ConfigureAwait(false);
                            _logger.LogInformation("Парсинг завершен");

                            return result;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                            return new ModelValute();
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Тело ответа не найдено");
                        return new ModelValute();
                    }
                }
                else
                {
                    _logger.LogInformation($"Запрос завершился ошибкой, статус код:" + responce.StatusCode);
                    return new ModelValute();
                }

            }
            catch (TaskCanceledException ex) when (!cancellation.IsCancellationRequested)
            {
                _logger.LogError("Операция отменена" + ex.Message + ex.StackTrace);
                return new ModelValute();
            }
            catch (TaskCanceledException ex) when (cancellation.IsCancellationRequested)
            {
                _logger.LogError("Операция отменена пользователем" + ex.Message + ex.StackTrace);
                return new ModelValute();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Возникло исключение при выполнении запроса" + ex.Message, ex.StackTrace);
                return new ModelValute();
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return new ModelValute();
            }
        }
    }
}
