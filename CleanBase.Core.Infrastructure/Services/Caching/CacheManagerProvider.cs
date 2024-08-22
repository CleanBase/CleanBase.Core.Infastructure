using CleanBase.Core.Services.Core.Base;
using CleanBase.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Infrastructure.Services.Caching
{
	public abstract class CacheManagerProvider : ICacheManagerProvider
	{
		public AppSettings AppSettings { get; set; }

		public string Seperator => ".";

		public CacheManagerProvider(AppSettings appSettings) => AppSettings = appSettings;

		public string BuildKey(params string[] keys)
		{
			List<string> values = new List<string>();
			values.Add(AppSettings.AppId);
			values.AddRange(keys);

			return string.Join(Seperator, values);
		}

		public abstract Task<T> GetOrCreateAsync<T>(Func<Task<T>> func, params string[] keys);

		public abstract Task<T> GetOrCreateAsync<T>(
			Func<Task<T>> func,
			long expiredTimeInMinute,
			params string[] keys);

		public abstract bool TryGetValue(object key, out object value);

		public abstract object Get(object key);
		public abstract TItem Get<TItem>(object key);
		public abstract object Set(object value, params string[] keys);

		public abstract T Set<T>(T value, params string[] keys);
	}
}
