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
using TelegramNewsBot.TelegramBotSet.ModelsTg;

namespace TelegramNewsBot.RequestAndParcing.RequestBse
{
    public class ApiRequests
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiRequests> _logger;
        public readonly Dictionary<long, UserDataModel> _userSession;
        private readonly ParsedClass _parseClass;
        private readonly IMemoryCache _iMemoryCache;
        public ApiRequests(IHttpClientFactory httpClientFactory, ILogger<ApiRequests> logger, ParsedClass parseClass, IMemoryCache iMemoryCache)
        { 
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _userSession = new Dictionary<long, UserDataModel>();
            _parseClass = parseClass;
            _iMemoryCache = iMemoryCache;
        }

        public async Task<ModelTestApi> CachingApiRequests(string url, string city)
        {
            string key_cache = $"caching_key_{url}_{city}";

            if (_iMemoryCache.TryGetValue(key_cache, out ModelTestApi caching))
            {
                _logger.LogInformation($"📦 Данные из кэша для {key_cache}");
                return caching;
            }
            try
            {
                _logger.LogInformation("Делаю запрос данных");

                var result = await ApiRequesttss(url, city);

                _logger.LogInformation("Данные получены");

                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(8));

                _iMemoryCache.Set(key_cache, result,options);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Возникло исключение при кэшировании данных" + ex.Message + ex.StackTrace);
                return new ModelTestApi();
            }
        }

        public async Task<ModelTestApi> ApiRequesttss(string url, string citys, CancellationToken cancellation = default)
        {
            char[] MyChar = {'!'};
            string city = citys.TrimStart(MyChar);
            string updateurl = url + city;
            Console.WriteLine(updateurl);
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                var options = new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                _logger.LogInformation("Начинаю запрос");
                var timer = System.Diagnostics.Stopwatch.StartNew();
                HttpResponseMessage recpon = await client.SendAsync(options, cancellation).ConfigureAwait(false);
                timer.Stop();
                _logger.LogInformation($"Запрос выполнен");
                if (recpon.IsSuccessStatusCode)
                {
                    if (recpon != null)
                    {
                        try
                        {
                            _logger.LogInformation("Читаю ответ");
                            await using var streamreading = await recpon.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            _logger.LogInformation("Данные получены");

                            _logger.LogInformation("Начинаю парсинг");
                            var parsed = await _parseClass.ParsedApi(streamreading).ConfigureAwait(false);
                            _logger.LogInformation("Парсинг окончен");

                            return parsed;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Не удалось прочитать ответ" + ex.Message);
                            return new ModelTestApi();
                        }
                    }
                    else
                    {
                        _logger.LogError("Поток данных пуст");
                        return new ModelTestApi();
                    }
                }
                else
                {
                    _logger.LogError($"Ошибка запросa по url: {updateurl}" + recpon.StatusCode);
                    return new ModelTestApi();
                }
            }
            catch (TaskCanceledException ex) when (cancellation.IsCancellationRequested)
            {
                _logger.LogError("Операция была отменена пользователем" + ex.Message);
                return new ModelTestApi();
            }
            catch (TaskCanceledException ex) when (!cancellation.IsCancellationRequested)
            {
                _logger.LogError("Операция была отменена" + ex.Message);
                return new ModelTestApi();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("При попытке запроса возникло исключение" + ex.Message);
                return new ModelTestApi();
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message);
                return new ModelTestApi();
            }
        }
    }
}
