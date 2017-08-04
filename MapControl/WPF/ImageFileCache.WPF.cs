// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
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
    /// <summary>
    /// ObjectCache implementation based on local image files.
    /// The only valid data type for cached values is byte[].
    /// </summary>
    public class ImageFileCache : ObjectCache
    {
        private static readonly FileSystemAccessRule fullControlRule = new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
            FileSystemRights.FullControl, AccessControlType.Allow);

        private readonly MemoryCache memoryCache = MemoryCache.Default;
        private readonly string rootFolder;

        public ImageFileCache(string rootFolder)
        {
            if (string.IsNullOrEmpty(rootFolder))
            {
                throw new ArgumentException("The parameter rootFolder must not be null or empty.");
            }

            this.rootFolder = rootFolder;

            Debug.WriteLine("Created ImageFileCache in " + rootFolder);
        }

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
            if (key == null)
            {
                throw new ArgumentNullException("The parameter key must not be null.");
            }

            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            return memoryCache.Contains(key) || FindFile(key) != null;
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

            var buffer = memoryCache.Get(key) as byte[];

            if (buffer == null)
            {
                var path = FindFile(key);

                if (path != null)
                {
                    try
                    {
                        //Debug.WriteLine("ImageFileCache: Reading " + path);
                        buffer = File.ReadAllBytes(path);
                        memoryCache.Set(key, buffer, new CacheItemPolicy());
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("ImageFileCache: Failed reading {0}: {1}", path, ex.Message);
                    }
                }
            }

            return buffer;
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

            var buffer = value as byte[];

            if (buffer == null || buffer.Length == 0)
            {
                throw new NotSupportedException("The parameter value must be a non-empty byte array.");
            }

            memoryCache.Set(key, buffer, policy);

            var path = GetPath(key);

            if (path != null)
            {
                try
                {
                    //Debug.WriteLine("ImageFileCache: Writing {0}, Expires {1}", path, policy.AbsoluteExpiration.DateTime.ToLocalTime());
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllBytes(path, buffer);

                    var fileSecurity = File.GetAccessControl(path);
                    fileSecurity.AddAccessRule(fullControlRule);
                    File.SetAccessControl(path, fileSecurity);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ImageFileCache: Failed writing {0}: {1}", path, ex.Message);
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
            if (key == null)
            {
                throw new ArgumentNullException("The parameter key must not be null.");
            }

            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            memoryCache.Remove(key);

            var path = FindFile(key);

            if (path != null)
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ImageFileCache: Failed removing {0}: {1}", path, ex.Message);
                }
            }

            return null;
        }

        private string FindFile(string key)
        {
            var path = GetPath(key);

            try
            {
                if (path != null && File.Exists(path))
                {
                    return path;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageFileCache: Failed finding {0}: {1}", path, ex.Message);
            }

            return null;
        }

        private string GetPath(string key)
        {
            try
            {
                return Path.Combine(rootFolder, Path.Combine(key.Split('\\', '/', ':', ';')));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageFileCache: Invalid key {0}/{1}: {2}", rootFolder, key, ex.Message);
            }

            return null;
        }
    }
}
