using System.Linq;
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

                var initialItem =
                    Items.OfType<MapMenuItem>().FirstOrDefault(item => item.IsChecked) ??
                    Items.OfType<MapMenuItem>().FirstOrDefault();

                if (initialItem != null)
                {
                    await initialItem.Execute(Map);
                }
            }
        }
    }
}
