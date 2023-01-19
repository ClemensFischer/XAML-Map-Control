// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#elif UWP
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
    /// projected map coordinates that are relative to an origin Location.
    /// </summary>
    public partial class MapPath : IMapElement
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            nameof(Location), typeof(Location), typeof(MapPath),
            new PropertyMetadata(null, (o, e) => ((MapPath)o).UpdateData()));

        private MapBase parentMap;

        /// <summary>
        /// Gets or sets a Location that is used as
        /// - either the origin point of a geometry specified in projected map coordinates (meters)
        /// - or as an optional value to constrain the view position of MapPaths with multiple
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
            get => parentMap;
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

                UpdateData();
            }
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            UpdateData();
        }

        protected virtual void UpdateData()
        {
            MapPanel.SetLocation(this, Location);

            if (parentMap != null && Location != null && Data != null)
            {
                var matrix = parentMap.GetMapTransform(Location);

                if (Data.Transform is MatrixTransform transform)
                {
                    transform.Matrix = matrix;
                }
                else
                {
                    Data.Transform = new MatrixTransform { Matrix = matrix };
                }
            }
        }

        #region Methods used only by derived classes MapPolyline, MapPolygon and MapMultiPolygon

        protected double GetLongitudeOffset(Location location)
        {
            var longitudeOffset = 0d;

            if (location != null && parentMap.MapProjection.Type <= MapProjectionType.NormalCylindrical)
            {
                var point = parentMap.LocationToView(location);

                if (point.HasValue &&
                    (point.Value.X < 0d || point.Value.X > parentMap.RenderSize.Width ||
                     point.Value.Y < 0d || point.Value.Y > parentMap.RenderSize.Height))
                {
                    longitudeOffset = parentMap.ConstrainedLongitude(location.Longitude) - location.Longitude;
                }
            }

            return longitudeOffset;
        }

        protected Point? LocationToMap(Location location, double longitudeOffset)
        {
            if (longitudeOffset != 0d)
            {
                location = new Location(location.Latitude, location.Longitude + longitudeOffset);
            }

            var point = parentMap.MapProjection.LocationToMap(location);

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

            if (!point.HasValue)
            {
                return null;
            }

            return parentMap.ViewTransform.MapToView(point.Value);
        }

        #endregion
    }
}
