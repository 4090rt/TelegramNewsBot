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
    public class RssRequests
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<RssRequests> _logger;
        private readonly ILogger<ParsedClass> _loggerparce;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _memoryCache;
        private readonly ParsedClass _parsedClass;
        public RssRequests(IHttpClientFactory factory,ILogger<RssRequests> logger, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, ILogger<ParsedClass> loggerparce)
        {
            _factory = factory;
            _logger = logger;
            _memoryCache = memoryCache;
            _loggerparce = loggerparce;

        }

        public async Task<List<ModelClassRss>> CacheRequest(string url)
        {
            string keycache = $"key_request_news{url}";

            if (_memoryCache.TryGetValue(keycache, out object? CACHELISTobject))
            {
                if (CACHELISTobject is List<ModelClassRss> cacheList)
                {
                    _logger.LogInformation($"📦 Данные из кэша для {keycache}");
                    return cacheList;
                }
            }
            try
            {
                _logger.LogInformation("Делаю Запрос новостей");
                var result = await Request(url);

                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(3));

                _memoryCache.Set(keycache, result, options);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении Информации");
                throw;
            }
        }



        public async Task<List<ModelClassRss>> Request(string URL)
        {
            try
            {
              var client =  _factory.CreateClient("RssCLient");
                _logger.LogInformation("Делаю запрос");

                HttpResponseMessage response = await client.GetAsync(URL).ConfigureAwait(false);
                _logger.LogInformation($"Запрос завершен статус код: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    if (response != null)
                    {
                        try
                        {
                            _logger.LogInformation("Читаю ответ");
                            var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
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
            catch (TaskCanceledException ex)
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
