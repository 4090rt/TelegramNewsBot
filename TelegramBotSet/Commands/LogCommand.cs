using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TelegramNewsBot.DataBase;
using TelegramNewsBot.RequestAndParcing.RequestBse;
namespace TelegramNewsBot.TelegramBotSet.Commands
{
    public class Parametrs : EventArgs
    { 
        public string String1 { get; set; }
        public string String2 { get; set; }
        public DateTime date1 { get; set; }
    }

    public class LogCommand
    {
        public event EventHandler<Parametrs> LogginginBd;
        private readonly ILogger<LogCommand> _logger;
        private readonly ExceptionClass _exception;
        public LogCommand(ILogger<LogCommand> logger, ExceptionClass exception)
        {
            _logger = logger;
            _exception = exception;
        }
        public async Task SaveIndBd(string str1, string str2, DateTime dateTime)
        {
            try
            {
                _logger.LogInformation(str1);
                DbSaveCommands save3 = new DbSaveCommands(_logger);
                await save3.Addcommands(str1, str2, dateTime.ToString());

                OnLiggingPower(str1,str2,dateTime);
                _logger.LogInformation("✅ Событие LogginginBd вызвано");
            }
            catch (Exception ex)
            {
                _exception.Send1(_exception.Exceptions, ex);
            }
        }

        public void OnLiggingPower(string str1, string str2, DateTime dateTime)
        {
            var args = new Parametrs
            {
                String1 = str1,
                String2 = str2,
                date1 = dateTime
            };

            LogginginBd?.Invoke(this, args);
        }
    }
}
