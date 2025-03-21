using System;
using System.Diagnostics;
using System.Linq;
#if WPF
using System.Windows.Markup;
#elif UWP
using Windows.UI.Xaml.Markup;
#elif WINUI
using Microsoft.UI.Xaml.Markup;
#else
using Avalonia.Metadata;
#endif

namespace MapControl.UiTools
{
#if WPF
    [ContentProperty(nameof(MapProjection))]
#elif UWP || WINUI
    [ContentProperty(Name = nameof(MapProjection))]
#endif
    public class MapProjectionMenuItem : MapMenuItem
    {
#if AVALONIA
        [Content]
#endif
        public string MapProjection { get; set; }

        public MapProjectionMenuItem()
        {
            Click += (s, e) =>
            {
                if (DataContext is MapBase map)
                {
                    Execute(map);

                    foreach (var item in ParentMenuItems.OfType<MapProjectionMenuItem>())
                    {
                        item.IsChecked = map.MapProjection.CrsId == item.MapProjection;
                    }
                }
            };
        }

        public void Execute(MapBase map)
        {
            bool success = true;

            if (map.MapProjection.CrsId != MapProjection)
            {
                try
                {
                    map.MapProjection = MapProjectionFactory.Instance.GetProjection(MapProjection);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{nameof(MapProjectionFactory)}: {ex.Message}");
                    success = false;
                }
            }

            IsChecked = success;
        }
    }
}
