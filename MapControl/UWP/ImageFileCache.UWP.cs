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
    public partial class ImageFileCache : IImageCache
    {
        public async Task<ImageCacheItem> GetAsync(string key)
        {
            ImageCacheItem imageCacheItem = null;
            var path = GetPath(key);

            try
            {
                if (path != null && File.Exists(path))
                {
                    var buffer = await File.ReadAllBytesAsync(path);
                    var expiration = ReadExpiration(ref buffer);

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
            var path = GetPath(key);

            if (buffer != null && buffer.Length > 0 && path != null)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using (var stream = File.Create(path))
                    {
                        await stream.AsOutputStream().WriteAsync(buffer);
                        await stream.WriteAsync(Encoding.ASCII.GetBytes(expiresTag), 0, 8);
                        await stream.WriteAsync(BitConverter.GetBytes(expiration.Ticks), 0, 8);
                    }

                    //Debug.WriteLine("ImageFileCache: Wrote {0}, Expires {1}", path, expiration.ToLocalTime());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ImageFileCache: Failed writing {0}: {1}", path, ex.Message);
                }
            }
        }
    }
}
