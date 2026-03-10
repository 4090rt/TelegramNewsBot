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
        private readonly FallBackPolitic _fallbackPolitic;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Httpoptions _httpOptions;
        private readonly ExceptionClass _exceptionClass;
        public ApiRequests(IHttpClientFactory httpClientFactory, ILogger<ApiRequests> logger, ParsedClass parseClass, IMemoryCache iMemoryCache, FallBackPolitic fallbackPolitic)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _userSession = new Dictionary<long, UserDataModel>();
            _parseClass = parseClass;
            _iMemoryCache = iMemoryCache;
            _fallbackPolitic = fallbackPolitic;
        }

        public async Task<List<ModelTestApi>> CachingApiRequests(string url, string city, CancellationToken cancellation = default)
        {
            string key_cache = $"caching_key_{url}_{city}";
            List<ModelTestApi>? oldcache = null;
            if (_iMemoryCache.TryGetValue(key_cache, out List<ModelTestApi>? caching))
            {
                oldcache = caching;
                _logger.LogInformation($"📦 Данные из кэша для {key_cache}");
                return caching;
            }
            await _semaphore.WaitAsync(cancellation);
            try
            {
                if (_iMemoryCache.TryGetValue(key_cache, out List<ModelTestApi>? cached2))
                {
                    return cached2;
                }

                var fallback = _fallbackPolitic.fallbackPolicyS(
                    _fallbackPolitic.Proverka,
                    oldcache,
                    key_cache,
                    cancellation);

                _logger.LogInformation("Делаю запрос данных");
                var fallbackresult = await fallback.ExecuteAsync(async () =>
                {
                    var result = await ApiRequesttss(url, city, cancellation).ConfigureAwait(false);

                    if (result != null && result.Count > 0)
                    {
                        var options = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                            .SetSlidingExpiration(TimeSpan.FromMinutes(8));

                        _iMemoryCache.Set(key_cache, result, options);

                        var StaleOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(25));

                        _iMemoryCache.Set($"stale: {key_cache}", result, StaleOptions);
                        _logger.LogInformation($"✅ Cached fresh data for {key_cache}");
                    }
                    return result ?? new List<ModelTestApi>();
                });
                return fallbackresult;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Возникло исключение при кэшировании данных" + ex.Message + ex.StackTrace);
                return new List<ModelTestApi>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<ModelTestApi>> ApiRequesttss(string url, string citys, CancellationToken cancellation = default)
        {
            char[] MyChar = {'!'};
            string city = citys.TrimStart(MyChar);
            string updateurl = url + city;
            Console.WriteLine(updateurl);
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                var options = _httpOptions.OptionsComleted(_httpOptions.Options, url);

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
                            _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
                            return new List<ModelTestApi>();
                        }
                    }
                    else
                    {
                        _logger.LogError("Поток данных пуст");
                        return new List<ModelTestApi>();
                    }
                }
                else
                {
                    _logger.LogError($"Ошибка запросa по url: {updateurl}" + recpon.StatusCode);
                    return new List<ModelTestApi>();
                }
            }
            catch (TaskCanceledException ex) when (cancellation.IsCancellationRequested)
            {
                _logger.LogError("Операция была отменена пользователем" + ex.Message);
                return new List<ModelTestApi>();
            }
            catch (TaskCanceledException ex) when (!cancellation.IsCancellationRequested)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
                return new List<ModelTestApi>();
            }
            catch (HttpRequestException ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
                return new List<ModelTestApi>();
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
                return new List<ModelTestApi>();
            }
        }
    }
}
