// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    /// <summary>
    /// ContentControl placed on a MapPanel at a geographic location specified by the Location property.
    /// </summary>
    public class MapContentControl : ContentControl
    {
        public static readonly StyledProperty<bool> AutoCollapseProperty =
            DependencyPropertyHelper.AddOwner<MapContentControl, bool>(MapPanel.AutoCollapseProperty);

        public static readonly StyledProperty<Location> LocationProperty =
            DependencyPropertyHelper.AddOwner<MapContentControl, Location>(MapPanel.LocationProperty);

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
