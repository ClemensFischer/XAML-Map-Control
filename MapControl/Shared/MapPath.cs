// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// A path element with a Data property that holds a Geometry in view coordinates or
    /// cartesian map coordinates that are relative to an origin Location.
    /// </summary>
    public partial class MapPath : IMapElement
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            nameof(Location), typeof(Location), typeof(MapPath),
            new PropertyMetadata(null, (o, e) => ((MapPath)o).LocationOrViewportChanged()));

        private MapBase parentMap;
        private MatrixTransform dataTransform;
        private double longitudeOffset;

        public MapPath()
        {
            MapPanel.InitMapElement(this);
        }

        /// <summary>
        /// Gets or sets a Location that is used as
        /// - either the origin point of a geometry specified in cartesian map units (meters)
        /// - or as an optional value to constrain the view position of MapPaths with multiple
        ///   Locations (like MapPolyline or MapPolygon) to the visible map viewport, as done
        ///   for elements where the MapPanel.Location property is set.
        /// </summary>
        public Location Location
        {
            get { return (Location)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public MapBase ParentMap
        {
            get { return parentMap; }
            set
            {
                if (parentMap != null)
                {
                    parentMap.ViewportChanged -= OnViewportChanged;
                }

                parentMap = value;

                if (parentMap != null)
                {
                    parentMap.ViewportChanged += OnViewportChanged;
                }

                LocationOrViewportChanged();
            }
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            LocationOrViewportChanged();
        }

        private void LocationOrViewportChanged()
        {
            longitudeOffset = 0d;

            if (parentMap != null && parentMap.MapProjection.IsNormalCylindrical && Location != null)
            {
                var viewPos = LocationToView(Location);

                if (viewPos.X < 0d || viewPos.X > parentMap.RenderSize.Width ||
                    viewPos.Y < 0d || viewPos.Y > parentMap.RenderSize.Height)
                {
                    longitudeOffset = Location.NearestLongitude(Location.Longitude, parentMap.Center.Longitude) - Location.Longitude;
                }
            }

            UpdateData();
        }

        protected virtual void UpdateData()
        {
            if (parentMap != null && Data != null && Location != null)
            {
                if (dataTransform == null)
                {
                    Data.Transform = dataTransform = new MatrixTransform();
                }

                var viewPos = LocationToView(Location);
                var scale = parentMap.GetScale(Location);
                var transform = new Matrix(scale.X, 0d, 0d, scale.Y, 0d, 0d);
                transform.Rotate(parentMap.ViewTransform.Rotation);
                transform.Translate(viewPos.X, viewPos.Y);
                dataTransform.Matrix = transform;
            }
        }

        protected Point LocationToMap(Location location)
        {
            if (longitudeOffset != 0d)
            {
                location = new Location(location.Latitude, location.Longitude + longitudeOffset);
            }

            var point = parentMap.MapProjection.LocationToMap(location);

            if (point.Y == double.PositiveInfinity)
            {
                point.Y = 1e9;
            }
            else if (point.X == double.NegativeInfinity)
            {
                point.Y = -1e9;
            }

            return point;
        }

        protected Point LocationToView(Location location)
        {
            return parentMap.ViewTransform.MapToView(LocationToMap(location));
        }
    }
}
