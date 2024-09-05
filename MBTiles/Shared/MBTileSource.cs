// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
#if WPF
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using ImageSource = Avalonia.Media.IImage;
#endif

namespace MapControl.MBTiles
{
    public class MBTileSource : TileSource, IDisposable
    {
        private SQLiteConnection connection;

        public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>();

        public async Task OpenAsync(string file)
        {
            Close();

            connection = new SQLiteConnection("Data Source=" + Path.GetFullPath(file) + ";Read Only=True");

            await connection.OpenAsync();

            using (var command = new SQLiteCommand("select * from metadata", connection))
            {
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    Metadata[(string)reader["name"]] = (string)reader["value"];
                }
            }
        }

        public void Close()
        {
            if (connection != null)
            {
                Metadata.Clear();
                connection.Dispose();
                connection = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }

        public async Task<byte[]> ReadImageBufferAsync(int x, int y, int zoomLevel)
        {
            using (var command = new SQLiteCommand("select tile_data from tiles where zoom_level=@z and tile_column=@x and tile_row=@y", connection))
            {
                command.Parameters.AddWithValue("@z", zoomLevel);
                command.Parameters.AddWithValue("@x", x);
                command.Parameters.AddWithValue("@y", (1 << zoomLevel) - y - 1);

                return await command.ExecuteScalarAsync() as byte[];
            }
        }

        public override async Task<ImageSource> LoadImageAsync(int x, int y, int zoomLevel)
        {
            ImageSource image = null;

            try
            {
                var buffer = await ReadImageBufferAsync(x, y, zoomLevel);

                if (buffer != null)
                {
                    image = await ImageLoader.LoadImageAsync(buffer);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{nameof(MBTileSource)}: {ex.Message}");
            }

            return image;
        }
    }
}
