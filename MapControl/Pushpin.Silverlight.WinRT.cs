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
    /// Displays a pushpin at a geographic location provided by the MapPanel.Location attached property.
    /// </summary>
    public class Pushpin : ContentControl
    {
        public Pushpin()
        {
            DefaultStyleKey = typeof(Pushpin);
            MapPanel.AddParentMapHandlers(this);
        }

        public Location Location
        {
            get { return (Location)GetValue(MapPanel.LocationProperty); }
            set { SetValue(MapPanel.LocationProperty, value); }
        }
    }
}
