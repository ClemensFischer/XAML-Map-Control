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

        public MapLayerMenuItem()
        {
            Loaded += (s, e) =>
            {
                if (DataContext is MapBase map)
                {
                    IsChecked = map.Children.Contains(MapLayer);
                }
            };

            Click += async (s, e) =>
            {
                if (DataContext is MapBase map)
                {
                    await Execute(map);

                    foreach (var item in ParentMenuItems.OfType<MapLayerMenuItem>())
                    {
                        item.IsChecked = map.Children.Contains(item.MapLayer);
                    }
                }
            };
        }

        public override async Task Execute(MapBase map)
        {
            var layer = MapLayer ?? (MapLayer = await MapLayerFactory.Invoke());

            if (layer != null)
            {
                map.MapLayer = layer;
                IsChecked = true;
            }
        }
    }

    public class MapOverlayMenuItem : MapLayerMenuItem
    {
        public override async Task Execute(MapBase map)
        {
            var layer = MapLayer ?? (MapLayer = await MapLayerFactory.Invoke());

            if (layer != null)
            {
                if (map.Children.Contains(layer))
                {
                    map.Children.Remove(layer);
                }
                else
                {
                    var index = 1;

                    foreach (var itemLayer in ParentMenuItems?
                        .OfType<MapOverlayMenuItem>()
                        .Select(item => item.MapLayer)
                        .Where(itemLayer => itemLayer != null))
                    {
                        if (itemLayer == layer)
                        {
                            map.Children.Insert(index, itemLayer);
                            break;
                        }

                        if (map.Children.Contains(itemLayer))
                        {
                            index++;
                        }
                    }
                }

                IsChecked = true;
            }
        }
    }
}
