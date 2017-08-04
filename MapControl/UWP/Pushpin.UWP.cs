// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.UI.Xaml.Controls;

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
