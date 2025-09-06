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
            return map.Children.Contains(MapLayer);
        }

        public override async Task Execute(MapBase map)
        {
            if (MapLayer == null)
            {
                MapLayer = await MapLayerFactory?.Invoke();
            }

            if (MapLayer != null)
            {
                map.MapLayer = MapLayer;
            }
        }
    }

    public class MapOverlayMenuItem : MapLayerMenuItem
    {
        public override async Task Execute(MapBase map)
        {
            if (MapLayer == null)
            {
                MapLayer = await MapLayerFactory?.Invoke();
            }

            if (MapLayer != null)
            {
                if (map.Children.Contains(MapLayer))
                {
                    map.Children.Remove(MapLayer);
                }
                else
                {
                    var index = 1;

                    foreach (var mapLayer in ParentMenuItems
                        .OfType<MapOverlayMenuItem>()
                        .Select(item => item.MapLayer)
                        .Where(mapLayer => mapLayer != null))
                    {
                        if (mapLayer == MapLayer)
                        {
                            map.Children.Insert(index, mapLayer);
                            break;
                        }

                        if (map.Children.Contains(mapLayer))
                        {
                            index++;
                        }
                    }
                }
            }
        }
    }
}
