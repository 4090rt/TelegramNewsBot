using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNewsBot.DataBase
{
    public class PollOpen
    {
        private readonly Stack<SQLiteConnection> _available = new Stack<SQLiteConnection>();
        private readonly List<SQLiteConnection> _inUse = new List<SQLiteConnection>();
        private readonly object _lock = new object();
        private readonly string _dbpath;
        private readonly int _maxCouhnt = 10;
        private readonly ILogger _logger;
        public PollOpen(ILogger logger)
        { 
            DbPathClass dbpath = new DbPathClass();
            _dbpath = dbpath.dbpath(); 
            _logger = logger;
        }

        public SQLiteConnection CreateConnection()
        {
            try
            {
                var connect = new SQLiteConnection($"Data Source={_dbpath}");
                connect.Open();
                return connect;
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Возникло исключени" + ex.Message + ex.StackTrace);
                throw;
            }
        }

        public SQLiteConnection Pollopen()
        {
            try
            {
                lock (_lock)
                {
                    SQLiteConnection conn;

                    if (_available.Count > 1)
                    {
                        conn = _available.Pop();
                        if (conn == null)
                        {
                            conn = CreateConnection();
                        }
                        if (conn.State != System.Data.ConnectionState.Open)
                        {
                            conn.Dispose();
                            conn = CreateConnection();
                        }
                    }
                    else if (_available.Count + _inUse.Count < _maxCouhnt)
                    {
                        conn = CreateConnection();
                    }
                    else
                    {
                        throw new Exception("Пул занят!");
                    }

                    _inUse.Add(conn);
                    return conn;
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Возникло исключени" + ex.Message + ex.StackTrace);
                throw;
            }
        }

        public void ClosePool(SQLiteConnection conn)
        {
            if (conn == null) return;
            try
            {
                lock (_lock)
                {
                    if (_inUse.Contains(conn))
                    {
                        _inUse.Remove(conn);

                        if (conn.State == System.Data.ConnectionState.Open)
                        {
                            _available.Push(conn);
                        }
                        else
                        {
                            conn.Dispose();
                        }
                    }
                    else
                    {
                        throw new Exception("Соединение не найдено");
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Возникло исключение " + ex.Message + ex.StackTrace);
                throw;
            }
        }
    }
}
