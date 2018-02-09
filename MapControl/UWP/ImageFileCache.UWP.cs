// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
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
        private StorageFolder rootFolder;

        public ImageFileCache(StorageFolder rootFolder)
        {
            if (rootFolder == null)
            {
                throw new ArgumentNullException("The parameter rootFolder must not be null.");
            }

            this.rootFolder = rootFolder;

            Debug.WriteLine("Created ImageFileCache in " + rootFolder.Path);
        }

        public virtual async Task<ImageCacheItem> GetAsync(string key)
        {
            string path = null;

            try
            {
                path = Path.Combine(key.Split('\\', '/', ':', ';'));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageFileCache: Invalid key {0}: {1}", key, ex.Message);
            }

            if (path != null)
            {
                var item = await rootFolder.TryGetItemAsync(path);

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
            }

            return null;
        }

        public virtual async Task SetAsync(string key, IBuffer buffer, DateTime expiration)
        {
            var paths = key.Split('\\', '/', ':', ';');

            try
            {
                var folder = rootFolder;

                for (int i = 0; i < paths.Length - 1; i++)
                {
                    folder = await folder.CreateFolderAsync(paths[i], CreationCollisionOption.OpenIfExists);
                }

                var file = await folder.CreateFileAsync(paths[paths.Length - 1], CreationCollisionOption.ReplaceExisting);
                //Debug.WriteLine("ImageFileCache: Writing {0}, Expires {1}", file.Path, expiration.ToLocalTime());

                await FileIO.WriteBufferAsync(file, buffer);

                // Store expiration date in ImageProperties.DateTaken
                var properties = await file.Properties.GetImagePropertiesAsync();
                properties.DateTaken = expiration;
                await properties.SavePropertiesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ImageFileCache: Writing {0}\\{1}: {2}", rootFolder.Path, string.Join("\\", paths), ex.Message);
            }
        }
    }
}
