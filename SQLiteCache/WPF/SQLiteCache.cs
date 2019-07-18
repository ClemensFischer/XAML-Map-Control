// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;

namespace MapControl.Caching
{
    /// <summary>
    /// ObjectCache implementation based on SqLite.
    /// </summary>
    public sealed class SQLiteCache : ObjectCache, IDisposable
    {
        private readonly SQLiteConnection connection;

        public SQLiteCache(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("The parameter path must not be null or empty.");
            }

            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = Path.Combine(path, "TileCache.sqlite");
            }

            connection = new SQLiteConnection("Data Source=" + Path.GetFullPath(path));

            connection.Open();

            using (var command = new SQLiteCommand("create table if not exists items (key text, expiration integer, buffer blob)", connection))
            {
                command.ExecuteNonQuery();
            }
        }

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
            throw new NotSupportedException("SqLiteCache does not support the ability to enumerate items.");
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotSupportedException("SqLiteCache does not support the ability to create change monitors.");
        }

        public override long GetCount(string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
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
                Debug.WriteLine("SqLiteCache: GetCount(): {0}", ex.Message);
            }

            return 0;
        }

        public override bool Contains(string key, string regionName = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException("The parameter key must not be null.");
            }

            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            try
            {
                using (var command = new SQLiteCommand("select expiration, buffer from items where key=@key", connection))
                {
                    command.Parameters.AddWithValue("@key", key);

                    return command.ExecuteReader().Read();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SqLiteCache: Get(\"{0}\"): {1}", key, ex.Message);
            }

            return false;
        }

        public override object Get(string key, string regionName = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException("The parameter key must not be null.");
            }

            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            try
            {
                using (var command = new SQLiteCommand("select expiration, buffer from items where key=@key", connection))
                {
                    command.Parameters.AddWithValue("@key", key);
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        return new ImageCacheItem
                        {
                            Expiration = new DateTime((long)reader["expiration"]),
                            Buffer = (byte[])reader["buffer"]
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SqLiteCache: Get(\"{0}\"): {1}", key, ex.Message);
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
            if (key == null)
            {
                throw new ArgumentNullException("The parameter key must not be null.");
            }

            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            var imageCacheItem = value as ImageCacheItem;

            if (imageCacheItem == null || imageCacheItem.Buffer == null || imageCacheItem.Buffer.Length == 0)
            {
                throw new NotSupportedException("The parameter value must be an ImageCacheItem with a non-empty Buffer.");
            }

            try
            {
                using (var command = new SQLiteCommand("insert or replace into items (key, expiration, buffer) values (@key, @exp, @buf)", connection))
                {
                    command.Parameters.AddWithValue("@key", key);
                    command.Parameters.AddWithValue("@exp", imageCacheItem.Expiration.Ticks);
                    command.Parameters.AddWithValue("@buf", imageCacheItem.Buffer);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SqLiteCache: Set(\"{0}\"): {1}", key, ex.Message);
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
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
