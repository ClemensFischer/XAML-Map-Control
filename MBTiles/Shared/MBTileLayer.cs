// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MapControl.MBTiles
{
    /// <summary>
    /// MapTileLayer that uses an MBTiles SQLite Database. See https://wiki.openstreetmap.org/wiki/MBTiles.
    /// </summary>
    public class MBTileLayer : MapTileLayer
    {
        public static readonly DependencyProperty FileProperty = DependencyProperty.Register(
            nameof(File), typeof(string), typeof(MBTileLayer),
            new PropertyMetadata(null, async (o, e) => await ((MBTileLayer)o).FilePropertyChanged((string)e.NewValue)));

        public MBTileLayer()
            : this(new TileImageLoader())
        {
        }

        public MBTileLayer(ITileImageLoader tileImageLoader)
            : base(tileImageLoader)
        {
        }

        public string File
        {
            get { return (string)GetValue(FileProperty); }
            set { SetValue(FileProperty, value); }
        }

        /// <summary>
        /// May be overridden to create a derived MBTileSource that handles other tile formats than png and jpg, e.g. pbf.
        /// </summary>
        protected virtual MBTileSource CreateTileSource(MBTileData tileData)
        {
            return new MBTileSource(tileData);
        }

        private async Task FilePropertyChanged(string file)
        {
            (TileSource as MBTileSource)?.Dispose();

            ClearValue(TileSourceProperty);
            ClearValue(SourceNameProperty);
            ClearValue(DescriptionProperty);
            ClearValue(MinZoomLevelProperty);
            ClearValue(MaxZoomLevelProperty);

            if (file != null)
            {
                var tileData = await MBTileData.CreateAsync(file);
                int minZoom;
                int maxZoom;
                string s;

                if (tileData.Metadata.TryGetValue("name", out s))
                {
                    SourceName = s;
                }

                if (tileData.Metadata.TryGetValue("description", out s))
                {
                    Description = s;
                }

                if (tileData.Metadata.TryGetValue("minzoom", out s) && int.TryParse(s, out minZoom))
                {
                    MinZoomLevel = minZoom;
                }

                if (tileData.Metadata.TryGetValue("maxzoom", out s) && int.TryParse(s, out maxZoom))
                {
                    MaxZoomLevel = maxZoom;
                }

                TileSource = CreateTileSource(tileData);
            }
        }
    }
}
