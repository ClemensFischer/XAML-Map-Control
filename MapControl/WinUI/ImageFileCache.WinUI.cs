// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MapControl.Caching
{
    public partial class ImageFileCache : IImageCache
    {
        public async Task<Tuple<byte[], DateTime>> GetAsync(string key)
        {
            Tuple<byte[], DateTime> cacheItem = null;
            var path = GetPath(key);

            try
            {
                if (path != null && File.Exists(path))
                {
                    var buffer = await File.ReadAllBytesAsync(path);
                    var expiration = ReadExpiration(ref buffer);

                    cacheItem = Tuple.Create(buffer, expiration);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageFileCache: Failed reading {path}: {ex.Message}");
            }

            return cacheItem;
        }

        public async Task SetAsync(string key, byte[] buffer, DateTime expiration)
        {
            var path = GetPath(key);

            if (buffer != null && buffer.Length > 0 && path != null)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using (var stream = File.Create(path))
                    {
                        await stream.WriteAsync(buffer, 0, buffer.Length);
                        await WriteExpirationAsync(stream, expiration);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ImageFileCache: Failed writing {path}: {ex.Message}");
                }
            }
        }
    }
}
