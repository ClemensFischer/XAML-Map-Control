// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2015 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MapControl.Caching
{
    public class ImageFileCache : IImageCache
    {
        private readonly string name;
        private StorageFolder rootFolder;

        public ImageFileCache(string name = null, StorageFolder folder = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = TileImageLoader.DefaultCacheName;
            }

            if (folder == null)
            {
                folder = TileImageLoader.DefaultCacheFolder;
            }

            this.name = name;

            folder.CreateFolderAsync(name, CreationCollisionOption.OpenIfExists).Completed = (o, s) =>
            {
                rootFolder = o.GetResults();
                Debug.WriteLine("Created ImageFileCache in {0}.", rootFolder.Path);
            };
        }

        public virtual async Task<ImageCacheItem> GetAsync(string key)
        {
            var item = await rootFolder.TryGetItemAsync(key);

            if (item == null || !item.IsOfType(StorageItemTypes.File))
            {
                return null;
            }

            var file = (StorageFile)item;

            var cacheItem = new ImageCacheItem
            {
                Buffer = await FileIO.ReadBufferAsync(file)
            };

            try
            {
                // Use ImageProperties.DateTaken to get expiration date
                var imageProperties = await file.Properties.GetImagePropertiesAsync();
                cacheItem.Expires = imageProperties.DateTaken.UtcDateTime;
            }
            catch
            {
            }

            return cacheItem;
        }

        public virtual async Task SetAsync(string key, IBuffer buffer, DateTime expires)
        {
            try
            {
                var names = key.Split('\\');
                var folder = rootFolder;

                for (int i = 0; i < names.Length - 1; i++)
                {
                    folder = await folder.CreateFolderAsync(names[i], CreationCollisionOption.OpenIfExists);
                }

                var file = await folder.CreateFileAsync(names[names.Length - 1], CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBufferAsync(file, buffer);

                // Use ImageProperties.DateTaken to store expiration date
                var imageProperties = await file.Properties.GetImagePropertiesAsync();
                imageProperties.DateTaken = expires;
                await imageProperties.SavePropertiesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
