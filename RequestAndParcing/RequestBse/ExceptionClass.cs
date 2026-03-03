using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramNewsBot.RequestAndParcing.ModelBse;

namespace TelegramNewsBot.RequestAndParcing.RequestBse
{
    public class ExceptionClass
    {
        private readonly Microsoft.Extensions.Logging.ILogger<ExceptionClass> _logger;
        
        public ExceptionClass(Microsoft.Extensions.Logging.ILogger<ExceptionClass> logger)
        {
            _logger = logger;
        }

        public void Send1(Action<Exception> action, Exception ex)
        {
            action?.Invoke(ex);
        }

        public void Exceptions(Exception ex)
        {
            switch(ex)
            {
                case HttpRequestException httpex:
                    _logger.LogError("Возникло исключение при выполнении запроса" + ex.Message, ex.StackTrace);
                    break;
                case TaskCanceledException cancelEx:
                    _logger.LogError("Операция отменена" + ex.Message + ex.StackTrace);
                    break;

                default:
                    _logger.LogError($"Общая ошибка: {ex.Message}");
                    break;
            }
        }
    }
}
