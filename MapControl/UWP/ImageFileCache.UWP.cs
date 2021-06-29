// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MapControl.Caching
{
    public class ImageFileCache : IImageCache
    {
        private const string expiresTag = "EXPIRES:";

        private readonly string folderPath;

        public ImageFileCache(StorageFolder folder)
            : this(folder.Path)
        {
        }

        public ImageFileCache(string path)
        {
            folderPath = path;
            Debug.WriteLine("Created ImageFileCache in " + folderPath);
        }

        public async Task<ImageCacheItem> GetAsync(string key)
        {
            ImageCacheItem imageCacheItem = null;
            string path;

            try
            {
                path = Path.Combine(GetPathElements(key));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageFileCache: Invalid key {0}: {1}", key, ex.Message);
                return imageCacheItem;
            }

            var folder = await StorageFolder.GetFolderFromPathAsync(folderPath);
            var item = await folder.TryGetItemAsync(path);

            if (item != null && item.IsOfType(StorageItemTypes.File))
            {
                var file = (StorageFile)item;

                try
                {
                    var buffer = (await FileIO.ReadBufferAsync(file)).ToArray();
                    var expiration = GetExpiration(ref buffer);

                    imageCacheItem = new ImageCacheItem
                    {
                        Buffer = buffer.AsBuffer(),
                        Expiration = expiration
                    };

                    //Debug.WriteLine("ImageFileCache: Read {0}, Expires {1}", file.Path, expiration.ToLocalTime());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ImageFileCache: Failed reading {0}: {1}", file.Path, ex.Message);
                }
            }

            return imageCacheItem;
        }

        public async Task SetAsync(string key, IBuffer buffer, DateTime expiration)
        {
            if (buffer != null && buffer.Length > 0)
            {
                var folders = GetPathElements(key);

                try
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(folderPath);

                    for (int i = 0; i < folders.Length - 1; i++)
                    {
                        folder = await folder.CreateFolderAsync(folders[i], CreationCollisionOption.OpenIfExists);
                    }

                    var file = await folder.CreateFileAsync(folders[folders.Length - 1], CreationCollisionOption.ReplaceExisting);

                    //Debug.WriteLine("ImageFileCache: Write {0}, Expires {1}", file.Path, expiration.ToLocalTime());

                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await stream.WriteAsync(buffer);
                        await stream.WriteAsync(Encoding.ASCII.GetBytes(expiresTag).AsBuffer());
                        await stream.WriteAsync(BitConverter.GetBytes(expiration.Ticks).AsBuffer());
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ImageFileCache: Failed writing {0}: {1}", Path.Combine(folderPath, Path.Combine(folders)), ex.Message);
                }
            }
        }

        private static string[] GetPathElements(string key)
        {
            return key.Split('\\', '/', ',', ':', ';');
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
