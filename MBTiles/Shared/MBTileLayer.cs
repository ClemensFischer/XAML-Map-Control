// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MapControl
{
    public class MBTileLayer : MapTileLayer
    {
        public static readonly DependencyProperty FileProperty = DependencyProperty.Register(
            nameof(File), typeof(string), typeof(MBTileLayer),
            new PropertyMetadata(null, (o, e) => ((MBTileLayer)o).FilePropertyChanged((string)e.NewValue)));

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

        private void FilePropertyChanged(string file)
        {
            MBTileSource ts;

            if (file != null)
            {
                ts = new MBTileSource(file);

                if (ts.Metadata.ContainsKey("name"))
                {
                    SourceName = ts.Metadata["name"];
                }

                if (ts.Metadata.ContainsKey("description"))
                {
                    Description = ts.Metadata["description"];
                }

                if (ts.Metadata.ContainsKey("minzoom"))
                {
                    int minZoom;
                    if (int.TryParse(ts.Metadata["minzoom"], out minZoom))
                    {
                        MinZoomLevel = minZoom;
                    }
                }

                if (ts.Metadata.ContainsKey("maxzoom"))
                {
                    int maxZoom;
                    if (int.TryParse(ts.Metadata["maxzoom"], out maxZoom))
                    {
                        MaxZoomLevel = maxZoom;
                    }
                }

                TileSource = ts;
            }
            else if ((ts = TileSource as MBTileSource) != null)
            {
                ClearValue(TileSourceProperty);

                if (ts.Metadata.ContainsKey("name"))
                {
                    ClearValue(SourceNameProperty);
                }

                if (ts.Metadata.ContainsKey("description"))
                {
                    ClearValue(DescriptionProperty);
                }

                if (ts.Metadata.ContainsKey("minzoom"))
                {
                    ClearValue(MinZoomLevelProperty);
                }

                if (ts.Metadata.ContainsKey("maxzoom"))
                {
                    ClearValue(MaxZoomLevelProperty);
                }
            }
        }
    }
}
