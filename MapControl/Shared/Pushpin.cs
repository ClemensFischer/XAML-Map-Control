// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace MapControl
{
    /// <summary>
    /// Pushpin at a geographic location specified by the Location property.
    /// </summary>
    public class Pushpin : ContentControl
    {
#if WINDOWS_UWP
        public static readonly DependencyProperty AutoCollapseProperty = DependencyProperty.Register(
            nameof(AutoCollapse), typeof(bool), typeof(Pushpin),
            new PropertyMetadata(false, (o, e) => MapPanel.SetAutoCollapse((FrameworkElement)o, (bool)e.NewValue)));

        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            nameof(Location), typeof(Location), typeof(Pushpin),
            new PropertyMetadata(null, (o, e) => MapPanel.SetLocation((FrameworkElement)o, (Location)e.NewValue)));
#else
        public static readonly DependencyProperty AutoCollapseProperty = MapPanel.AutoCollapseProperty.AddOwner(typeof(Pushpin));

        public static readonly DependencyProperty LocationProperty = MapPanel.LocationProperty.AddOwner(typeof(Pushpin));
#endif
        public Pushpin()
        {
            DefaultStyleKey = typeof(Pushpin);

            MapPanel.InitMapElement(this);
        }

        /// <summary>
        /// Wrapper for the MapPanel.AutoCollapse attached property.
        /// </summary>
        public bool AutoCollapse
        {
            get { return (bool)GetValue(AutoCollapseProperty); }
            set { SetValue(AutoCollapseProperty, value); }
        }

        /// <summary>
        /// Wrapper for the MapPanel.Location attached property.
        /// </summary>
        public Location Location
        {
            get { return (Location)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }
    }
}
