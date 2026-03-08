using Dapper;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.DataBase
{
    public class Users
    { 
        public string Command { get; set; }
        public string User { get; set; }
        public DateTime Date { get; set; }
    }

    public class LINQ
    {
        private readonly ILogger _logger;

        public LINQ(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<Users>> LinqRequest()
        {
            PollOpen open = new PollOpen();
            SQLiteConnection connect = null;
            try
            {
                connect = open.Pollopen();

                string sql = "SELECT Command, User, Date FROM COM";

                var us = (await connect.QueryAsync<Users>(sql)).ToList();

                var today = DateTime.Today;
                var sort = from p in us
                           where p.Date.Date == today
                           select p;

                return sort.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Возникло исключение" + ex.Message + ex.StackTrace);
                return new List<Users>();
            }
            finally
            {
                open.ClosePool(connect);
            }
        }
    }
}
