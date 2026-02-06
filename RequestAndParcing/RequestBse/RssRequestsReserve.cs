using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public RssRequestsReserve(IHttpClientFactory httpClientFactory, Microsoft.Extensions.Logging.ILogger<RssRequestsReserve> logger, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _memoryCache = memoryCache;
            _memoryCache = memoryCache;
        }

        public async Task<List<ModelClassRss>> ReserveRequestCache(string url)
        {
            string keycache = $"cache_key" + DateTime.UtcNow;

            if (_memoryCache.TryGetValue(keycache, out object? cacheobject))
            {
                if (cacheobject is List<ModelClassRss> cachelist)
                {
                    _logger.LogInformation($"📦 Данные из кэша для {keycache}");
                    return cachelist;
                }
            }
            try
            {
                _logger.LogInformation("Делаю Запрос новостей");
                var request = await ReserveRequest(url);

                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(3));

                _memoryCache.Set(keycache, request, options);

                return request;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении Информации");
                throw;
            }
        }

        public async Task<List<ModelClassRss>> ReserveRequest(string url)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("RssClientReserv");
                _logger.LogInformation("Начинаю запрос");
                HttpResponseMessage respon = await client.GetAsync(url).ConfigureAwait(false);
                _logger.LogInformation($"Запрос завершен статус код: {respon.StatusCode}");
                if (respon.IsSuccessStatusCode)
                {
                    if (respon != null)
                    {
                        try
                        {
                            _logger.LogInformation("Читаю ответ");
                            var content = await respon.Content.ReadAsStreamAsync().ConfigureAwait(false);
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
            catch (TaskCanceledException ex)
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
