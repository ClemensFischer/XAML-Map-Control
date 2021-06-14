// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#elif WINDOWS_UWP
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
            new PropertyMetadata(null, (o, e) => ((MapPath)o).UpdateData()));

        private MapBase parentMap;

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

            if (parentMap != null && Data != null && Location != null)
            {
                var scale = parentMap.GetScale(Location);
                var transform = new Matrix(scale.X, 0d, 0d, scale.Y, 0d, 0d);

                transform.Rotate(parentMap.ViewTransform.Rotation);

                Data.Transform = new MatrixTransform { Matrix = transform };
            }
        }

        #region Methods used only by derived classes MapPolyline, MapPolygon and MapMultiPolygon

        protected double GetLongitudeOffset(Location location)
        {
            var longitudeOffset = 0d;

            if (location != null && parentMap.MapProjection.IsNormalCylindrical)
            {
                var pos = parentMap.LocationToView(location);

                if (pos.X < 0d || pos.X > parentMap.RenderSize.Width ||
                    pos.Y < 0d || pos.Y > parentMap.RenderSize.Height)
                {
                    longitudeOffset = parentMap.ConstrainedLongitude(location.Longitude) - location.Longitude;
                }
            }

            return longitudeOffset;
        }

        protected Point LocationToMap(Location location, double longitudeOffset)
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

        protected Point LocationToView(Location location, double longitudeOffset)
        {
            return parentMap.ViewTransform.MapToView(LocationToMap(location, longitudeOffset));
        }

        #endregion
    }
}
