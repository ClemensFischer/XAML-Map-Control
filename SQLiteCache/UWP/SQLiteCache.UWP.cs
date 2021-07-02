// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MapControl.Caching
{
    public partial class SQLiteCache : IImageCache
    {
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
                            Buffer = (byte[])reader["buffer"]
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

        public async Task SetAsync(string key, ImageCacheItem cacheItem)
        {
            try
            {
                using (var command = SetItemCommand(key, cacheItem.Expiration, cacheItem.Buffer))
                {
                    await command.ExecuteNonQueryAsync();
                }

                //Debug.WriteLine("SQLiteCache.SetAsync(\"{0}\"): expires {1}", key, cacheItem.Expiration.ToLocalTime());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SQLiteCache.SetAsync(\"{0}\"): {1}", key, ex.Message);
            }
        }
    }
}
