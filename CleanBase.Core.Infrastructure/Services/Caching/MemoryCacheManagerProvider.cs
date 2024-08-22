using CleanBase.Core.Settings;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.Services.Caching
{
    public class MemoryCacheManagerProvider : CacheManagerProvider
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheManagerProvider(AppSettings appSettings, IMemoryCache memoryCache)
            : base(appSettings)
        {
            _memoryCache = memoryCache;
        }

        public override object Set(object value, params string[] keys)
        {
            return this._memoryCache.Set(this.BuildKey(keys), value);
        }

        public override T Set<T>(T value, params string[] keys)
        {
            return this._memoryCache.Set<T>(this.BuildKey(keys), value);
        }

        public override object Get(object key) => this._memoryCache.Get(key);

        public override TItem Get<TItem>(object key) => this._memoryCache.Get<TItem>(key);

        public override Task<T> GetOrCreateAsync<T>(Func<Task<T>> func, params string[] keys)
        {
            long expiredTimeInMinute = AppSettings.Cache?.SlidingExpiration ?? 60L;
            return GetOrCreateAsync(func, expiredTimeInMinute, keys);
        }

        public override Task<T> GetOrCreateAsync<T>(
            Func<Task<T>> func,
            long expiredTimeInMinute,
            params string[] keys)
        {
            string cacheKey = BuildKey(keys);
            long expirationTime = Math.Max(1L, expiredTimeInMinute);

            return _memoryCache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(expirationTime);
                return func();
            });
        }

        public override bool TryGetValue(object key, out object value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }


    }
}
