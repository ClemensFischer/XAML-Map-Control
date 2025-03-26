using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl.UiTools
{
    public abstract class MapMenuItem : ToggleMenuFlyoutItem
    {
        protected MapMenuItem()
        {
            Loaded += (s, e) =>
            {
                ParentMenuItems = ((Panel)VisualTreeHelper.GetParent(this)).Children.OfType<MapMenuItem>().ToList();

                if (DataContext is MapBase map)
                {
                    IsChecked = GetIsChecked(map);
                }
            };

            Click += async (s, e) =>
            {
                if (DataContext is MapBase map)
                {
                    await Execute(map);

                    foreach (var item in ParentMenuItems)
                    {
                        item.IsChecked = item.GetIsChecked(map);
                    }
                }
            };
        }

        protected IList<MapMenuItem> ParentMenuItems { get; private set; }

        protected abstract bool GetIsChecked(MapBase map);

        public abstract Task Execute(MapBase map);
    }
}
