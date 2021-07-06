// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;

namespace MapControl.Caching
{
    public partial class SQLiteCache : ObjectCache
    {
        public override string Name
        {
            get { return string.Empty; }
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get { return DefaultCacheCapabilities.AbsoluteExpirations | DefaultCacheCapabilities.SlidingExpirations; }
        }

        public override object this[string key]
        {
            get { return Get(key); }
            set { Set(key, value, null); }
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotSupportedException("SQLiteCache does not support the ability to enumerate items.");
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotSupportedException("SQLiteCache does not support the ability to create change monitors.");
        }

        public override long GetCount(string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("SQLiteCache does not support named regions.");
            }

            try
            {
                using (var command = new SQLiteCommand("select count(*) from items", connection))
                {
                    return (long)command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SQLiteCache.GetCount(): {ex.Message}");
            }

            return 0;
        }

        public override bool Contains(string key, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("SQLiteCache does not support named regions.");
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            try
            {
                using (var command = GetItemCommand(key))
                {
                    return command.ExecuteReader().Read();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SQLiteCache.Contains({key}): {ex.Message}");
            }

            return false;
        }

        public override object Get(string key, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("SQLiteCache does not support named regions.");
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            try
            {
                using (var command = GetItemCommand(key))
                {
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        return Tuple.Create((byte[])reader["buffer"], new DateTime((long)reader["expiration"]));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SQLiteCache.Get({key}): {ex.Message}");
            }

            return null;
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            var value = Get(key, regionName);

            return value != null ? new CacheItem(key, value) : null;
        }

        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            return keys.ToDictionary(key => key, key => Get(key, regionName));
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("SQLiteCache does not support named regions.");
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!(value is Tuple<byte[], DateTime> cacheItem))
            {
                throw new ArgumentException("The value argument must be a Tuple<byte[], DateTime>.", nameof(value));
            }

            try
            {
                using (var command = SetItemCommand(key, cacheItem.Item1, cacheItem.Item2))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SQLiteCache.Set({key}): {ex.Message}");
            }
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            Set(key, value, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, regionName);
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            Set(item.Key, item.Value, policy, item.RegionName);
        }

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            var oldValue = Get(key, regionName);

            Set(key, value, policy);

            return oldValue;
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            return AddOrGetExisting(key, value, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, regionName);
        }

        public override CacheItem AddOrGetExisting(CacheItem item, CacheItemPolicy policy)
        {
            var oldItem = GetCacheItem(item.Key, item.RegionName);

            Set(item, policy);

            return oldItem;
        }

        public override object Remove(string key, string regionName = null)
        {
            var oldValue = Get(key, regionName);

            if (oldValue != null)
            {
                try
                {
                    using (var command = RemoveItemCommand(key))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SQLiteCache.Remove({key}): {ex.Message}");
                }
            }

            return oldValue;
        }
    }
}
