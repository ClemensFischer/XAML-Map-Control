// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
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
    /// Base class for MapPolyline and MapPolygon.
    /// </summary>
    public abstract partial class MapShape : IMapElement
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            nameof(Location), typeof(Location), typeof(MapShape),
            new PropertyMetadata(null, (o, e) => ((MapShape)o).LocationPropertyChanged()));

        /// <summary>
        /// Gets or sets an optional Location to constrain the viewport position to the visible
        /// map viewport, as done for elements where the MapPanel.Location property is set.
        /// </summary>
        public Location Location
        {
            get { return (Location)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        private void LocationPropertyChanged()
        {
            if (parentMap != null)
            {
                UpdateData();
            }
        }

        private MapBase parentMap;

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

        protected abstract void UpdateData();

        protected MapShape()
            : this(new PathGeometry())
        {
        }

        protected MapShape(Geometry data)
        {
            Data = data;

            MapPanel.InitMapElement(this);
        }

        protected Point LocationToPoint(Location location)
        {
            var point = parentMap.MapProjection.LocationToPoint(location);

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

        protected Point LocationToViewportPoint(Location location)
        {
            return parentMap.MapProjection.ViewportTransform.Transform(LocationToPoint(location));
        }

        protected double GetLongitudeOffset()
        {
            var longitudeOffset = 0d;

            if (parentMap.MapProjection.IsNormalCylindrical && Location != null)
            {
                var viewportPosition = LocationToViewportPoint(Location);

                if (viewportPosition.X < 0d || viewportPosition.X > parentMap.RenderSize.Width ||
                    viewportPosition.Y < 0d || viewportPosition.Y > parentMap.RenderSize.Height)
                {
                    var nearestLongitude = Location.NearestLongitude(Location.Longitude, parentMap.Center.Longitude);

                    longitudeOffset = nearestLongitude - Location.Longitude;
                }
            }

            return longitudeOffset;
        }
    }
}
