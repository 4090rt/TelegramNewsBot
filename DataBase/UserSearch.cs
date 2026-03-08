using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TelegramNewsBot.DataBase
{
    public class NoLockOptions
    {
        public bool NolockUsing { get; set; }
        public bool Logging { get; set; }
    }
    public class Data
    {
        public string UserName { get; set; }
        public DateTime DateLasdCommand { get; set; }
        public string Command { get; set; }
    }
    public class UserSearch
    {
        private readonly Microsoft.Extensions.Logging.ILogger<UserSearch> _logger;
        private bool _isvalid = false;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

        public UserSearch(Microsoft.Extensions.Logging.ILogger<UserSearch> logger, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
            Task.Run(async () => await inithialize()).ConfigureAwait(false);
        }

        public async Task inithialize()
        {
            if (_isvalid) return;

            if (_isvalid == false)
            {
                await cREATEINDEX();
                await ProverkaINdex();
            }
            _isvalid = true;
        }

        public async Task<List<Data>> CachedReq(string username, NoLockOptions options, int page = 1, int pagesize = 15)
        {
            string cached_key = $"hey_cache{username}";

            if (_cache.TryGetValue(cached_key, out object? cachedobject))
            {
                if (cachedobject is List<Data> cached)
                {
                    _logger.LogInformation($"📦 Данные из кэша для {cached}");
                    return cached;
                }
            }
            try
            {
                List<Data> list = await CachedReq(username, options, page, pagesize).ConfigureAwait(false);

                var optionsmemory = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cached_key, list, optionsmemory);
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение " + ex.Message + ex.StackTrace);
                return new List<Data>();
            }
        }

        public async Task<List<Data>> Request(string username, NoLockOptions options, int page = 1, int pagesize = 15)
        {
            int paginateFormul = (page -1) * pagesize;

            if (options == null)
            { 
                options = new NoLockOptions();
            }
            PollOpen pool = new PollOpen();
            SQLiteConnection con = null;
            try
            {
                con.Open();
                string command = "SELECT User, Date, Command, FROM COM WHERE User = @U ORDER BY Date DESC, Command ASC LIMIT @pahesize OFFSET @offset";
                var resultat = new List<Data>();
                var nolockoptions = options.NolockUsing ? "WITH(NOLOCK)" : "";

                if (options.Logging == true)
                {
                    _logger.LogInformation($"🔧 Query options: UseNoLock={options.NolockUsing}, Date={DateTime.UtcNow}");
                }

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using (var commandsql = new SQLiteCommand(command, con))
                {
                    commandsql.Parameters.AddWithValue("@U", username);

                    using (var resulta = await commandsql.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (resulta != null)
                        {
                            int rowcount = 0;
                            while (await resulta.ReadAsync())
                            {
                                string user = resulta.GetString(0);
                                DateTime date = resulta.GetDateTime(1);
                                string commanda = resulta.GetString(2);

                                var data = new Data()
                                {
                                    UserName = user,
                                    DateLasdCommand = date,
                                    Command = commanda
                                };
                                resultat.Add(data);
                            }
                            _logger.LogInformation($"найдено {rowcount} записей за {stopwatch.ElapsedMilliseconds}мс");
                        }
                        else
                        {
                            return new List<Data>();
                        }
                    }
                }
                stopwatch.Stop();
                return resultat;
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение " + ex.Message + ex.StackTrace);
                return new List<Data>();
            }
            finally
            {
                if (con != null)
                {
                    pool.ClosePool(con);
                }
            }
        }

        public async Task cREATEINDEX()
        {
            PollOpen pool = new PollOpen();
            SQLiteConnection con = null;
            try
            {
                con = pool.Pollopen();
                string command = "CREATE INDEX IF NOT EXISTS  IX_COM_User ON COM(User)";

                using (var commandsql = new SQLiteCommand(command, con))
                {
                    await commandsql.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogInformation("Индекс создан!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение " + ex.Message + ex.StackTrace);
            }
            finally
            {
                if (con != null)
                    pool.ClosePool(con);
            }
        }

        public async Task<bool> ProverkaINdex()
        {
            PollOpen pool = new PollOpen();
            SQLiteConnection con = null;
            try
            {
                con = pool.Pollopen();
                string command = "SELECT name FROM sqlite_master WHERE type = 'index' AND name = 'IX_COM_User'";

                using (var sqlcommand = new SQLiteCommand(command, con))
                { 
                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);

                    if (result != null && result != DBNull.Value)
                    {
                        bool exists = Convert.ToInt32(result) == 1;

                        if (exists)
                        {
                            _logger.LogInformation($"✅ Индекс '{result}' существует!");
                        }
                        else
                        {
                            _logger.LogInformation($"❌ Индекс 'IX_COM_DateLasdCommand' не найден");
                        }
                        return exists;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение " + ex.Message + ex.StackTrace);
                return false;
            }
            finally
            {
                if (con != null)
                    pool.ClosePool(con);
            }
        }
    }
}
