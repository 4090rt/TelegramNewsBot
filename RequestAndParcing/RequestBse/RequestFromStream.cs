using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelegramNewsBot.RequestAndParcing.ModelBse;
using TelegramNewsBot.RequestAndParcing.ParsedBase;

namespace TelegramNewsBot.RequestAndParcing.RequestBse
{
    public class RequestFromStream
    {
        private readonly Microsoft.Extensions.Logging.ILogger<RequestFromStream> _logger;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ParsedClass _parsedClass;
        private readonly Microsoft.Extensions.Logging.ILogger<ParsedClass> _loggerparsed;

        public RequestFromStream(Microsoft.Extensions.Logging.ILogger<RequestFromStream> logger, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, IHttpClientFactory httpClientFactory, ParsedClass parsedClass, Microsoft.Extensions.Logging.ILogger<ParsedClass> loggerparsed)
        {
            _logger = logger;
            _loggerparsed = loggerparsed;
            _memoryCache = memoryCache;
            _parsedClass = parsedClass;
            _httpClientFactory = httpClientFactory;
        }
        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        public async Task Request(string url, Func<List<ModelClassRss>, Task> onNewsReceived, CancellationToken cancellation = default)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Http2StreamClient");

                var options = new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Version = HttpVersion.Version20,
                    VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };

                _logger.LogInformation("Начинаю запрос");
                var timer = System.Diagnostics.Stopwatch.StartNew();
                using HttpResponseMessage recpon = await client.SendAsync(options, HttpCompletionOption.ResponseHeadersRead, cancellation).ConfigureAwait(false);
                timer.Stop();
                _logger.LogInformation($"Запрос завершен за {timer}" + recpon.StatusCode);
                if (recpon.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Читаю ответ");
                    await using var conent = await recpon.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    _logger.LogInformation("Ответ прочитан");

                    _logger.LogInformation("Начинаю парсинг");
                    var result = await _parsedClass.ParseRss(conent).ConfigureAwait(false);
                    _logger.LogInformation("Парсинг закончен");

                    cancellation.ThrowIfCancellationRequested();

                    await onNewsReceived(result);
                }
                else
                {
                    _logger.LogError("Запрос заверишлся ошибкой" + recpon.StatusCode);
                    return;
                }
            }
            catch (TaskCanceledException ex) when (!cancellation.IsCancellationRequested)
            {
                _logger.LogError("Запрос отменен" + ex.Message + ex.StackTrace);
                return;
            }
            catch (TaskCanceledException ex) when (cancellation.IsCancellationRequested)
            {
                _logger.LogError("Запрос отменен пользователем" + ex.Message + ex.StackTrace);
                return;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("При выполнении запроса возникло исключение" + ex.Message + ex.StackTrace);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return;
            }
        }
    }
}
