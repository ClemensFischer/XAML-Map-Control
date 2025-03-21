using System.Collections.Generic;
using System.Linq;
#if UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl.UiTools
{
    public class MapMenuItem : ToggleMenuFlyoutItem
    {
        protected IEnumerable<MapMenuItem> ParentMenuItems
            => (VisualTreeHelper.GetParent(this) as Panel)?.Children.OfType<MapMenuItem>();
    }
}
