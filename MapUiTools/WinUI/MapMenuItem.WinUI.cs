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
            Loaded += (s, e) => ParentMenuItems = ((Panel)VisualTreeHelper.GetParent(this)).Children.OfType<MapMenuItem>().ToList();
        }

        public abstract Task Execute(MapBase map);

        protected IList<MapMenuItem> ParentMenuItems { get; private set; }
    }
}
