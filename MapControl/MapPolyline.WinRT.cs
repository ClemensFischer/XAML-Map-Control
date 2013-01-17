// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;

namespace MapControl
{
    public partial class MapPolyline : Path
    {
        /// <summary>
        /// Property type declared as object instead of IEnumerable.
        /// See here: http://stackoverflow.com/q/10544084/1136211
        /// </summary>
        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register(
            "Locations", typeof(object), typeof(MapPolyline),
            new PropertyMetadata(null, LocationsPropertyChanged));

        public MapPolyline()
        {
            Data = Geometry;
            MapPanel.AddParentMapHandlers(this);
        }
    }
}
