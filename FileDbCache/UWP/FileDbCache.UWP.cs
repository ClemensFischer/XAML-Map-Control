// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace MapControl.Caching
{
    public partial class FileDbCache : IImageCache
    {
        public FileDbCache(StorageFolder folder, string fileName = "TileCache.fdb")
        {
            if (folder == null)
            {
                throw new ArgumentNullException("The parameter folder must not be null.");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("The parameter fileName must not be null.");
            }

            dbPath = Path.Combine(folder.Path, "TileCache.fdb");

            Open();

            Application.Current.Resuming += (s, e) => Open();
            Application.Current.Suspending += (s, e) => Close();
        }

        public Task<ImageCacheItem> GetAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("The parameter key must not be null.");
            }

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
            if (key == null)
            {
                throw new ArgumentNullException("The parameter key must not be null.");
            }

            if (buffer == null)
            {
                throw new ArgumentNullException("The parameter buffer must not be null.");
            }

            return Task.Run(() => AddOrUpdateRecord(key, buffer.ToArray(), expiration));
        }
    }
}
