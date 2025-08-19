using Avalonia;
using Avalonia.Controls;

namespace MapControl
{
    /// <summary>
    /// ContentControl placed on a MapPanel at a geographic location specified by the Location property.
    /// </summary>
    public class MapContentControl : ContentControl
    {
        public static readonly StyledProperty<bool> AutoCollapseProperty =
            MapPanel.AutoCollapseProperty.AddOwner<MapContentControl>();

        public static readonly StyledProperty<Location> LocationProperty =
            MapPanel.LocationProperty.AddOwner<MapContentControl>();

        /// <summary>
        /// Gets/sets MapPanel.AutoCollapse.
        /// </summary>
        public bool AutoCollapse
        {
            get => GetValue(AutoCollapseProperty);
            set => SetValue(AutoCollapseProperty, value);
        }

        /// <summary>
        /// Gets/sets MapPanel.Location.
        /// </summary>
        public Location Location
        {
            get => GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }
    }

    /// <summary>
    /// MapContentControl with a Pushpin Style.
    /// </summary>
    public class Pushpin : MapContentControl
    {
    }
}
