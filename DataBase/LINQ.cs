using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using TelegramNewsBot.RequestAndParcing.RequestBse;

namespace TelegramNewsBot.DataBase
{
    public class Users
    {
        public string Command { get; set; }
        public string User { get; set; }
        public DateTime Date { get; set; }
    }

    public class UserCommands
    {
        public string Command { get; set; }
        public string User { get; set; }
        public DateTime Date { get; set; }
    }

    public class PopularCommand
    {
        public string Command { get; set; }
        public int Count { get; set; }
    }

    public class UserActivity
    {
        public string User { get; set; }
        public string FirstCommand { get; set; }
        public string LastCommand { get; set; }
    }

    public class LINQ
    {
        private readonly ILogger _logger;
        private readonly PollOpen _open = new PollOpen();
        private readonly ExceptionClass _exceptionClass;

        public LINQ(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<Users>> LinqRequest()
        {
            SQLiteConnection connect = null;
            try
            {
                connect = _open.Pollopen();

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
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
                return new List<Users>();
            }
            finally
            {
                _open.ClosePool(connect);
            }
        }

        public async Task<List<UserCommands>> LinqRequestUserC(string username)
        {
            SQLiteConnection connect = null;
            try
            {
                connect = _open.Pollopen();

                string sql = "SELECT Command, User, Date FROM COM WHERE User = @User";

                var us = (await connect.QueryAsync<UserCommands>(sql, new { User = username })).ToList();

                var sor = from p in us
                          where p.User == username
                          select p;

                return sor.ToList();
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1 (_exceptionClass.Exceptions, ex);
                return new List<UserCommands>();
            }
            finally
            {
                _open.ClosePool(connect);
            }
        }

        public async Task<List<Users>> LinqRequestFromWeek()
        {
            SQLiteConnection connect = null;
            try
            {
                connect = _open.Pollopen();

                string sql = "SELECT Command, User, Date FROM COM";

                var us = (await connect.QueryAsync<Users>(sql)).ToList();

                var datetime = DateTime.Now.AddDays(-7);

                var sor = from p in us
                          where p.Date >= datetime
                          select p;

                return sor.ToList();
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
                return new List<Users>();
            }
            finally
            {
                _open.ClosePool(connect);
            }
        }

        public async Task<List<Users>> LinqRequestLast10()
        {
            SQLiteConnection connect = null;
            try
            {
                connect = _open.Pollopen();

                string sql = "SELECT Command, User, Date FROM COM";

                var us = (await connect.QueryAsync<Users>(sql)).ToList();

                var sor = (from p in us
                           orderby p.Date descending
                           select p).Take(10).ToList();

                return sor.ToList();
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
                return new List<Users>();
            }
            finally
            {
                _open.ClosePool(connect);
            }
        }

        public async Task<List<Users>> LinqRequestFromCommand(string command)
        {
            SQLiteConnection connect = null;
            try
            {
                connect = _open.Pollopen();

                string sql = "SELECT Command, User, Date FROM COM";

                var us = (await connect.QueryAsync<Users>(sql)).ToList();

                var sor = from p in us
                          where p.Command == command
                          select p;

                return sor.ToList();
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
                return new List<Users>();
            }
            finally
            {
                _open.ClosePool(connect);
            }
        }

        public async Task<List<PopularCommand>> LinqRequestFromPopularCommand()
        {
            SQLiteConnection connect = null;
            try
            {
                connect = _open.Pollopen();

                string sql = "SELECT Command, User, Date FROM COM";

                var us = (await connect.QueryAsync<Users>(sql)).ToList();

                var sor = (from p in us
                           group p by p.Command into g
                           orderby g.Count() descending
                           select new PopularCommand
                           {
                               Command = g.Key,
                               Count = g.Count()
                           }).Take(5).ToList();

                return sor.ToList();
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
                return new List<PopularCommand>();
            }
            finally
            {
                _open.ClosePool(connect);
            }
        }

        public async Task<List<UserActivity>> LinqRequestFromFirstAndLast()
        {
            SQLiteConnection connect = null;
            try
            {
                connect = _open.Pollopen();

                string sql = "SELECT Command, User, Date FROM COM";

                var us = (await connect.QueryAsync<Users>(sql)).ToList();

                var sor = (from p in us
                           group p by p.User into g
                           select new UserActivity
                           {
                               User = g.Key,
                               FirstCommand = g.OrderBy(u => u.Date).First().Command,
                               LastCommand = g.OrderByDescending(u => u.Date).First().Command
                           })
                           .ToList();

                return sor.ToList();
            }
            catch (Exception ex)
            {
                _exceptionClass.Send1(_exceptionClass.Exceptions, ex);
                return new List<UserActivity>();
            }
            finally
            {
                _open.ClosePool(connect);
            }
        }
    }
}
