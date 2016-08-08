// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
#endif

namespace MapControl
{
    /// <summary>
    /// Container class for an item in a MapItemsControl.
    /// </summary>
    public class MapItem : ListBoxItem
    {
        public MapItem()
        {
            DefaultStyleKey = typeof(MapItem);
            MapPanel.AddParentMapHandlers(this);
        }

        public Location Location
        {
            get { return (Location)GetValue(MapPanel.LocationProperty); }
            set { SetValue(MapPanel.LocationProperty, value); }
        }
    }
}
