// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MapControl.Caching
{
    public class ImageFileCache : IImageCache
    {
        private const string expiresTag = "EXPIRES:";

        private readonly string rootDirectory;

        public ImageFileCache(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("The directory argument must not be null or empty.", nameof(directory));
            }

            rootDirectory = directory;
            Debug.WriteLine("Created ImageFileCache in " + rootDirectory);
        }

        public async Task<ImageCacheItem> GetAsync(string key)
        {
            ImageCacheItem imageCacheItem = null;
            var path = GetPath(key);

            try
            {
                if (path != null && File.Exists(path))
                {
                    var buffer = await File.ReadAllBytesAsync(path);
                    var expiration = GetExpiration(ref buffer);

                    imageCacheItem = new ImageCacheItem
                    {
                        Buffer = buffer.AsBuffer(),
                        Expiration = expiration
                    };

                    //Debug.WriteLine("ImageFileCache: Read {0}, Expires {1}", path, expiration.ToLocalTime());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageFileCache: Failed reading {0}: {1}", path, ex.Message);
            }

            return imageCacheItem;
        }

        public async Task SetAsync(string key, IBuffer buffer, DateTime expiration)
        {
            string path;

            if (buffer != null && buffer.Length > 0 && (path = GetPath(key)) != null)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using (var stream = File.Create(path).AsOutputStream())
                    {
                        await stream.WriteAsync(buffer);
                        await stream.WriteAsync(Encoding.ASCII.GetBytes(expiresTag).AsBuffer());
                        await stream.WriteAsync(BitConverter.GetBytes(expiration.Ticks).AsBuffer());
                    }

                    //Debug.WriteLine("ImageFileCache: Wrote {0}, Expires {1}", path, expiration.ToLocalTime());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ImageFileCache: Failed writing {0}: {1}", path, ex.Message);
                }
            }
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

        private static DateTime GetExpiration(ref byte[] buffer)
        {
            DateTime expiration = DateTime.Today;

            if (buffer.Length > 16 && Encoding.ASCII.GetString(buffer, buffer.Length - 16, 8) == expiresTag)
            {
                expiration = new DateTime(BitConverter.ToInt64(buffer, buffer.Length - 8), DateTimeKind.Utc);
                Array.Resize(ref buffer, buffer.Length - 16);
            }

            return expiration;
        }
    }
}
