using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
#if WPF
using System.Windows;
#elif UWP
using Windows.UI.Xaml;
#elif WINUI
using Microsoft.UI.Xaml;
#elif AVALONIA
using DependencyProperty = Avalonia.AvaloniaProperty;
#endif

namespace MapControl.MBTiles
{
    /// <summary>
    /// MapTileLayer that uses an MBTiles SQLite Database. See https://wiki.openstreetmap.org/wiki/MBTiles.
    /// </summary>
    public class MBTileLayer : MapTileLayer
    {
        private static ILogger logger;
        private static ILogger Logger => logger ??= ImageLoader.LoggerFactory?.CreateLogger<MBTileLayer>();

        public static readonly DependencyProperty FileProperty =
            DependencyPropertyHelper.Register<MBTileLayer, string>(nameof(File), null,
                async (layer, oldValue, newValue) => await layer.FilePropertyChanged(newValue));

        public string File
        {
            get => (string)GetValue(FileProperty);
            set => SetValue(FileProperty, value);
        }

        /// <summary>
        /// May be overridden to create a derived MBTileSource that handles other tile formats than png and jpg.
        /// </summary>
        protected virtual async Task<MBTileSource> CreateTileSourceAsync(string file)
        {
            var tileSource = new MBTileSource();

            await tileSource.OpenAsync(file);

            if (tileSource.Metadata.TryGetValue("format", out string format) && format != "png" && format != "jpg")
            {
                tileSource.Dispose();

                throw new NotSupportedException($"Tile image format {format} is not supported.");
            }

            return tileSource;
        }

        private async Task FilePropertyChanged(string file)
        {
            (TileSource as MBTileSource)?.Close();

            ClearValue(TileSourceProperty);
            ClearValue(SourceNameProperty);
            ClearValue(DescriptionProperty);
            ClearValue(MinZoomLevelProperty);
            ClearValue(MaxZoomLevelProperty);

            if (!string.IsNullOrEmpty(file))
            {
                try
                {
                    var tileSource = await CreateTileSourceAsync(file);

                    TileSource = tileSource;

                    if (tileSource.Metadata.TryGetValue("name", out string value))
                    {
                        SourceName = value;
                    }

                    if (tileSource.Metadata.TryGetValue("description", out value))
                    {
                        Description = value;
                    }

                    if (tileSource.Metadata.TryGetValue("minzoom", out value) && int.TryParse(value, out int zoomLevel))
                    {
                        MinZoomLevel = zoomLevel;
                    }

                    if (tileSource.Metadata.TryGetValue("maxzoom", out value) && int.TryParse(value, out zoomLevel))
                    {
                        MaxZoomLevel = zoomLevel;
                    }
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Invalid file: {file}", file);
                }
            }
        }
    }
}
