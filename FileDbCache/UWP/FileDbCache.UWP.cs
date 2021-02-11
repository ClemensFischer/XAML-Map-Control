// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MapControl.Caching
{
    public partial class FileDbCache : IImageCache
    {
        public FileDbCache(StorageFolder folder, string fileName = "TileCache.fdb")
        {
            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("The fileName argument must not be null or empty", nameof(fileName));
            }

            Open(Path.Combine(folder.Path, fileName));
        }

        public Task<ImageCacheItem> GetAsync(string key)
        {
            return Task.Run(() =>
            {
                var record = GetRecordByKey(key);

                if (record == null)
                {
                    return null;
                }

                return new ImageCacheItem
                {
                    Buffer = ((byte[])record[0]).AsBuffer(),
                    Expiration = (DateTime)record[1]
                };
            });
        }

        public Task SetAsync(string key, IBuffer buffer, DateTime expiration)
        {
            return Task.Run(() => AddOrUpdateRecord(key, buffer?.ToArray(), expiration));
        }
    }
}
