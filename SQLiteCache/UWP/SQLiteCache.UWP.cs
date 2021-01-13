// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MapControl.Caching
{
    /// <summary>
    /// IImageCache implementation based on SqLite.
    /// </summary>
    public partial class SQLiteCache : IImageCache
    {
        public SQLiteCache(StorageFolder folder, string fileName = "TileCache.sqlite")
        {
            if (folder == null)
            {
                throw new ArgumentNullException("The parameter folder must not be null.");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("The parameter fileName must not be null or empty.");
            }

            connection = Open(Path.Combine(folder.Path, fileName));

            Clean();
        }

        public async Task<ImageCacheItem> GetAsync(string key)
        {
            try
            {
                using (var command = GetItemCommand(key))
                {
                    var reader = await command.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        return new ImageCacheItem
                        {
                            Expiration = new DateTime((long)reader["expiration"]),
                            Buffer = ((byte[])reader["buffer"]).AsBuffer()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SQLiteCache.GetAsync(\"{0}\"): {1}", key, ex.Message);
            }

            return null;
        }

        public async Task SetAsync(string key, IBuffer buffer, DateTime expiration)
        {
            try
            {
                using (var command = SetItemCommand(key, expiration, buffer?.ToArray()))
                {
                    await command.ExecuteNonQueryAsync();
                }

                //Debug.WriteLine("SQLiteCache.SetAsync(\"{0}\"): expires {1}", key, expiration.ToLocalTime());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SQLiteCache.SetAsync(\"{0}\"): {1}", key, ex.Message);
            }
        }
    }
}
