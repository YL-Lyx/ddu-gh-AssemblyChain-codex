using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AssemblyChain.Geometry.Toolkit.Utils
{
    /// <summary>
    /// 緩存管理器，提供高效的內存緩存支持
    /// </summary>
    public class CacheManager<TKey, TValue> : IDisposable
    {
        private readonly ConcurrentDictionary<TKey, CacheItem<TValue>> _cache;
        private readonly Timer _cleanupTimer;
        private readonly TimeSpan _defaultExpiration;
        private readonly int _maxSize;
        private bool _disposed = false;

        public CacheManager(TimeSpan defaultExpiration = default, int maxSize = 1000)
        {
            _defaultExpiration = defaultExpiration == default ? TimeSpan.FromMinutes(30) : defaultExpiration;
            _maxSize = Math.Max(1, maxSize);
            _cache = new ConcurrentDictionary<TKey, CacheItem<TValue>>();
            _cleanupTimer = new Timer(CleanupExpiredItems, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// 獲取緩存項目
        /// </summary>
        public TValue Get(TKey key)
        {
            if (_disposed) return default;
            if (_cache.TryGetValue(key, out var item))
            {
                if (!item.IsExpired)
                {
                    item.LastAccessed = DateTime.UtcNow;
                    return item.Value;
                }
                _cache.TryRemove(key, out _);
            }
            return default;
        }

        /// <summary>
        /// 設置緩存項目
        /// </summary>
        public void Set(TKey key, TValue value, TimeSpan? expiration = null)
        {
            if (_disposed) return;
            var expirationTime = expiration ?? _defaultExpiration;
            var item = new CacheItem<TValue>
            {
                Value = value,
                CreatedAt = DateTime.UtcNow,
                LastAccessed = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(expirationTime)
            };
            _cache.AddOrUpdate(key, item, (k, v) => item);
            if (_cache.Count > _maxSize) CleanupOldItems();
        }

        /// <summary>
        /// 獲取或創建緩存項目
        /// </summary>
        public TValue GetOrCreate(TKey key, Func<TValue> factory, TimeSpan? expiration = null)
        {
            var value = Get(key);
            if (!EqualityComparer<TValue>.Default.Equals(value, default)) return value;
            var newValue = factory();
            Set(key, newValue, expiration);
            return newValue;
        }

        /// <summary>
        /// 異步獲取或創建緩存項目
        /// </summary>
        public async Task<TValue> GetOrCreateAsync(TKey key, Func<Task<TValue>> factory, TimeSpan? expiration = null)
        {
            var value = Get(key);
            if (!EqualityComparer<TValue>.Default.Equals(value, default)) return value;
            var newValue = await factory().ConfigureAwait(false);
            Set(key, newValue, expiration);
            return newValue;
        }

        /// <summary>
        /// 移除緩存項目
        /// </summary>
        public bool Remove(TKey key)
        {
            if (_disposed) return false;
            return _cache.TryRemove(key, out _);
        }

        /// <summary>
        /// 清空所有緩存
        /// </summary>
        public void Clear()
        {
            if (_disposed) return;
            _cache.Clear();
        }

        /// <summary>
        /// 獲取緩存統計信息
        /// </summary>
        public CacheStatistics GetStatistics()
        {
            if (_disposed) return new CacheStatistics();
            var totalItems = _cache.Count;
            var expiredItems = _cache.Values.Count(item => item.IsExpired);
            var validItems = totalItems - expiredItems;
            return new CacheStatistics
            {
                TotalItems = totalItems,
                ValidItems = validItems,
                ExpiredItems = expiredItems,
                MaxSize = _maxSize,
                MemoryUsage = EstimateMemoryUsage()
            };
        }

        /// <summary>
        /// 清理過期項目
        /// </summary>
        private void CleanupExpiredItems(object state = null)
        {
            if (_disposed) return;
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.IsExpired)
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// 清理最舊的項目
        /// </summary>
        private void CleanupOldItems()
        {
            if (_disposed) return;
            int removeCount = Math.Max(0, _cache.Count - _maxSize);
            if (removeCount == 0) return;
            var itemsToRemove = _cache
                .OrderBy(kvp => kvp.Value.LastAccessed)
                .Take(removeCount)
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var key in itemsToRemove)
            {
                _cache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// 估算內存使用量
        /// </summary>
        private long EstimateMemoryUsage()
        {
            // 粗略估算：項目數量乘以項目結構大小
            int baseSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CacheItem<TValue>));
            return (long)_cache.Count * baseSize;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _cleanupTimer?.Dispose();
            _cache.Clear();
            _disposed = true;
        }
    }

    /// <summary>
    /// 緩存項目
    /// </summary>
    /// <typeparam name="T">值類型</typeparam>
    internal class CacheItem<T>
    {
        public T Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessed { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }

    /// <summary>
    /// 緩存統計信息
    /// </summary>
    public class CacheStatistics
    {
        public int TotalItems { get; set; }
        public int ValidItems { get; set; }
        public int ExpiredItems { get; set; }
        public int MaxSize { get; set; }
        public long MemoryUsage { get; set; }
        public double HitRate => TotalItems > 0 ? (double)ValidItems / TotalItems : 0;
    }

    /// <summary>
    /// 全局緩存管理器
    /// </summary>
    public static class GlobalCache
    {
        private static readonly Lazy<CacheManager<string, object>> _instance =
            new Lazy<CacheManager<string, object>>(() => new CacheManager<string, object>());

        public static CacheManager<string, object> Instance => _instance.Value;

        /// <summary>
        /// 獲取類型化的緩存管理器
        /// </summary>
        public static CacheManager<TKey, TValue> GetTypedCache<TKey, TValue>(TimeSpan? expiration = null, int? maxSize = null)
        {
            return new CacheManager<TKey, TValue>(
                expiration ?? TimeSpan.FromMinutes(30),
                maxSize ?? 1000
            );
        }
    }
}



