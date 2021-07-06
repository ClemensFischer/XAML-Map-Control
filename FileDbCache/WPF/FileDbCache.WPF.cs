// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;

namespace MapControl.Caching
{
    public partial class FileDbCache : ObjectCache
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
            throw new NotSupportedException("FileDbCache does not support the ability to enumerate items.");
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotSupportedException("FileDbCache does not support the ability to create change monitors.");
        }

        public override long GetCount(string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("FileDbCache does not support named regions.");
            }

            try
            {
                return fileDb.NumRecords;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FileDbCache.GetCount(): {ex.Message}");
            }

            return 0;
        }

        public override bool Contains(string key, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("FileDbCache does not support named regions.");
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            try
            {
                return fileDb.GetRecordByKey(key, Array.Empty<string>(), false) != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FileDbCache.Contains({key}): {ex.Message}");
            }

            return false;
        }

        public override object Get(string key, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("FileDbCache does not support named regions.");
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var record = GetRecordByKey(key);

            if (record == null)
            {
                return null;
            }

            return Tuple.Create((byte[])record[0], (DateTime)record[1]);
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
                throw new NotSupportedException("FileDbCache does not support named regions.");
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!(value is Tuple<byte[], DateTime> cacheItem))
            {
                throw new ArgumentException("The value argument must be a Tuple<byte[], DateTime>.", nameof(value));
            }

            AddOrUpdateRecord(key, cacheItem.Item1, cacheItem.Item2);
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
                    fileDb.DeleteRecordByKey(key);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"FileDbCache.Remove({key}): {ex.Message}");
                }
            }

            return oldValue;
        }
    }
}
