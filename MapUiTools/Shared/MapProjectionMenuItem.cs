using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

        protected override bool GetIsChecked(MapBase map)
        {
            return map.MapProjection.CrsId == MapProjection;
        }

        public override Task Execute(MapBase map)
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

            return Task.CompletedTask;
        }
    }
}
