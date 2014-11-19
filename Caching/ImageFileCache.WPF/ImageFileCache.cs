// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Media.Imaging;

namespace MapControl.Caching
{
    /// <summary>
    /// ObjectCache implementation based on local image files.
    /// The only valid data type for cached values is System.Windows.Media.Imaging.BitmapFrame.
    /// </summary>
    public class ImageFileCache : ObjectCache
    {
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
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The parameter name must not be null or empty or consist only of white-space characters.");
            }

            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new ArgumentException("The parameter folder must not be null or empty or consist only of white-space characters.");
            }

            this.name = name;
            rootFolder = Path.Combine(folder, name);
            Directory.CreateDirectory(rootFolder);

            Debug.WriteLine("Created ImageFileCache in {0}.", (object)rootFolder);
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
            return memoryCache.Contains(key, regionName) || FindFile(GetPath(key)) != null;
        }

        public override object Get(string key, string regionName = null)
        {
            var bitmap = memoryCache.Get(key, regionName) as BitmapFrame;

            if (bitmap == null)
            {
                try
                {
                    var path = FindFile(GetPath(key));

                    if (path != null)
                    {
                        using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            bitmap = BitmapFrame.Create(fileStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                            var metadata = (BitmapMetadata)bitmap.Metadata;
                            DateTime expiration;

                            // metadata.DateTaken must be parsed in CurrentCulture
                            if (metadata != null &&
                                metadata.DateTaken != null &&
                                DateTime.TryParse(metadata.DateTaken, CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out expiration))
                            {
                                memoryCache.Set(key, bitmap, expiration, regionName);
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            return bitmap;
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
            var bitmap = value as BitmapFrame;

            if (bitmap == null)
            {
                throw new ArgumentException("The parameter value must contain a System.Windows.Media.Imaging.BitmapFrame.");
            }

            var metadata = (BitmapMetadata)bitmap.Metadata;
            var format = metadata != null ? metadata.Format : "bmp";
            BitmapEncoder encoder = null;

            switch (format)
            {
                case "bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                case "gif":
                    encoder = new GifBitmapEncoder();
                    break;
                case "jpg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case "png":
                    encoder = new PngBitmapEncoder();
                    break;
                case "tiff":
                    encoder = new TiffBitmapEncoder();
                    break;
                case "wmphoto":
                    encoder = new WmpBitmapEncoder();
                    break;
                default:
                    break;
            }

            if (encoder == null)
            {
                throw new NotSupportedException(string.Format("The bitmap format {0} is not supported.", format));
            }

            memoryCache.Set(key, bitmap, policy, regionName);

            var path = string.Format("{0}.{1}", GetPath(key), format);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    encoder.Frames.Add(bitmap);
                    encoder.Save(fileStream);
                }

                var fileSecurity = File.GetAccessControl(path);
                fileSecurity.AddAccessRule(fullControlRule);
                File.SetAccessControl(path, fileSecurity);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageFileCache: Writing file {0} failed: {1}", path, ex.Message);
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

            memoryCache.Remove(key, regionName);

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
            return Path.Combine(rootFolder, key);
        }

        private static string FindFile(string path)
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

            return null;
        }
    }
}
