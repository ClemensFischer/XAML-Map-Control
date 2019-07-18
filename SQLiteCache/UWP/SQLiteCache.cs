// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Microsoft.Data.Sqlite;
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
    public sealed class SQLiteCache : IImageCache, IDisposable
    {
        private readonly SqliteConnection connection;

        public SQLiteCache(StorageFolder folder, string fileName = "TileCache.sqlite")
        {
            if (folder == null)
            {
                throw new ArgumentNullException("The parameter folder must not be null.");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("The parameter fileName must not be null.");
            }

            connection = new SqliteConnection("Data Source=" + Path.Combine(folder.Path, fileName));

            connection.Open();

            using (var command = new SqliteCommand("create table if not exists items (key text, expiration integer, buffer blob)", connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public async Task<ImageCacheItem> GetAsync(string key)
        {
            try
            {
                using (var command = new SqliteCommand("select expiration, buffer from items where key=@key", connection))
                {
                    command.Parameters.AddWithValue("@key", key);
                    var reader = await command.ExecuteReaderAsync();

                    if (reader.Read())
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
                Debug.WriteLine("SqLiteCache: GetAsync(\"{0}\"): {1}", key, ex.Message);
            }

            return null;
        }

        public async Task SetAsync(string key, IBuffer buffer, DateTime expiration)
        {
            try
            {
                using (var command = new SqliteCommand("insert or replace into items (key, expiration, buffer) values (@key, @exp, @buf)", connection))
                {
                    command.Parameters.AddWithValue("@key", key);
                    command.Parameters.AddWithValue("@exp", expiration.Ticks);
                    command.Parameters.AddWithValue("@buf", buffer.ToArray());
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SqLiteCache: SetAsync(\"{0}\"): {1}", key, ex.Message);
            }
        }
    }
}
