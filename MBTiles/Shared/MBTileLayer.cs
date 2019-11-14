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
                var tiledata = await MBTileData.CreateAsync(file);
                var metadata = await tiledata.ReadMetadataAsync();
                string s;
                int minZoom;
                int maxZoom;

                if (metadata.TryGetValue("name", out s))
                {
                    SourceName = s;
                }

                if (metadata.TryGetValue("description", out s))
                {
                    Description = s;
                }

                if (metadata.TryGetValue("minzoom", out s) && int.TryParse(s, out minZoom))
                {
                    MinZoomLevel = minZoom;
                }

                if (metadata.TryGetValue("maxzoom", out s) && int.TryParse(s, out maxZoom))
                {
                    MaxZoomLevel = maxZoom;
                }

                TileSource = new MBTileSource(tiledata);
            }
        }
    }
}
