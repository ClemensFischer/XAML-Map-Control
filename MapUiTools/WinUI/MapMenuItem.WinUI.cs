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
    public abstract partial class MapMenuItem : ToggleMenuFlyoutItem
    {
        protected MapMenuItem()
        {
            Loaded += (_, _) =>
            {
                ParentMenuItems = ((Panel)VisualTreeHelper.GetParent(this)).Children.OfType<MapMenuItem>().ToList();
                Initialize();
            };

            Click += (_, _) => Execute();
        }

        protected IList<MapMenuItem> ParentMenuItems { get; private set; }
    }
}
