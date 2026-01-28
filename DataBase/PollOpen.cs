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
        private readonly Stack<SQLiteConnection> _pool = new Stack<SQLiteConnection>();
        private readonly string _dbpath;
        private readonly int _maxCouhnt = 10;

        public PollOpen()
        { 
            DbPathClass dbpath = new DbPathClass();
            _dbpath = dbpath.dbpath(); 
        }

        public SQLiteConnection CreateConnection()
        {
            var connect = new SQLiteConnection($"Data Source={_dbpath}");
            connect.Open();
            return connect;
        }

        public SQLiteConnection Pollopen()
        {
            lock (_pool)
            {
                if (_pool.Count > 1)
                { 
                    _pool.Pop();
                }
            }
            return CreateConnection();
        }

        public void ClosePool(SQLiteConnection connection)
        {
            if (connection.State == System.Data.ConnectionState.Broken && connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Dispose();
                return;
            }
            lock (_pool)
            {
                if (_pool.Count < _maxCouhnt)
                {
                    _pool.Push(connection);
                }
                else
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
        


    }
}
