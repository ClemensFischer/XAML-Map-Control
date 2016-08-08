// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;

namespace MapControl
{
    /// <summary>
    /// Displays a pushpin at a geographic location provided by the MapPanel.Location attached property.
    /// </summary>
    public class Pushpin : ContentControl
    {
        static Pushpin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(Pushpin), new FrameworkPropertyMetadata(typeof(Pushpin)));
        }

        public static readonly DependencyProperty LocationProperty =
            MapPanel.LocationProperty.AddOwner(typeof(Pushpin));

        public Location Location
        {
            get { return (Location)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }
    }
}
