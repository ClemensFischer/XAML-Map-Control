using System;
#if WPF
using System.Windows;
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using Avalonia;
using Brush = Avalonia.Media.IBrush;
#endif

namespace MapControl
{
    /// <summary>
    /// The map control. Displays map content provided by one or more tile or image layers,
    /// such as MapTileLayerBase or MapImageLayer instances.
    /// The visible map area is defined by the Center and ZoomLevel properties.
    /// The map can be rotated by an angle that is given by the Heading property.
    /// MapBase can contain map overlay child elements like other MapPanels or MapItemsControls.
    /// </summary>
    public partial class MapBase : MapPanel
    {
        public static double ZoomLevelToScale(double zoomLevel)
        {
            return 256d * Math.Pow(2d, zoomLevel) / (360d * MapProjection.Wgs84MeterPerDegree);
        }

        public static double ScaleToZoomLevel(double scale)
        {
            return Math.Log(scale * 360d * MapProjection.Wgs84MeterPerDegree / 256d, 2d);
        }

        public static TimeSpan ImageFadeDuration { get; set; } = TimeSpan.FromSeconds(0.1);

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyPropertyHelper.Register<MapBase, TimeSpan>(nameof(AnimationDuration), TimeSpan.FromSeconds(0.3));

        public static readonly DependencyProperty MapProjectionProperty =
            DependencyPropertyHelper.Register<MapBase, MapProjection>(nameof(MapProjection), new WebMercatorProjection(),
                (map, oldValue, newValue) => map.MapProjectionPropertyChanged(newValue));

        public static readonly DependencyProperty ProjectionCenterProperty =
            DependencyPropertyHelper.Register<MapBase, Location>(nameof(ProjectionCenter), null,
                (map, oldValue, newValue) => map.ProjectionCenterPropertyChanged());

        private Location transformCenter;
        private Point viewCenter;
        private double centerLongitude;
        private double maxLatitude = 85.05112878; // default WebMercatorProjection
        private bool internalPropertyChange;

        /// <summary>
        /// Raised when the current map viewport has changed.
        /// </summary>
        public event EventHandler<ViewportChangedEventArgs> ViewportChanged;

