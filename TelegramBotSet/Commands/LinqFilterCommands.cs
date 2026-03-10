using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramNewsBot.DataBase;
using TelegramNewsBot.RequestAndParcing.RequestBse;

namespace TelegramNewsBot.TelegramBotSet.Commands
{
    public class LinqFilterCommands
    {
        private readonly ILogger _logger;
        private readonly DelegateFromBD _delegateFromBD;
        private readonly LINQ _lINQ;
        private readonly ExceptionClass _exceptionClass;
        public LinqFilterCommands(ILogger logger, DelegateFromBD delegateFromBD, LINQ lINQ, ExceptionClass exceptionClass)
        {
            _logger = logger;
            _delegateFromBD = delegateFromBD;
            _lINQ = lINQ;
            _exceptionClass = exceptionClass;
        }

        public async Task Command1()
        {
            try
            {
                await _delegateFromBD.DelegateRun(_delegateFromBD.DelegateRealiz, _lINQ.LinqRequest, "Команды за сегодня:");
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
            }
        }
        public async Task CommandsFromUser(string Username)
        {
            try
            {
                await _delegateFromBD.DelegateRunStr(_delegateFromBD.DelegateRealizeStr, 
                    async () => await _lINQ.LinqRequestUserC(Username),
                    $"Действия пользователя {Username} сегодня", Username);
            }
            catch(Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
            }
        }
        public async Task CommandFromWeekLast()
        {
            try
            {
                await _delegateFromBD.DelegateRun(_delegateFromBD.DelegateRealiz, async () => await _lINQ.LinqRequestFromWeek(), $"Действия пользователей за последнюю неделю");
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
            }
        }

        public async Task CommandFromLast10()
        {
            try
            {
                await _delegateFromBD.DelegateRun(_delegateFromBD.DelegateRealiz, async () => await _lINQ.LinqRequestLast10(), $"Последние 10 запросов");

            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
            }
        }

        public async Task CommandFroCommands(string command)
        {
            try
            {
                await _delegateFromBD.DelegateRunStr(_delegateFromBD.DelegateRealizeStr, 
                    async () => await _lINQ.LinqRequestFromCommand(command),
                    $"Последние использования команды {command}", command);
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
            }
        }

        public async Task CommandFro5Popular()
        {
            try
            {
                await _delegateFromBD.DelegateRun(_delegateFromBD.DelegateRealiz, async () => await _lINQ.LinqRequestFromPopularCommand(), $"Топ 5 популярных команд");
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
            }
        }

        public async Task CommandfROMLastAndFirst()
        {
            try
            {
                await _delegateFromBD.DelegateRun(_delegateFromBD.DelegateRealiz, async () => await _lINQ.LinqRequestFromFirstAndLast(), $"Последния и первая команда пользователя");
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
            }
        }
    }
}
