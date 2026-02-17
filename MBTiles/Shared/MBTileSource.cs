using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
    public sealed partial class MBTileSource : TileSource, IDisposable
    {
        private static ILogger Logger => field ??= ImageLoader.LoggerFactory?.CreateLogger<MBTileSource>();

        private SQLiteConnection connection;

        public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>();

        public async Task OpenAsync(string file)
        {
            Close();

            connection = new SQLiteConnection("Data Source=" + FilePath.GetFullPath(file) + ";Read Only=True");

            await connection.OpenAsync();

            using var command = new SQLiteCommand("select * from metadata", connection);

            var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Metadata[(string)reader["name"]] = (string)reader["value"];
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
            Close();
        }

        public override async Task<ImageSource> LoadImageAsync(int zoomLevel, int column, int row)
        {
            ImageSource image = null;

            try
            {
                using var command = new SQLiteCommand("select tile_data from tiles where zoom_level=@z and tile_column=@x and tile_row=@y", connection);

                command.Parameters.AddWithValue("@z", zoomLevel);
                command.Parameters.AddWithValue("@x", column);
                command.Parameters.AddWithValue("@y", (1 << zoomLevel) - row - 1);

                var buffer = (byte[])await command.ExecuteScalarAsync();

                if (buffer?.Length > 0)
                {
                    image = await ImageLoader.LoadImageAsync(buffer);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "LoadImageAsync");
            }

            return image;
        }
    }
}
