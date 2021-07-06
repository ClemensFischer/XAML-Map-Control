// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.AccessControl;
using System.Security.Principal;

namespace MapControl.Caching
{
    public partial class ImageFileCache : ObjectCache
    {
        private static readonly FileSystemAccessRule fullControlRule = new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
            FileSystemRights.FullControl, AccessControlType.Allow);

        private readonly MemoryCache memoryCache = MemoryCache.Default;

        public override string Name
        {
            get { return string.Empty; }
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get { return DefaultCacheCapabilities.None; }
        }

        public override object this[string key]
        {
            get { return Get(key); }
            set { Set(key, value, null); }
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotSupportedException("ImageFileCache does not support the ability to enumerate items.");
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotSupportedException("ImageFileCache does not support the ability to create change monitors.");
        }

        public override long GetCount(string regionName = null)
        {
            throw new NotSupportedException("ImageFileCache does not support the ability to count items.");
        }

        public override bool Contains(string key, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("ImageFileCache does not support named regions.");
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (memoryCache.Contains(key))
            {
                return true;
            }

            var path = GetPath(key);

            try
            {
                return path != null && File.Exists(path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageFileCache: Failed finding {path}: {ex.Message}");
            }

            return false;
        }

        public override object Get(string key, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("ImageFileCache does not support named regions.");
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var cacheItem = memoryCache.Get(key) as Tuple<byte[], DateTime>;

            if (cacheItem == null)
            {
                var path = GetPath(key);

                try
                {
                    if (path != null && File.Exists(path))
                    {
                        var buffer = File.ReadAllBytes(path);
                        var expiration = ReadExpiration(ref buffer);

                        cacheItem = new Tuple<byte[], DateTime>(buffer, expiration);

                        memoryCache.Set(key, cacheItem, new CacheItemPolicy { AbsoluteExpiration = expiration });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ImageFileCache: Failed reading {path}: {ex.Message}");
                }
            }

            return cacheItem;
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
                throw new NotSupportedException("ImageFileCache does not support named regions.");
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!(value is Tuple<byte[], DateTime> cacheItem))
            {
                throw new ArgumentException("The value argument must be a Tuple<byte[], DateTime>.", nameof(value));
            }

            memoryCache.Set(key, cacheItem, policy);

            var buffer = cacheItem.Item1;
            var path = GetPath(key);

            if (buffer != null && buffer.Length > 0 && path != null)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using (var stream = File.Create(path))
                    {
                        stream.Write(buffer, 0, buffer.Length);
                        WriteExpiration(stream, cacheItem.Item2);
                    }

                    var fileInfo = new FileInfo(path);
                    var fileSecurity = fileInfo.GetAccessControl();
                    fileSecurity.AddAccessRule(fullControlRule);
                    fileInfo.SetAccessControl(fileSecurity);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ImageFileCache: Failed writing {path}: {ex.Message}");
                }
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
            if (regionName != null)
            {
                throw new NotSupportedException("ImageFileCache does not support named regions.");
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            memoryCache.Remove(key);

            var path = GetPath(key);

            try
            {
                if (path != null && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageFileCache: Failed removing {path}: {ex.Message}");
            }

            return null;
        }
    }
}
