using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramNewsBot.RequestAndParcing.RequestBse;

namespace TelegramNewsBot.DataBase
{
    public class DelegateFromBD
    {
        private readonly ILogger _logger;
        private readonly ExceptionClass _exceptionClass;

        public delegate Task<T> Delegate<T>(Func<Task<T>> method, string str);
        public delegate Task<T> Delegatestr<T>(Func<Task<T>> method, string str, string znach);

        public DelegateFromBD(ILogger logger, ExceptionClass exceptionClass)
        {
            _logger = logger;
            _exceptionClass = exceptionClass;
        }

        public async Task<T> DelegateRun<T>(Delegate<T> delegat, Func<Task<T>> method, string str)
        {
            var result = await delegat.Invoke(method, str);
            return result;
        }

        public async Task<T> DelegateRunStr<T>(Delegatestr<T> delegat, Func<Task<T>> method, string str, string znach)
        {
            var result = await delegat.Invoke(method, str, znach);
            return result;
        }

        public async Task<T> DelegateRealiz<T>(Func<Task<T>> method, string str)
        {
            try
            {
                var result = await method().ConfigureAwait(false);

                _logger.LogWarning($"{str}");

                if (result is System.Collections.IEnumerable enumerable && result is not string)
                {
                    foreach (var item in enumerable)
                    {
                        _logger.LogWarning(item?.ToString());
                    }
                }
                else
                {
                    _logger.LogWarning(result?.ToString());
                }
                return result;
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
                return default;
            }
        }

        public async Task<T> DelegateRealizeStr<T>(Func<Task<T>> method, string str, string znach)
        {
            try
            {
                var result = await method().ConfigureAwait(false);

                _logger.LogWarning($"{str}");

                if (result is System.Collections.IEnumerable enumerable && result is not string)
                {
                    foreach (var item in enumerable)
                    {
                        _logger.LogWarning(item?.ToString());
                    }
                }
                else
                {
                    _logger.LogWarning(result?.ToString());
                }
                return result;
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
                return default;
            }
        }
    }
}
