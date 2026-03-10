using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;

namespace TelegramNewsBot.TelegramBotSet.TelegramService
{
    public class Parametrs : EventArgs
    {
        public  Exception Exception { get; init; } = null;
        public string ExceptMessage { get; set; } = string.Empty;
    }
    public class EventErrorPolly
    {
        private readonly ILogger<EventErrorPolly> _logger;
        public EventHandler<Parametrs>? ErrorPolly;

        public EventErrorPolly(ILogger<EventErrorPolly> logger)
        { 
            _logger = logger;
        }

        public Task Errors(Exception exception)
        {
            string messeagerror;

            switch (exception)
            {
                case ApiRequestException apiRequestException:
                    messeagerror = $"Telegram API Error: {apiRequestException.ErrorCode} - {apiRequestException.Message}";
                    break;
                default:
                    messeagerror = exception.ToString();
                    break;
            }
            _logger.LogError(messeagerror);

            var args = new Parametrs
            {
                Exception = exception,
                ExceptMessage = messeagerror
            };

            ErrorPolly?.Invoke(this,args);

            return Task.CompletedTask;
        }
    }
}
