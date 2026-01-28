using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramNewsBot.TelegramBotSet.ModelsTg;

namespace TelegramNewsBot.RequestAndParcing.RequestBse
{
    public class ApiRequests
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiRequests> _logger;
        public readonly Dictionary<long, UserDataModel> _userSession;

        public ApiRequests(IHttpClientFactory httpClientFactory, ILogger<ApiRequests> logger)
        { 
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _userSession = new Dictionary<long, UserDataModel>();
        }

        public async Task<Stream> ApiRequesttss(string url, string citys)
        {
            char[] MyChar = {'!'};
            string city = citys.TrimStart(MyChar);
            string updateurl = url + city;
            Console.WriteLine(updateurl);
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");

                _logger.LogInformation("Начинаю запрос");
                HttpResponseMessage recpon = await client.GetAsync(updateurl).ConfigureAwait(false);
                _logger.LogInformation($"Запрос выполнен");
                if (recpon.IsSuccessStatusCode)
                {
                    if (recpon != null)
                    {
                        try
                        {
                            var streamreading = await recpon.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            _logger.LogInformation("Данные получены");
                            return streamreading;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Не удалось прочитать ответ" + ex.Message);
                            return null;
                        }
                    }
                    else
                    {
                        _logger.LogError("Поток данных пуст");
                        return null;
                    }
                }
                else
                {
                    _logger.LogError("Ошибка запросa" + recpon.StatusCode);
                    return null;
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError("Операция была отменена" + ex.Message);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("При попытке запроса возникло исключение" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message);
                return null;
            }
        }
    }
}
