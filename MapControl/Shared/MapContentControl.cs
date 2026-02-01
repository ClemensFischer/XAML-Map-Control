#if WPF
using System.Windows;
using System.Windows.Controls;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#elif AVALONIA
using Avalonia.Controls;
#endif

namespace MapControl
{
    /// <summary>
    /// ContentControl placed on a MapPanel at a geographic location specified by the Location property.
    /// </summary>
    public partial class MapContentControl : ContentControl
    {
        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.Register<MapContentControl, Location>(nameof(Location), null,
                (control, oldValue, newValue) => MapPanel.SetLocation(control, newValue));

        public static readonly DependencyProperty AutoCollapseProperty =
            DependencyPropertyHelper.Register<MapContentControl, bool>(nameof(AutoCollapse), false,
                (control, oldValue, newValue) => MapPanel.SetAutoCollapse(control, newValue));

        /// <summary>
        /// Gets/sets MapPanel.Location.
        /// </summary>
        public Location Location
        {
            get => (Location)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        /// <summary>
        /// Gets/sets MapPanel.AutoCollapse.
        /// </summary>
        public bool AutoCollapse
        {
            get => (bool)GetValue(AutoCollapseProperty);
            set => SetValue(AutoCollapseProperty, value);
        }
    }

    /// <summary>
    /// MapContentControl with a Pushpin Style.
    /// </summary>
    public partial class Pushpin : MapContentControl
    {
    }
}
