using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.RequestBse
{
    public class RssRequests
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<RssRequests> _logger;
        public RssRequests(IHttpClientFactory factory,ILogger<RssRequests> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task<Stream> RssRequestsMethod(string url, CancellationToken cancellationToken = default)
        { 
            using var client =  _factory.CreateClient("RssCLient");
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
                            _logger.LogInformation("Поток пустой");
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Не удалось прочитать ответ" + ex.Message);
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
                _logger.LogError("Операция отменена" + ex.Message);
                return null;
            }
            catch (HttpRequestException ex)
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
