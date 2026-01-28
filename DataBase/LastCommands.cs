using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.DataBase
{
    public class LastCommands
    {
        Microsoft.Extensions.Logging.ILogger _logger;
        private bool _isInitialized = false;
        public LastCommands(Microsoft.Extensions.Logging.ILogger logger) 
        {
            _logger = logger;
            Task.Run(async () => await InitializeAsync()).Wait();
        }
        public async Task InitializeAsync()
        {
            if (_isInitialized) return;
            await Index();
            await indexproverka();
        }
        public async Task LastnCommand()
        {
            SQLiteConnection connection = null;
            PollOpen OPEN = new PollOpen();

            try
            {
                connection = OPEN.CreateConnection();
                string command = "SELECT Command, User, Date FROM COM WHERE Command = @C";

                using (var commandsql = new SQLiteCommand(command, connection))
                {
                    string c = "Вызов команды /Weather";
                    commandsql.Parameters.AddWithValue("@C",c);

                    using (var reader = await commandsql.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync())
                        {
                            string commandf = reader.GetString(0);
                            string user = reader.GetString(1);
                            string date = reader.GetString(2);

                            _logger.LogInformation($"Запись: '{commandf}' от {user} в {date}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при попытке запроса" + ex.Message);
            }
            finally
            {
                OPEN.ClosePool(connection);
            }
        }

        public async Task Index()
        {
            SQLiteConnection connection = null;
            PollOpen OPEN = new PollOpen();
            try
            {
                connection = OPEN.Pollopen();

                using (var command = new SQLiteCommand("CREATE INDEX IF NOT EXISTS IX_COM_Command ON COM(Command)", connection))
                {
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение при создании индекса" + ex.Message);
                return;
            }
            finally
            {
                OPEN.ClosePool(connection);
            }
        }

        public async Task indexproverka()
        {
            SQLiteConnection connection = null;
            PollOpen OPEN = new PollOpen();
            try
            {
                connection = OPEN.Pollopen();

                using (var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type = 'index' AND name = 'IX_COM_Command'", connection))
                {
                    var result = await command.ExecuteScalarAsync().ConfigureAwait(false) as string;

                    if (!string.IsNullOrEmpty(result)) 
                    {
                        _logger.LogInformation($"✅ Индекс '{result}' существует!"); 
                    }
                    else
                    {
                        _logger.LogInformation($"❌ Индекс 'IX_COM_Command' не найден");
                    }
                }
            }
            catch (Exception EX)
            {
                _logger.LogError("Возникло исключение при проверке индекса" + EX.Message);
                return;
            }
            finally
            {
                OPEN.ClosePool(connection);
            }
        }
    }
}
