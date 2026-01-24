#if WPF
using System.Windows;
#elif UWP
using Windows.UI.Xaml;
#elif WINUI
using Microsoft.UI.Xaml;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    /// <summary>
    /// A path element with a Data property that holds a Geometry in view coordinates or
    /// projected map coordinates that are relative to an origin Location.
    /// </summary>
    public partial class MapPath : IMapElement
    {
        public static readonly DependencyProperty LocationProperty =
            DependencyPropertyHelper.Register<MapPath, Location>(nameof(Location), null,
                (path, oldValue, newValue) => path.UpdateData());

        /// <summary>
        /// Gets or sets a Location that is used as
        /// - either the origin point of a geometry specified in projected map coordinates (meters)
        /// - or as an optional anchor point to constrain the view position of MapPaths with multiple
        ///   Locations (like MapPolyline or MapPolygon) to the visible map viewport, as done
        ///   for elements where the MapPanel.Location property is set.
        /// </summary>
        public Location Location
        {
            get => (Location)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        /// <summary>
        /// Implements IMapElement.ParentMap.
        /// </summary>
        public MapBase ParentMap
        {
            get;
            set
            {
                if (field != null)
                {
                    field.ViewportChanged -= OnViewportChanged;
                }

                field = value;

                if (field != null)
                {
                    field.ViewportChanged += OnViewportChanged;
                }

                UpdateData();
            }
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            UpdateData();
        }

        protected virtual void UpdateData()
        {
            if (ParentMap != null && Location != null && Data != null)
            {
                SetDataTransform(ParentMap.GetMapToViewTransform(Location));
            }

            MapPanel.SetLocation(this, Location);
        }

        protected double GetLongitudeOffset(Location location)
        {
            var longitudeOffset = 0d;

            if (ParentMap.MapProjection.IsNormalCylindrical && location != null)
            {
                var position = ParentMap.LocationToView(location);

                if (position.HasValue && !ParentMap.InsideViewBounds(position.Value))
                {
                    longitudeOffset = ParentMap.CoerceLongitude(location.Longitude) - location.Longitude;
                }
            }

            return longitudeOffset;
        }

        protected Point? LocationToMap(Location location, double longitudeOffset)
        {
            var point = ParentMap.MapProjection.LocationToMap(location.Latitude, location.Longitude + longitudeOffset);

            if (point.HasValue)
            {
                if (point.Value.Y == double.PositiveInfinity)
                {
                    point = new Point(point.Value.X, 1e9);
                }
                else if (point.Value.Y == double.NegativeInfinity)
                {
                    point = new Point(point.Value.X, -1e9);
                }
            }

            return point;
        }

        protected Point? LocationToView(Location location, double longitudeOffset)
        {
            var point = LocationToMap(location, longitudeOffset);

            if (point.HasValue)
            {
                point = ParentMap.ViewTransform.MapToView(point.Value);
            }

            return point;
        }
    }
}
