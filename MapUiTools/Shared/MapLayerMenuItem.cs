using System.IO;
using System.Linq;
using System.Threading.Tasks;
#if WPF
using System.Windows;
using System.Windows.Markup;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
#else
using Avalonia.Metadata;
using FrameworkElement = Avalonia.Controls.Control;
#endif

namespace MapControl.UiTools
{
#if WPF
    [ContentProperty(nameof(MapLayer))]
#elif UWP || WINUI
    [ContentProperty(Name = nameof(MapLayer))]
#endif
    public partial class MapLayerMenuItem : MapMenuItem
    {
#if AVALONIA
        [Content]
#endif
        public FrameworkElement MapLayer { get; set; }

        protected override bool GetIsChecked(MapBase map)
        {
            return MapLayer != null && map.Children.Contains(MapLayer);
        }

        public override Task ExecuteAsync(MapBase map)
        {
            if (MapLayer != null)
            {
                map.MapLayer = MapLayer;
            }

            return Task.CompletedTask;
        }
    }

    public partial class MapOverlayMenuItem : MapLayerMenuItem
    {
        public string SourcePath { get; set; }

        public int InsertOrder { get; set; }

        public double OverlayOpacity { get; set; } = 1d;

        public override async Task ExecuteAsync(MapBase map)
        {
            if (MapLayer == null)
            {
                await CreateMapLayer();
            }

            if (MapLayer != null)
            {
                if (map.Children.Contains(MapLayer))
                {
                    map.Children.Remove(MapLayer);
                }
                else
                {
                    var insertIndex = ParentMenuItems
                        .OfType<MapOverlayMenuItem>()
                        .Where(item => item.InsertOrder <= InsertOrder && item.GetIsChecked(map))
                        .Count();

                    if (map.MapLayer != null)
                    {
                        insertIndex++;
                    }

                    map.Children.Insert(insertIndex, MapLayer);
                }
            }
        }

        protected virtual async Task CreateMapLayer()
        {
            var ext = Path.GetExtension(SourcePath).ToLower();

            if (ext == ".kmz" || ext == ".kml")
            {
                MapLayer = await GroundOverlay.CreateAsync(SourcePath);
            }
            else
            {
                MapLayer = await GeoImage.CreateAsync(SourcePath);
            }

            MapLayer.Opacity = OverlayOpacity;
        }
    }
}
