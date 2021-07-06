// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MapControl.MBTiles
{
    public sealed class MBTileData : IDisposable
    {
        private readonly SQLiteConnection connection;

        public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>();

        private MBTileData(string file)
        {
            connection = new SQLiteConnection("Data Source=" + Path.GetFullPath(file));
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public static async Task<MBTileData> CreateAsync(string file)
        {
            var tileData = new MBTileData(file);

            await tileData.OpenAsync();
            await tileData.ReadMetadataAsync();

            return tileData;
        }

        private async Task OpenAsync()
        {
            await connection.OpenAsync();

            using (var command = new SQLiteCommand("create table if not exists metadata (name string, value string)", connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            using (var command = new SQLiteCommand("create table if not exists tiles (zoom_level integer, tile_column integer, tile_row integer, tile_data blob)", connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task ReadMetadataAsync()
        {
            try
            {
                using (var command = new SQLiteCommand("select * from metadata", connection))
                {
                    var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        Metadata[(string)reader["name"]] = (string)reader["value"];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MBTileData: {ex.Message}");
            }
        }

        public async Task WriteMetadataAsync()
        {
            try
            {
                using (var command = new SQLiteCommand("insert or replace into metadata (name, value) values (@n, @v)", connection))
                {
                    foreach (var keyValue in Metadata)
                    {
                        command.Parameters.AddWithValue("@n", keyValue.Key);
                        command.Parameters.AddWithValue("@v", keyValue.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MBTileData: {ex.Message}");
            }
        }

        public async Task<byte[]> ReadImageBufferAsync(int x, int y, int zoomLevel)
        {
            byte[] imageBuffer = null;

            try
            {
                using (var command = new SQLiteCommand("select tile_data from tiles where zoom_level=@z and tile_column=@x and tile_row=@y", connection))
                {
                    command.Parameters.AddWithValue("@z", zoomLevel);
                    command.Parameters.AddWithValue("@x", x);
                    command.Parameters.AddWithValue("@y", (1 << zoomLevel) - y - 1);

                    imageBuffer = await command.ExecuteScalarAsync() as byte[];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MBTileData: {zoomLevel}/{x}/{y}: {ex.Message}");
            }

            return imageBuffer;
        }

        public async Task WriteImageBufferAsync(int x, int y, int zoomLevel, byte[] imageBuffer)
        {
            try
            {
                using (var command = new SQLiteCommand("insert or replace into tiles (zoom_level, tile_column, tile_row, tile_data) values (@z, @x, @y, @b)", connection))
                {
                    command.Parameters.AddWithValue("@z", zoomLevel);
                    command.Parameters.AddWithValue("@x", x);
                    command.Parameters.AddWithValue("@y", (1 << zoomLevel) - y - 1);
                    command.Parameters.AddWithValue("@b", imageBuffer);

                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MBTileData: {zoomLevel}/{x}/{y}: {ex.Message}");
            }
        }
    }
}
