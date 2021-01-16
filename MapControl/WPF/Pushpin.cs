// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;

namespace MapControl
{
    /// <summary>
    /// Pushpin at a geographic location specified by the Location property.
    /// </summary>
    public class Pushpin : ContentControl
    {
        public static readonly DependencyProperty AutoCollapseProperty = MapPanel.AutoCollapseProperty.AddOwner(typeof(Pushpin));

        public static readonly DependencyProperty LocationProperty = MapPanel.LocationProperty.AddOwner(typeof(Pushpin));

        static Pushpin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Pushpin), new FrameworkPropertyMetadata(typeof(Pushpin)));
        }

        /// <summary>
        /// Gets/sets MapPanel.AutoCollapse.
        /// </summary>
        public bool AutoCollapse
        {
            get { return (bool)GetValue(AutoCollapseProperty); }
            set { SetValue(AutoCollapseProperty, value); }
        }

        /// <summary>
        /// Gets/sets MapPanel.Location.
        /// </summary>
        public Location Location
        {
            get { return (Location)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }
    }
}
