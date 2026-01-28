using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.DataBase
{
    public class DbSaveCommands
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public DbSaveCommands(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }
        public async Task Addcommands(string command, string user, string date)
        {
            PollOpen open = new PollOpen();
            SQLiteConnection connection = null;
            try
            {
               _logger.LogInformation("Сохраняю.. ");
                connection = open.Pollopen();
                string sqlitecommand = "INSERT INTO [COM] (Command,User,Date) VALUES (@C, @U,@D)";

                using (var commands = new SQLiteCommand(sqlitecommand, connection))
                {
                    commands.Parameters.AddWithValue("@C", command);
                    commands.Parameters.AddWithValue("@U", user);
                    commands.Parameters.AddWithValue("@D", date);
                    await commands.ExecuteScalarAsync().ConfigureAwait(false);
                    _logger.LogInformation("Запись добавлена");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при попытке сохранения" + ex.Message);
                return;
            }
            finally 
            {
               open.ClosePool(connection);
            }
        }
    }
}
