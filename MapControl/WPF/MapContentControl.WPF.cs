using System.Windows;
using System.Windows.Controls;

namespace MapControl
{
    /// <summary>
    /// ContentControl placed on a MapPanel at a geographic location specified by the Location property.
    /// </summary>
    public class MapContentControl : ContentControl
    {
        public static readonly DependencyProperty AutoCollapseProperty =
            DependencyPropertyHelper.AddOwner<MapContentControl, bool>(MapPanel.AutoCollapseProperty);

        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.AddOwner<MapContentControl, Location>(MapPanel.LocationProperty);

        static MapContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapContentControl), new FrameworkPropertyMetadata(typeof(MapContentControl)));
        }

        /// <summary>
        /// Gets/sets MapPanel.AutoCollapse.
        /// </summary>
        public bool AutoCollapse
        {
            get => (bool)GetValue(AutoCollapseProperty);
            set => SetValue(AutoCollapseProperty, value);
        }

        /// <summary>
        /// Gets/sets MapPanel.Location.
        /// </summary>
        public Location Location
        {
            get => (Location)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }
    }

    /// <summary>
    /// MapContentControl with a Pushpin Style.
    /// </summary>
    public class Pushpin : MapContentControl
    {
        static Pushpin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Pushpin), new FrameworkPropertyMetadata(typeof(Pushpin)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyPropertyHelper.Register<Pushpin, CornerRadius>(nameof(CornerRadius));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
