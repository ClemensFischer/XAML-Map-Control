using System.Threading.Tasks;
#if WPF
using System.Windows;
using System.Windows.Controls;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#else
using Avalonia.Controls;
using DependencyProperty = Avalonia.AvaloniaProperty;
#endif

namespace MapControl.UiTools
{
    public partial class MenuButton : Button
    {
        public static readonly DependencyProperty MapProperty =
            DependencyPropertyHelper.Register<MenuButton, MapBase>(nameof(Map), null,
                async (button, oldValue, newValue) => await button.Initialize());

        public MapBase Map
        {
            get => (MapBase)GetValue(MapProperty);
            set => SetValue(MapProperty, value);
        }

        private async Task Initialize()
        {
            if (Map != null)
            {
                DataContext = Map;

                if (Items.Count > 0)
                {
                    if (Items[0] is MapLayerMenuItem mapLayerItem)
                    {
                        await mapLayerItem.Execute(Map);
                    }
                    else if (Items[0] is MapProjectionMenuItem mapProjectionItem)
                    {
                        mapProjectionItem.Execute(Map);
                    }
                }
            }
        }
    }
}
