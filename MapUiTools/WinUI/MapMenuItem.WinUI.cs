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
        public abstract Task<bool> Execute(MapBase map);

        protected IEnumerable<MapMenuItem> ParentMenuItems
            => ((Panel)VisualTreeHelper.GetParent(this)).Children.OfType<MapMenuItem>();
    }
}
