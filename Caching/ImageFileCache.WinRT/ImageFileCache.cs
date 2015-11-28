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

            if (item != null && item.IsOfType(StorageItemTypes.File))
            {
                var file = (StorageFile)item;
                //Debug.WriteLine("ImageFileCache: Reading file {0}", file.Path);

                try
                {
                    return new ImageCacheItem
                    {
                        Buffer = await FileIO.ReadBufferAsync(file),
                        Expires = (await file.Properties.GetImagePropertiesAsync()).DateTaken.UtcDateTime
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ImageFileCache: Reading file {0} failed: {1}", file.Path, ex.Message);
                }
            }

            return null;
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
                //Debug.WriteLine("ImageFileCache: Writing file {0}", file.Path);

                await FileIO.WriteBufferAsync(file, buffer);

                // Store expiration date in ImageProperties.DateTaken
                var properties = await file.Properties.GetImagePropertiesAsync();
                properties.DateTaken = expires;
                await properties.SavePropertiesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageFileCache: Writing file {0}\\{1} failed: {2}", rootFolder.Path, key, ex.Message);
            }
        }
    }
}
