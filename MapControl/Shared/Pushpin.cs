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
        public static readonly DependencyProperty LocationProperty =
#if WINDOWS_UWP
            DependencyProperty.Register(
                nameof(Location), typeof(Location), typeof(Pushpin),
                new PropertyMetadata(null, (o, e) => MapPanel.SetLocation((FrameworkElement)o, (Location)e.NewValue)));
#else
            MapPanel.LocationProperty.AddOwner(typeof(Pushpin));
#endif
        public Pushpin()
        {
            DefaultStyleKey = typeof(Pushpin);

            MapPanel.InitMapElement(this);
        }

        public Location Location
        {
            get { return (Location)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }
    }
}