        /// <summary>
        /// Gets or sets the map foreground Brush.
        /// </summary>
        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the duration of the Center, ZoomLevel and Heading animations.
        /// The default value is 0.3 seconds.
        /// </summary>
        public TimeSpan AnimationDuration
        {
            get => (TimeSpan)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        /// <summary>
        /// Gets or sets the MapProjection used by the map control.
        /// </summary>
        public MapProjection MapProjection
        {
            get => (MapProjection)GetValue(MapProjectionProperty);
            set => SetValue(MapProjectionProperty, value);
        }

        /// <summary>
        /// Gets or sets an optional center (reference point) for azimuthal projections.
        /// If ProjectionCenter is null, the Center property value will be used instead.
        /// </summary>
        public Location ProjectionCenter
        {
            get => (Location)GetValue(ProjectionCenterProperty);
            set => SetValue(ProjectionCenterProperty, value);
        }

        /// <summary>
        /// Gets or sets the location of the center point of the map.
        /// </summary>
        public Location Center
        {
            get => (Location)GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }

        /// <summary>
        /// Gets or sets the target value of a Center animation.
        /// </summary>
        public Location TargetCenter
        {
            get => (Location)GetValue(TargetCenterProperty);
            set => SetValue(TargetCenterProperty, value);
        }

        /// <summary>
        /// Gets or sets the minimum value of the ZoomLevel and TargetZoomLevel properties.
        /// Must not be less than zero or greater than MaxZoomLevel. The default value is 1.
        /// </summary>
        public double MinZoomLevel
        {
            get => (double)GetValue(MinZoomLevelProperty);
            set => SetValue(MinZoomLevelProperty, value);
        }

        /// <summary>
        /// Gets or sets the maximum value of the ZoomLevel and TargetZoomLevel properties.
        /// Must not be less than MinZoomLevel. The default value is 20.
        /// </summary>
        public double MaxZoomLevel
        {
            get => (double)GetValue(MaxZoomLevelProperty);
            set => SetValue(MaxZoomLevelProperty, value);
        }

        /// <summary>
        /// Gets or sets the map zoom level.
        /// </summary>
        public double ZoomLevel
        {
            get => (double)GetValue(ZoomLevelProperty);
            set => SetValue(ZoomLevelProperty, value);
        }

        /// <summary>
        /// Gets or sets the target value of a ZoomLevel animation.
        /// </summary>
        public double TargetZoomLevel
        {
            get => (double)GetValue(TargetZoomLevelProperty);
            set => SetValue(TargetZoomLevelProperty, value);
        }

        /// <summary>
        /// Gets or sets the map heading, a counter-clockwise rotation angle in degrees.
        /// </summary>
        public double Heading
        {
            get => (double)GetValue(HeadingProperty);
            set => SetValue(HeadingProperty, value);
        }

        /// <summary>
        /// Gets or sets the target value of a Heading animation.
        /// </summary>
        public double TargetHeading
        {
            get => (double)GetValue(TargetHeadingProperty);
            set => SetValue(TargetHeadingProperty, value);
        }

        /// <summary>
        /// Gets the ViewTransform instance that is used to transform between projected
        /// map coordinates and view coordinates.
        /// </summary>
        public ViewTransform ViewTransform { get; } = new ViewTransform();

        /// <summary>
        /// Gets the map scale as horizontal and vertical scaling factors from meters to
        /// view coordinates at the specified geographic coordinates.
        /// </summary>
        public Point GetScale(double latitude, double longitude)
        {
            return ViewTransform.GetMapScale(MapProjection.GetRelativeScale(latitude, longitude));
        }

        /// <summary>
        /// Gets the map scale as horizontal and vertical scaling factors from meters to
        /// view coordinates at the specified location.
        /// </summary>
        public Point GetScale(Location location)
        {
            return ViewTransform.GetMapScale(MapProjection.GetRelativeScale(location));
        }

        /// <summary>
        /// Gets a transform Matrix from meters to view coordinates for scaling and rotating
        /// objects that are anchored at the specified Location.
        /// </summary>
        public Matrix GetMapTransform(Location location)
        {
            return ViewTransform.GetMapTransform(MapProjection.GetRelativeScale(location));
        }

        /// <summary>
        /// Transforms geographic coordinates to a Point in view coordinates.
        /// </summary>
        public Point? LocationToView(double latitude, double longitude)
        {
            var point = MapProjection.LocationToMap(latitude, longitude);

            if (point.HasValue)
            {
                point = ViewTransform.MapToView(point.Value);
            }

            return point;
        }

        /// <summary>
        /// Transforms a Location in geographic coordinates to a Point in view coordinates.
        /// </summary>
        public Point? LocationToView(Location location)
        {
            return LocationToView(location.Latitude, location.Longitude);
        }

        /// <summary>
        /// Transforms a Point in view coordinates to a Location in geographic coordinates.
        /// </summary>
        public Location ViewToLocation(Point point)
        {
            return MapProjection.MapToLocation(ViewTransform.ViewToMap(point));
        }

        /// <summary>
        /// Gets a BoundingBox in geographic coordinates that covers a Rect in view coordinates.
        /// </summary>
        public BoundingBox ViewRectToBoundingBox(Rect rect)
        {
            var p1 = ViewTransform.ViewToMap(new Point(rect.X, rect.Y));
            var p2 = ViewTransform.ViewToMap(new Point(rect.X, rect.Y + rect.Height));
            var p3 = ViewTransform.ViewToMap(new Point(rect.X + rect.Width, rect.Y));
            var p4 = ViewTransform.ViewToMap(new Point(rect.X + rect.Width, rect.Y + rect.Height));

            var x1 = Math.Min(p1.X, Math.Min(p2.X, Math.Min(p3.X, p4.X)));
            var y1 = Math.Min(p1.Y, Math.Min(p2.Y, Math.Min(p3.Y, p4.Y)));
            var x2 = Math.Max(p1.X, Math.Max(p2.X, Math.Max(p3.X, p4.X)));
            var y2 = Math.Max(p1.Y, Math.Max(p2.Y, Math.Max(p3.Y, p4.Y)));

            return MapProjection.MapToBoundingBox(new Rect(x1, y1, x2 - x1, y2 - y1));
        }

        /// <summary>
        /// Sets a temporary center point in view coordinates for scaling and rotation transformations.
        /// This center point is automatically reset when the Center property is set by application code
        /// or by the methods TranslateMap, TransformMap, ZoomMap and ZoomToBounds.
        /// </summary>
        public void SetTransformCenter(Point center)
        {
            transformCenter = ViewToLocation(center);
            viewCenter = transformCenter != null ? center : new Point(ActualWidth / 2d, ActualHeight / 2d);
        }

        /// <summary>
        /// Resets the temporary transform center point set by SetTransformCenter.
        /// </summary>
        public void ResetTransformCenter()
        {
            transformCenter = null;
            viewCenter = new Point(ActualWidth / 2d, ActualHeight / 2d);
        }

        /// <summary>
        /// Changes the Center property according to the specified translation in view coordinates.
        /// </summary>
        public void TranslateMap(Point translation)
        {
            if (translation.X != 0d || translation.Y != 0d)
            {
                var center = ViewToLocation(new Point(viewCenter.X - translation.X, viewCenter.Y - translation.Y));

                if (center != null)
                {
                    Center = center;
                }
            }
        }

        /// <summary>
        /// Changes the Center, Heading and ZoomLevel properties according to the specified
        /// view coordinate translation, rotation and scale delta values. Rotation and scaling
        /// is performed relative to the specified center point in view coordinates.
        /// </summary>
        public void TransformMap(Point center, Point translation, double rotation, double scale)
        {
            if (rotation == 0d && scale == 1d)
            {
                TranslateMap(translation);
            }
            else
            {
                SetTransformCenter(center);
                viewCenter = new Point(viewCenter.X + translation.X, viewCenter.Y + translation.Y);

                if (rotation != 0d)
                {
                    var heading = CoerceHeadingProperty(Heading - rotation);
                    SetValueInternal(HeadingProperty, heading);
                    SetValueInternal(TargetHeadingProperty, heading);
                }

                if (scale != 1d)
                {
                    var zoomLevel = CoerceZoomLevelProperty(ZoomLevel + Math.Log(scale, 2d));
                    SetValueInternal(ZoomLevelProperty, zoomLevel);
                    SetValueInternal(TargetZoomLevelProperty, zoomLevel);
                }

                UpdateTransform(true);
            }
        }

        /// <summary>
        /// Sets the ZoomLevel or TargetZoomLevel property while retaining
        /// the specified center point in view coordinates.
        /// </summary>
        public void ZoomMap(Point center, double zoomLevel, bool animated = true)
        {
            zoomLevel = CoerceZoomLevelProperty(zoomLevel);

            if (animated || zoomLevelAnimation != null)
            {
                if (TargetZoomLevel != zoomLevel)
                {
                    SetTransformCenter(center);
                    TargetZoomLevel = zoomLevel;
                }
            }
            else
            {
                if (ZoomLevel != zoomLevel)
                {
                    SetTransformCenter(center);
                    SetValueInternal(ZoomLevelProperty, zoomLevel);
                    SetValueInternal(TargetZoomLevelProperty, zoomLevel);
                    UpdateTransform(true);
                }
            }
        }

        /// <summary>
        /// Sets the TargetZoomLevel and TargetCenter properties so that the specified BoundingBox
        /// fits into the current view. The TargetHeading property is set to zero.
        /// </summary>
        public void ZoomToBounds(BoundingBox boundingBox)
        {
            var mapRect = MapProjection.BoundingBoxToMap(boundingBox);

            if (mapRect.HasValue)
            {
                var targetCenter = MapProjection.MapToLocation(
                    mapRect.Value.X + mapRect.Value.Width / 2d,
                    mapRect.Value.Y + mapRect.Value.Height / 2d);

                if (targetCenter != null)
                {
                    var scale = Math.Min(ActualWidth / mapRect.Value.Width, ActualHeight / mapRect.Value.Height);
                    TargetZoomLevel = ScaleToZoomLevel(scale);
                    TargetCenter = targetCenter;
                    TargetHeading = 0d;
                }
            }
        }

        internal bool InsideViewBounds(Point point)
        {
            return point.X >= 0d && point.Y >= 0d && point.X <= ActualWidth && point.Y <= ActualHeight;
        }

        internal double CoerceLongitude(double longitude)
        {
            var offset = longitude - Center.Longitude;

            if (offset > 180d)
            {
                longitude = Center.Longitude + (offset % 360d) - 360d;
            }
            else if (offset < -180d)
            {
                longitude = Center.Longitude + (offset % 360d) + 360d;
            }

            return longitude;
        }

        private Location CoerceCenterProperty(Location center)
        {
            if (center == null)
            {
                center = new Location();
            }
            else if (
                center.Latitude < -maxLatitude || center.Latitude > maxLatitude ||
                center.Longitude < -180d || center.Longitude > 180d)
            {
                center = new Location(
                    Math.Min(Math.Max(center.Latitude, -maxLatitude), maxLatitude),
                    Location.NormalizeLongitude(center.Longitude));
            }

            return center;
        }

        private double CoerceMinZoomLevelProperty(double minZoomLevel)
        {
            return Math.Min(Math.Max(minZoomLevel, 0d), MaxZoomLevel);
        }

        private double CoerceMaxZoomLevelProperty(double maxZoomLevel)
        {
            return Math.Max(maxZoomLevel, MinZoomLevel);
        }

        private double CoerceZoomLevelProperty(double zoomLevel)
        {
            return Math.Min(Math.Max(zoomLevel, MinZoomLevel), MaxZoomLevel);
        }

        private double CoerceHeadingProperty(double heading)
        {
            return ((heading % 360d) + 360d) % 360d;
        }

        private void SetValueInternal(DependencyProperty property, object value)
        {
            internalPropertyChange = true;
            SetValue(property, value);
            internalPropertyChange = false;
        }

        private void MapProjectionPropertyChanged(MapProjection projection)
        {
            maxLatitude = 90d;

            if (projection.Type <= MapProjectionType.NormalCylindrical)
            {
                var maxLocation = projection.MapToLocation(0d, 180d * MapProjection.Wgs84MeterPerDegree);

                if (maxLocation != null && maxLocation.Latitude < 90d)
                {
                    maxLatitude = maxLocation.Latitude;

                    Center = CoerceCenterProperty(Center);
                }
            }

            ResetTransformCenter();
            UpdateTransform(false, true);
        }

        private void ProjectionCenterPropertyChanged()
        {
            ResetTransformCenter();
            UpdateTransform();
        }

        private void UpdateTransform(bool resetTransformCenter = false, bool projectionChanged = false)
        {
            var transformCenterChanged = false;
            var viewScale = ZoomLevelToScale(ZoomLevel);
            var projection = MapProjection;

            projection.Center = ProjectionCenter ?? Center;

            var mapCenter = projection.LocationToMap(transformCenter ?? Center);

            if (mapCenter.HasValue)
            {
                ViewTransform.SetTransform(mapCenter.Value, viewCenter, viewScale, -Heading);

                if (transformCenter != null)
                {
                    var center = ViewToLocation(new Point(ActualWidth / 2d, ActualHeight / 2d));

                    if (center != null)
                    {
                        var centerLatitude = center.Latitude;
                        var centerLongitude = Location.NormalizeLongitude(center.Longitude);

                        if (centerLatitude < -maxLatitude || centerLatitude > maxLatitude)
                        {
                            centerLatitude = Math.Min(Math.Max(centerLatitude, -maxLatitude), maxLatitude);
                            resetTransformCenter = true;
                        }

                        center = new Location(centerLatitude, centerLongitude);

                        SetValueInternal(CenterProperty, center);

                        if (centerAnimation == null)
                        {
                            SetValueInternal(TargetCenterProperty, center);
                        }

                        if (resetTransformCenter)
                        {
                            // Check if transform center has moved across 180° longitude.
                            //
                            transformCenterChanged = Math.Abs(center.Longitude - transformCenter.Longitude) > 180d;

                            ResetTransformCenter();

                            projection.Center = ProjectionCenter ?? Center;

                            mapCenter = projection.LocationToMap(center);

                            if (mapCenter.HasValue)
                            {
                                ViewTransform.SetTransform(mapCenter.Value, viewCenter, viewScale, -Heading);
                            }
                        }
                    }
                }

                ViewScale = ViewTransform.Scale;

                // Check if view center has moved across 180° longitude.
                //
                transformCenterChanged = transformCenterChanged || Math.Abs(Center.Longitude - centerLongitude) > 180d;
                centerLongitude = Center.Longitude;

                OnViewportChanged(new ViewportChangedEventArgs(projectionChanged, transformCenterChanged));
            }
        }

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            base.OnViewportChanged(e);

            ViewportChanged?.Invoke(this, e);
        }
    }
}
