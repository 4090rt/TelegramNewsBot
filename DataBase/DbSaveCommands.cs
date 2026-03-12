using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.DataBase
{
    public class DbSaveCommands
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly PollOpen _poolopen;

        public DbSaveCommands(Microsoft.Extensions.Logging.ILogger logger, PollOpen poolopen)
        {
            _logger = logger;
            _poolopen = poolopen;
        }
        public async Task Addcommands(IEnumerable <(string command, string user, string date)> values)
        {
            if (values == null || !values.Any()) return;

            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
               _logger.LogInformation("Сохраняю.. ");
                _logger.LogInformation($"Пакетное сохранение {values.Count()} значений...");
               
                connection = _poolopen.Pollopen();
                transaction = connection.BeginTransaction();
                string sqlitecommand = "INSERT INTO [COM] (Command,User,Date) VALUES (@C, @U,@D)";

                using (var commands = new SQLiteCommand(sqlitecommand, connection, transaction))
                {
                    commands.Parameters.Add("@C", DbType.String);
                    commands.Parameters.Add("@U", DbType.String);
                    commands.Parameters.Add("@D", DbType.DateTime);
                    foreach (var command in values)
                    {
                        commands.Parameters.AddWithValue("@C", command.command);
                        commands.Parameters.AddWithValue("@U", command.user);
                        commands.Parameters.AddWithValue("@D", command.date);
                        await commands.ExecuteScalarAsync().ConfigureAwait(false);
                    }
                }
                transaction.Commit();
                _logger.LogInformation($"Успешно добавлено {values.Count()} записей");
            }
            catch (SQLiteException ex)
            {
                transaction?.Rollback();
                _logger.LogError($"Возникло исключение при работе с БД" + ex.Message + ex.StackTrace);
                return;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                _logger.LogError("Ошибка при попытке сохранения" + ex.Message);
                return;
            }
            finally 
            {
                transaction?.Dispose();
               _poolopen.ClosePool(connection);
            }
        }
    }
}
