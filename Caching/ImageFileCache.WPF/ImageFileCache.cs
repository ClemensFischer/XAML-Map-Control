// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private static readonly Tuple<string, byte[]>[] imageFileTypes = new Tuple<string, byte[]>[]
        {
            new Tuple<string, byte[]>(".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            new Tuple<string, byte[]>(".jpg", new byte[] { 0xFF, 0xD8, 0xFF }),
            new Tuple<string, byte[]>(".bmp", new byte[] { 0x42, 0x4D }),
            new Tuple<string, byte[]>(".gif", new byte[] { 0x47, 0x49, 0x46 }),
            new Tuple<string, byte[]>(".tif", new byte[] { 0x49, 0x49, 0x2A, 0x00 }),
            new Tuple<string, byte[]>(".tif", new byte[] { 0x4D, 0x4D, 0x00, 0x2A }),
            new Tuple<string, byte[]>(".bin", new byte[] { }),
        };

        private static readonly FileSystemAccessRule fullControlRule = new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
            FileSystemRights.FullControl, AccessControlType.Allow);

        private readonly MemoryCache memoryCache = MemoryCache.Default;
        private readonly string name;
        private readonly string rootFolder;

        public ImageFileCache(string name, NameValueCollection config)
            : this(name, config["folder"])
        {
        }

        public ImageFileCache(string name, string folder)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The parameter name must not be null or empty.");
            }

            if (string.IsNullOrEmpty(folder))
            {
                throw new ArgumentException("The parameter folder must not be null or empty.");
            }

            this.name = name;
            rootFolder = Path.Combine(folder, name);
            Directory.CreateDirectory(rootFolder);

            Debug.WriteLine("Created ImageFileCache in " + rootFolder);
        }

        public override string Name
        {
            get { return name; }
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

            var path = Path.Combine(rootFolder, key)
                + imageFileTypes.First(t => t.Item2.SequenceEqual(buffer.Take(t.Item2.Length))).Item1;

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
            var path = Path.Combine(rootFolder, key);

            try
            {
                if (!string.IsNullOrEmpty(Path.GetExtension(path)))
                {
                    return path;
                }

                string folderName = Path.GetDirectoryName(path);

                if (Directory.Exists(folderName))
                {
                    return Directory.EnumerateFiles(folderName, Path.GetFileName(path) + ".*").FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageFileCache: Failed finding {0}: {1}", path, ex.Message);
            }

            return null;
        }
    }
}
