using System;
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
    public class MapLayerMenuItem : MapMenuItem
    {
#if AVALONIA
        [Content]
#endif
        public FrameworkElement MapLayer { get; set; }

        public Func<Task<FrameworkElement>> MapLayerFactory { get; set; }

        protected override bool GetIsChecked(MapBase map)
        {
            return MapLayer != null && map.Children.Contains(MapLayer);
        }

        public override async Task Execute(MapBase map)
        {
            MapLayer ??= await MapLayerFactory?.Invoke();

            if (MapLayer != null)
            {
                map.MapLayer = MapLayer;
            }
        }
    }

    public class MapOverlayMenuItem : MapLayerMenuItem
    {
        private string sourcePath;

        public string SourcePath
        {
            get => sourcePath;
            set
            {
                sourcePath = value;

                if (sourcePath.EndsWith(".kmz") || sourcePath.EndsWith(".kml"))
                {
                    MapLayerFactory = async () => await GroundOverlay.CreateAsync(sourcePath);
                }
                else
                {
                    MapLayerFactory = async () => await GeoImage.CreateAsync(sourcePath);
                }
            }
        }

        public int InsertOrder { get; set; }

        public override async Task Execute(MapBase map)
        {
            MapLayer ??= await MapLayerFactory?.Invoke();

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
    }
}
