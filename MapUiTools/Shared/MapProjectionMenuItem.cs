using System;
using System.Diagnostics;
using System.Linq;
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
    [ContentProperty(nameof(CrsId))]
#elif UWP || WINUI
    [ContentProperty(Name = nameof(CrsId))]
#endif
    public partial class MapProjectionMenuItem : MapMenuItem
    {
#if AVALONIA
        [Content]
#endif
        public string CrsId { get; set; }

        protected override bool GetIsEnabled(MapBase map)
        {
            return map.MapLayer is not IMapLayer mapLayer
                || mapLayer.SupportedCrsIds == null
                || mapLayer.SupportedCrsIds.Contains(CrsId);
        }

        protected override bool GetIsChecked(MapBase map)
        {
            return map.MapProjection.CrsId == CrsId;
        }

        public override Task ExecuteAsync(MapBase map)
        {
            if (!GetIsChecked(map))
            {
                try
                {
                    map.MapProjection = MapProjection.Parse(CrsId);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MapProjection.Parse: {ex.Message}");
                }
            }

            return Task.CompletedTask;
        }
    }
}
