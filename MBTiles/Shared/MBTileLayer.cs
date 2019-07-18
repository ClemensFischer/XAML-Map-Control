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
            var mbTileSource = TileSource as MBTileSource;

            if (mbTileSource != null)
            {
                if (file == null)
                {
                    ClearValue(TileSourceProperty);

                    if (mbTileSource.Name != null)
                    {
                        ClearValue(SourceNameProperty);
                    }

                    if (mbTileSource.Description != null)
                    {
                        ClearValue(DescriptionProperty);
                    }

                    if (mbTileSource.MinZoom.HasValue)
                    {
                        ClearValue(MinZoomLevelProperty);
                    }

                    if (mbTileSource.MaxZoom.HasValue)
                    {
                        ClearValue(MaxZoomLevelProperty);
                    }
                }

                mbTileSource.Dispose();
            }

            if (file != null)
            {
                mbTileSource = await MBTileSource.CreateAsync(file);

                if (mbTileSource.Name != null)
                {
                    SourceName = mbTileSource.Name;
                }

                if (mbTileSource.Description != null)
                {
                    Description = mbTileSource.Description;
                }

                if (mbTileSource.MinZoom.HasValue)
                {
                    MinZoomLevel = mbTileSource.MinZoom.Value;
                }

                if (mbTileSource.MaxZoom.HasValue)
                {
                    MaxZoomLevel = mbTileSource.MaxZoom.Value;
                }

                TileSource = mbTileSource;
            }
        }
    }
}
