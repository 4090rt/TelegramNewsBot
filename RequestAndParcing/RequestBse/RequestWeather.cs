using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.RequestAndParcing.RequestBse
{
    public class RequestWeather
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public RequestWeather(IHttpClientFactory httpClientFactory, Microsoft.Extensions.Logging.ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<Stream> WetherApiRequets(string city,  string url, CancellationToken cancellation = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClientWeather");
                _logger.LogInformation("Начинаю запрос");

                HttpResponseMessage responce = await client.GetAsync(url).ConfigureAwait(false);
                if (responce.IsSuccessStatusCode)
                {
                    if (responce != null)
                    {
                        try
                        {
                            var result = await responce.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            if (result != null)
                            {
                                _logger.LogInformation("Данные прочитаны");
                                return result;
                            }
                            else
                            {
                                _logger.LogError("Поток не найден");
                                return Stream.Null;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Не удалось прочитать ответ" + ex.Message);
                            return Stream.Null;
                        }
                    }
                    else
                    {
                        _logger.LogError("Поток данных пуст");
                        return Stream.Null;
                    }
                }
                else
                {
                    _logger.LogError("Запрос завершился ошибкой" + responce.StatusCode);
                    return Stream.Null;
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError("Операция была отменена" + ex.Message);
                return Stream.Null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("При попытке запроса возникло исключение" + ex.Message);
                return Stream.Null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message);
                return Stream.Null;
            }
        }
    }
}
