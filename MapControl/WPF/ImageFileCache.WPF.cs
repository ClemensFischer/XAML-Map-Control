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
using System.Text;
using System.Threading.Tasks;

namespace MapControl.Caching
{
    /// <summary>
    /// ObjectCache implementation based on local image files.
    /// The only valid data type for cached values is MapControl.ImageCacheItem.
    /// </summary>
    public class ImageFileCache : ObjectCache
    {
        private const string ExpiresTag = "EXPIRES:";

        private static readonly FileSystemAccessRule fullControlRule = new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
            FileSystemRights.FullControl, AccessControlType.Allow);

        private readonly MemoryCache memoryCache = MemoryCache.Default;
        private readonly string rootDirectory;

        public ImageFileCache(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("The parameter directory must not be null or empty.");
            }

            rootDirectory = directory;
        }

        public Task Clean()
        {
            return Task.Factory.StartNew(CleanRootDirectory, TaskCreationOptions.LongRunning);
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
            if (regionName != null)
            {
                throw new NotSupportedException("ImageFileCache does not support named regions.");
            }

            if (key == null)
            {
                throw new ArgumentNullException("The parameter key must not be null.");
            }

            return memoryCache.Contains(key) || FindFile(key) != null;
        }

        public override object Get(string key, string regionName = null)
        {
            if (regionName != null)
            {
                throw new NotSupportedException("ImageFileCache does not support named regions.");
            }

            if (key == null)
            {
                throw new ArgumentNullException("The parameter key must not be null.");
            }

            var imageCacheItem = memoryCache.Get(key) as ImageCacheItem;

            if (imageCacheItem == null)
            {
                var path = FindFile(key);

                if (path != null)
                {
                    try
                    {
                        var buffer = File.ReadAllBytes(path);
                        var expiration = GetExpiration(ref buffer);

                        imageCacheItem = new ImageCacheItem
                        {
                            Buffer = buffer,
                            Expiration = expiration
                        };

                        memoryCache.Set(key, imageCacheItem, new CacheItemPolicy { AbsoluteExpiration = expiration });

                        //Debug.WriteLine("ImageFileCache: Reading {0}, Expires {1}", path, imageCacheItem.Expiration.ToLocalTime());
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("ImageFileCache: Failed reading {0}: {1}", path, ex.Message);
                    }
                }
            }

            return imageCacheItem;
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
                throw new ArgumentNullException("The parameter key must not be null.");
            }

            if (!(value is ImageCacheItem imageCacheItem))
            {
                throw new ArgumentException("The parameter value must be a MapControl.Caching.ImageCacheItem instance.");
            }

            memoryCache.Set(key, imageCacheItem, policy);

            string path;

            if (imageCacheItem.Buffer != null &&
                imageCacheItem.Buffer.Length > 0 &&
                (path = GetPath(key)) != null)
            {
                try
                {
                    //Debug.WriteLine("ImageFileCache: Writing {0}, Expires {1}", path, imageCacheItem.Expiration.ToLocalTime());

                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using (var stream = File.Create(path))
                    {
                        stream.Write(imageCacheItem.Buffer, 0, imageCacheItem.Buffer.Length);
                        SetExpiration(stream, imageCacheItem.Expiration);
                    }

                    var fileInfo = new FileInfo(path);
                    var fileSecurity = fileInfo.GetAccessControl();
                    fileSecurity.AddAccessRule(fullControlRule);
                    fileInfo.SetAccessControl(fileSecurity);
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
            if (regionName != null)
            {
                throw new NotSupportedException("ImageFileCache does not support named regions.");
            }

            if (key == null)
            {
                throw new ArgumentNullException("The parameter key must not be null.");
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
                return Path.Combine(rootDirectory, Path.Combine(key.Split('/', ':', ';', ',')));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageFileCache: Invalid key {0}/{1}: {2}", rootDirectory, key, ex.Message);
            }

            return null;
        }

        private async Task CleanRootDirectory()
        {
            foreach (var dir in new DirectoryInfo(rootDirectory).EnumerateDirectories())
            {
                var deletedFileCount = await CleanDirectory(dir).ConfigureAwait(false);

                if (deletedFileCount > 0)
                {
                    Debug.WriteLine("ImageFileCache: Cleaned {0} files in {1}", deletedFileCount, dir);
                }
            }
        }

        private static async Task<int> CleanDirectory(DirectoryInfo directory)
        {
            var deletedFileCount = 0;

            foreach (var dir in directory.EnumerateDirectories())
            {
                deletedFileCount += await CleanDirectory(dir).ConfigureAwait(false);
            }

            foreach (var file in directory.EnumerateFiles())
            {
                try
                {
                    if (await ReadExpirationAsync(file).ConfigureAwait(false) < DateTime.UtcNow)
                    {
                        file.Delete();
                        deletedFileCount++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ImageFileCache: Failed cleaning {0}: {1}", file.FullName, ex.Message);
                }
            }

            if (!directory.EnumerateFileSystemInfos().Any())
            {
                try
                {
                    directory.Delete();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ImageFileCache: Failed cleaning {0}: {1}", directory.FullName, ex.Message);
                }
            }

            return deletedFileCount;
        }

        private static void SetExpiration(Stream stream, DateTime expiration)
        {
            stream.Write(Encoding.ASCII.GetBytes(ExpiresTag), 0, 8);
            stream.Write(BitConverter.GetBytes(expiration.Ticks), 0, 8);
        }

        private static DateTime GetExpiration(ref byte[] buffer)
        {
            DateTime expiration = DateTime.MaxValue;

            if (buffer.Length > 16 && Encoding.ASCII.GetString(buffer, buffer.Length - 16, 8) == ExpiresTag)
            {
                expiration = new DateTime(BitConverter.ToInt64(buffer, buffer.Length - 8), DateTimeKind.Utc);
                Array.Resize(ref buffer, buffer.Length - 16);
            }

            return expiration;
        }

        private static async Task<DateTime> ReadExpirationAsync(FileInfo file)
        {
            DateTime expiration = DateTime.MaxValue;

            if (file.Length > 16)
            {
                var buffer = new byte[16];

                using (var stream = file.OpenRead())
                {
                    stream.Seek(-16, SeekOrigin.End);

                    if (await stream.ReadAsync(buffer, 0, 16).ConfigureAwait(false) == 16 &&
                        Encoding.ASCII.GetString(buffer, 0, 8) == ExpiresTag)
                    {
                        expiration = new DateTime(BitConverter.ToInt64(buffer, 8), DateTimeKind.Utc);
                    }
                }
            }

            return expiration;
        }
    }
}
