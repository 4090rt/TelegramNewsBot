using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;

namespace TelegramNewsBot.RequestAndParcing.RequestBse
{
    public class RssRequestsReserve
    {
        private readonly Microsoft.Extensions.Logging.ILogger<RssRequestsReserve> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public RssRequestsReserve(IHttpClientFactory httpClientFactory, Microsoft.Extensions.Logging.ILogger<RssRequestsReserve> logger)
        { 
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<Stream> RssRequestRes(string url)
        {
            var client = _httpClientFactory.CreateClient("RssClientReserve");

            try
            {
                _logger.LogInformation("Начинаю запрос");
                HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        if (stream != null)
                        {
                            _logger.LogInformation("Данные получены");
                            return stream;
                        }
                        else
                        {
                            _logger.LogWarning("Поток пустой");
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Ошибка при чтении потока данных" + ex.Message);
                        return null;
                    }
                }
                else
                {
                    _logger.LogError($"Ошибка запроса: посткод: {response.StatusCode}");
                    return null;
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError("Таймаут операции" + ex.Message);
                return null;
            }
            catch (RequestException ex)
            {
                _logger.LogError("Ошибка запроса" + ex.Message);
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
