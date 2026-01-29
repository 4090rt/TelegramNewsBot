using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.DataBase
{
    public class DateLast
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private bool _checkindex;

        public DateLast(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
            Task.Run(async () => await Inichialisate()).Wait();
        }

        public async Task Inichialisate()
        {
            if (_checkindex) return;

            await CreateIndex();
            await Indexproverka();
        }

        public async Task LastDateSeqscrh()
        {
            PollOpen open = new PollOpen();
            SQLiteConnection connect =  null;
            try
            {
                connect = open.Pollopen();
                string command = "SELECT  Command, User, Date FROM COM ORDER BY Date DESC";

                using (var sqlitecommand = new SQLiteCommand(command, connect))
                {
                    using (var result = await sqlitecommand.ExecuteReaderAsync())
                    {
                        if (result != null)
                        {
                            while (await result.ReadAsync())
                            {
                                string Command = result.GetString(0);
                                string User = result.GetString(1);
                                string Date = result.GetString(2);

                                _logger.LogInformation($"{Command}, {User}, {Date}");
                            }
                        }
                        else 
                        {
                            _logger.LogError("Ответ от бд пуст");
                            return;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"Возникло исключение при запросе" + ex.Message);
            }
            finally
            {
                open.ClosePool(connect);
            }
        }
        public async Task CreateIndex()
        {
            PollOpen open = new PollOpen();
            SQLiteConnection connect = null;
            try
            {
                connect=open.Pollopen();
                string command = "CREATE INDEX IF NOT EXISTS IX_COM_Date ON COM(Date)";

                using (var sqlcommand = new SQLiteCommand(command, connect))
                { 
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение при создании индекса");
            }
            finally
            {
                open.ClosePool(connect);
            }
        }

        public async Task Indexproverka()
        {
            PollOpen open = new PollOpen();
            SQLiteConnection connect = null;
            try
            {
                connect = open.Pollopen();
                string command = "SELECT name FROM sqlite_master WHERE type = 'index' AND name = 'IX_COM_Date'";

                using (var sqlcommand = new SQLiteCommand(command, connect))
                {
                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false) as string;
                    if (!string.IsNullOrEmpty(result))
                    {
                        _logger.LogInformation($"✅ Индекс '{result}' существует!");
                    }
                    else
                    {
                        _logger.LogInformation($"❌ Индекс 'IX_COM_Date' не найден");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение при проверке индекса!" + ex.Message);
            }
            finally
            {
                open.ClosePool(connect);
            }
        }
    }
}
