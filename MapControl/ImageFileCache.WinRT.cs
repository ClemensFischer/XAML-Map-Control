// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MapControl
{
    public class ImageFileCache : IObjectCache
    {
        private readonly IStorageFolder rootFolder;

        public ImageFileCache()
        {
            rootFolder = ApplicationData.Current.TemporaryFolder;
        }

        public ImageFileCache(IStorageFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("The parameter folder must not be null.");
            }

            rootFolder = folder;
        }

        public async Task<object> GetAsync(string key)
        {
            try
            {
                return await PathIO.ReadBufferAsync(Path.Combine(rootFolder.Path, key));
            }
            catch
            {
                return null;
            }
        }

        public async Task SetAsync(string key, object value)
        {
            try
            {
                var buffer = (IBuffer)value;
                var names = key.Split('\\');
                var folder = rootFolder;

                for (int i = 0; i < names.Length - 1; i++)
                {
                    folder = await folder.CreateFolderAsync(names[i], CreationCollisionOption.OpenIfExists);
                }

                var file = await folder.CreateFileAsync(names[names.Length - 1], CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBufferAsync(file, buffer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
