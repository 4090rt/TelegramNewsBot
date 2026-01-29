using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.DataBase
{
    public class UserSearchCommand
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private bool _isInitialized = false;
        public UserSearchCommand(Microsoft.Extensions.Logging.ILogger logger)
        { 
            _logger = logger;
        }

        public async Task Inichializate()
        {
            if (_isInitialized) return;
            await IndexCreate();
            await Indexproverka();
        }

        public async Task Command(string username)
        {
            PollOpen open = new PollOpen();
            SQLiteConnection conection = null;
            if (username != null)
            {
                try
                {
                    Task.Run(async () => await Inichializate()).Wait();
                    conection = open.Pollopen();
                    string sqlitecommand = "SELECT  Command, User, Date  FROM COM WHERE User = @U";

                    using (var commandsql = new SQLiteCommand(sqlitecommand, conection))
                    {
                        commandsql.Parameters.AddWithValue("@U", username);
                        using (var result = await commandsql.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            if (result != null)
                            {
                                while (await result.ReadAsync())
                                {
                                    string usernames = result.GetString(0);
                                    string comand = result.GetString(1);
                                    string date = result.GetString(2);

                                    _logger.LogInformation($"{usernames}, {comand}, {date}");
                                }
                            }
                            else
                            {
                                _logger.LogError("Ответ от базы пуст");
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Возникло исключение при запросе действий юзера {username}" + ex.Message);
                }
                finally
                {
                    open.ClosePool(conection);
                }
            }
            else
            { 
                return;
            }
        }

        public async Task IndexCreate()
        {
            PollOpen open = new PollOpen();
            SQLiteConnection conection = null;  
            try
            {
                conection=open.Pollopen();
                string command = "CREATE INDEX IF NOT EXISTS IX_COM_User ON COM(User)";

                using (var sqlitecommand = new SQLiteCommand(command, conection))
                {
                    await sqlitecommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogInformation("Индекс создан!");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Возникло исключение при создании индекса" + ex.Message);
                return;
            }
            finally
            {
                open.ClosePool(conection);
            }
        }

        public async Task Indexproverka()
        {
            PollOpen open = new PollOpen();
            SQLiteConnection conection = null;
            try
            {
                conection = open.Pollopen();
                string command = "SELECT name FROM sqlite_master WHERE type = 'index' AND name = 'IX_COM_User'";

                using (var sqlitecommand = new SQLiteCommand(command, conection))
                {
                    var result = await sqlitecommand.ExecuteScalarAsync().ConfigureAwait(false) as string;
                    if (!string.IsNullOrEmpty(result))
                    {
                        _logger.LogInformation($"✅ Индекс '{result}' существует!");
                    }
                    else
                    {
                        _logger.LogInformation($"❌ Индекс 'IX_COM_User' не найден");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение при проверке индекса!" + ex.Message);
            }
            finally
            {
                open.ClosePool(conection);
            }
        }
    }
}
