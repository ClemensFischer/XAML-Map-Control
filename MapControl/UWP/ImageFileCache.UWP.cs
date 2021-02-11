// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MapControl.Caching
{
    public class ImageFileCache : IImageCache
    {
        private readonly StorageFolder folder;

        public ImageFileCache(StorageFolder folder)
        {
            this.folder = folder ?? throw new ArgumentNullException(nameof(folder));

            Debug.WriteLine("Created ImageFileCache in " + folder.Path);
        }

        public async Task<ImageCacheItem> GetAsync(string key)
        {
            string path;

            try
            {
                path = Path.Combine(GetPathElements(key));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageFileCache: Invalid key {0}: {1}", key, ex.Message);
                return null;
            }

            var item = await folder.TryGetItemAsync(path);

            if (item != null && item.IsOfType(StorageItemTypes.File))
            {
                var file = (StorageFile)item;
                //Debug.WriteLine("ImageFileCache: Reading " + file.Path);

                try
                {
                    return new ImageCacheItem
                    {
                        Buffer = await FileIO.ReadBufferAsync(file),
                        Expiration = (await file.Properties.GetImagePropertiesAsync()).DateTaken.UtcDateTime
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ImageFileCache: Reading {0}: {1}", file.Path, ex.Message);
                }
            }

            return null;
        }

        public async Task SetAsync(string key, IBuffer buffer, DateTime expiration)
        {
            if (buffer != null && buffer.Length > 0) // do not cache a no-tile entry
            {
                var folders = GetPathElements(key);

                try
                {
                    var folder = this.folder;

                    for (int i = 0; i < folders.Length - 1; i++)
                    {
                        folder = await folder.CreateFolderAsync(folders[i], CreationCollisionOption.OpenIfExists);
                    }

                    var file = await folder.CreateFileAsync(folders[folders.Length - 1], CreationCollisionOption.ReplaceExisting);
                    //Debug.WriteLine("ImageFileCache: Writing {0}, Expires {1}", file.Path, expiration.ToLocalTime());

                    await FileIO.WriteBufferAsync(file, buffer);

                    // Store expiration date in ImageProperties.DateTaken
                    var properties = await file.Properties.GetImagePropertiesAsync();
                    properties.DateTaken = expiration;

                    await properties.SavePropertiesAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ImageFileCache: Writing {0}: {1}", Path.Combine(folder.Path, Path.Combine(folders)), ex.Message);
                }
            }
        }

        private string[] GetPathElements(string key)
        {
            return key.Split('\\', '/', ',', ':', ';');
        }
    }
}
