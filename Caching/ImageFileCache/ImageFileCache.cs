// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
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

namespace Caching
{
    /// <summary>
    /// ObjectCache implementation based on local image files.
    /// The only valid data type for cached values is a byte array containing an
    /// 8-byte timestamp followed by a PNG, JPEG, BMP, GIF, TIFF or WMP image buffer.
    /// </summary>
    public class ImageFileCache : ObjectCache
    {
        private static readonly Tuple<string, byte[]>[] imageFileTypes = new Tuple<string, byte[]>[]
        {
            new Tuple<string, byte[]>(".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0xD, 0xA, 0x1A, 0xA }),
            new Tuple<string, byte[]>(".jpg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, 0x4A, 0x46, 0x49, 0x46, 0 }),
            new Tuple<string, byte[]>(".bmp", new byte[] { 0x42, 0x4D }),
            new Tuple<string, byte[]>(".gif", new byte[] { 0x47, 0x49, 0x46 }),
            new Tuple<string, byte[]>(".tif", new byte[] { 0x49, 0x49, 42, 0 }),
            new Tuple<string, byte[]>(".tif", new byte[] { 0x4D, 0x4D, 0, 42 }),
            new Tuple<string, byte[]>(".wdp", new byte[] { 0x49, 0x49, 0xBC }),
        };

        private static readonly FileSystemAccessRule fullControlRule = new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
            FileSystemRights.FullControl, AccessControlType.Allow);

        private readonly string name;
        private readonly string directory;

        public ImageFileCache(string name, NameValueCollection config)
            : this(name, config["directory"])
        {
        }

        public ImageFileCache(string name, string directory)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The parameter name must not be null or empty or only white-space.");
            }

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("The parameter directory must not be null or empty or only white-space.");
            }

            this.name = name;
            this.directory = Path.Combine(directory, name.Trim());
            Directory.CreateDirectory(this.directory);

            Trace.TraceInformation("Created ImageFileCache in {0}", this.directory);
        }

        public override string Name
        {
            get { return name; }
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get { return DefaultCacheCapabilities.InMemoryProvider; }
        }

        public override object this[string key]
        {
            get { return Get(key); }
            set { Set(key, value, null); }
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotSupportedException("LocalFileCache does not support the ability to enumerate items.");
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotSupportedException("LocalFileCache does not support the ability to create change monitors.");
        }

        public override long GetCount(string regionName = null)
        {
            throw new NotSupportedException("LocalFileCache does not support the ability to count items.");
        }

        public override bool Contains(string key, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            try
            {
                return MemoryCache.Default.Contains(key) || FindFile(GetPath(key)) != null;
            }
            catch
            {
                return false;
            }
        }

        public override object Get(string key, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            var value = MemoryCache.Default.Get(key);

            if (value == null)
            {
                try
                {
                    var path = FindFile(GetPath(key));

                    if (path != null)
                    {
                        var creationTime = File.GetLastWriteTimeUtc(path).ToBinary();

                        using (var fileStream = new FileStream(path, FileMode.Open))
                        using (var memoryStream = new MemoryStream((int)(fileStream.Length + 8)))
                        {
                            memoryStream.Write(BitConverter.GetBytes(creationTime), 0, 8);
                            fileStream.CopyTo(memoryStream);
                            value = memoryStream.GetBuffer();
                        }
                    }
                }
                catch
                {
                }
            }

            return value;
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            var value = Get(key, regionName);
            return value != null ? new CacheItem(key, value) : null;
        }

        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            var values = new Dictionary<string, object>();

            foreach (string key in keys)
            {
                values[key] = Get(key);
            }

            return values;
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("The parameter regionName must be null.");
            }

            var buffer = value as byte[];

            if (buffer == null || buffer.Length <= 8)
            {
                throw new NotSupportedException("The parameter value must be a byte[] containing at least 9 bytes.");
            }

            MemoryCache.Default.Set(key, buffer, policy);

            var path = GetPath(key) + GetFileExtension(buffer);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    fileStream.Write(buffer, 8, buffer.Length - 8);
                }

                var fileSecurity = File.GetAccessControl(path);
                fileSecurity.AddAccessRule(fullControlRule);
                File.SetAccessControl(path, fileSecurity);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("ImageFileCache: Writing file {0} failed: {1}", path, ex.Message);
            }
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            Set(item.Key, item.Value, policy, item.RegionName);
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            Set(key, value, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, regionName);
        }

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            var oldValue = Get(key, regionName);
            Set(key, value, policy);
            return oldValue;
        }

        public override CacheItem AddOrGetExisting(CacheItem item, CacheItemPolicy policy)
        {
            var oldItem = GetCacheItem(item.Key, item.RegionName);
            Set(item, policy);
            return oldItem;
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            return AddOrGetExisting(key, value, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, regionName);
        }

        public override object Remove(string key, string regionName = null)
        {
            var oldValue = Get(key, regionName);
            MemoryCache.Default.Remove(key);

            try
            {
                var path = FindFile(GetPath(key));

                if (path != null)
                {
                    File.Delete(path);
                }
            }
            catch
            {
            }

            return oldValue;
        }

        private string GetPath(string key)
        {
            return Path.Combine(directory, key);
        }

        private static string FindFile(string path)
        {
            if (!string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                return path;
            }

            string directoryName = Path.GetDirectoryName(path);

            if (Directory.Exists(directoryName))
            {
                return Directory.EnumerateFiles(directoryName, Path.GetFileName(path) + ".*").FirstOrDefault();
            }

            return null;
        }

        private static string GetFileExtension(byte[] buffer)
        {
            var fileType = imageFileTypes.FirstOrDefault(t =>
            {
                int i = 0;

                if (t.Item2.Length <= buffer.Length - 8)
                {
                    while (i < t.Item2.Length && t.Item2[i] == buffer[i + 8])
                    {
                        i++;
                    }
                }

                return i == t.Item2.Length;
            });

            return fileType != null ? fileType.Item1 : ".bin";
        }
    }
}
