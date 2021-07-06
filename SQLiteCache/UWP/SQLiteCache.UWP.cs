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
        public async Task<Tuple<byte[], DateTime>> GetAsync(string key)
        {
            try
            {
                using (var command = GetItemCommand(key))
                {
                    var reader = await command.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        return Tuple.Create((byte[])reader["buffer"], new DateTime((long)reader["expiration"]));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SQLiteCache.GetAsync({key}): {ex.Message}");
            }

            return null;
        }

        public async Task SetAsync(string key, byte[] buffer, DateTime expiration)
        {
            try
            {
                using (var command = SetItemCommand(key, buffer, expiration))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SQLiteCache.SetAsync({key}): {ex.Message}");
            }
        }
    }
}
