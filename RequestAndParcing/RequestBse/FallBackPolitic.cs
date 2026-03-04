using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Fallback;
using System.Collections.Generic;

namespace TelegramNewsBot.RequestAndParcing.RequestBse
{
    public delegate Task<T> Fallback<T>(T oldcache, string memorycache_key, CancellationToken cancellation = default);
    
    public class FallBackPolitic
    {
        private readonly ILogger<FallBackPolitic> _logger;
        private readonly IMemoryCache _memoryCache;

        public FallBackPolitic(ILogger<FallBackPolitic> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task<T> Proverka<T>(T oldcache, string memorycache_key, CancellationToken cancellation = default)
        {
            string stalecache = $"stalekey:{memorycache_key}";
            if (_memoryCache.TryGetValue(stalecache, out T? stalecached))
            {
                _logger.LogInformation($"✅ Returning stale copy for {stalecached}");
                return stalecached;
            }
            else
            {
                _logger.LogWarning("⚠️ Fallback: кэш пуст, возвращаю default");
                return default!;
            }
        }

        public AsyncFallbackPolicy<T> fallbackPolicyS<T>(Fallback<T> fallbackDelegate,T oldcache, string memorycache_key, CancellationToken cancellation = default)
        {
            var fallback = Policy<T>
                .Handle<Exception>()
                .OrResult(r => IsNullOrEmpty(r))
                .FallbackAsync(
                    fallbackAction: async (outcome, context, ctx) =>
                    {
                        var exception = outcome.Exception;
                        var isempty = IsNullOrEmpty(outcome.Result);

                        if (exception != null)
                        {
                            _logger.LogWarning($"⚠️ Fallback by exception: {exception.Message}");
                        }
                        if (isempty)
                        {
                            _logger.LogWarning($"⚠️ Fallback by empty result");
                        }
                        if (oldcache != null)
                        {
                            _logger.LogInformation("✅ Fallback: возвращаю старые данные из кэша");
                            return oldcache;
                        }
                        return await fallbackDelegate.Invoke(oldcache, memorycache_key, cancellation);
                    },
                    onFallbackAsync: async (outcome, ctx) =>
                    {
                        _logger.LogError($"🆘 Fallback сработал: {outcome.Exception?.Message}");
                        await Task.CompletedTask;
                    });
            
            return fallback;
        }

        public static bool IsNullOrEmpty<T>(T? result)
        {
            if (result == null) return true;
            
            if (result is System.Collections.IList list)
            {
                return list.Count == 0;
            }
            
            if (result is List<T> genericList)
            {
                return genericList.Count == 0;
            }
            
            return false;
        }
    }
}
