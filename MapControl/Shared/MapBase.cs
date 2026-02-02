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
    /// The map control. Displays map content provided by one or more layers like MapTileLayer,
    /// WmtsTileLayer or WmsImageLayer. The visible map area is defined by the Center and
    /// ZoomLevel properties. The map can be rotated by an angle provided by the Heading property.
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

        public static TimeSpan ImageFadeDuration { get; set; } = TimeSpan.FromSeconds(0.2);

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyPropertyHelper.Register<MapBase, TimeSpan>(nameof(AnimationDuration), TimeSpan.FromSeconds(0.3));

        public static readonly DependencyProperty MapProjectionProperty =
            DependencyPropertyHelper.Register<MapBase, MapProjection>(nameof(MapProjection), new WebMercatorProjection(),
                (map, oldValue, newValue) => map.MapProjectionPropertyChanged(newValue));

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
        /// Gets the ViewTransform instance that is used to transform between
        /// projected map coordinates and view coordinates.
        /// </summary>
        public ViewTransform ViewTransform { get; } = new ViewTransform();

        /// <summary>
        /// Gets a transform Matrix from meters to view coordinates for scaling and rotating
        /// at the specified geographic coordinates.
        /// </summary>
        public Matrix GetMapToViewTransform(double latitude, double longitude)
        {
            var transform = MapProjection.RelativeTransform(latitude, longitude);
            transform.Scale(ViewTransform.Scale, ViewTransform.Scale);
            transform.Rotate(ViewTransform.Rotation);
            return transform;
        }

        /// <summary>
        /// Gets a transform Matrix from meters to view coordinates for scaling and rotating
        /// at the specified Location.
        /// </summary>
        public Matrix GetMapToViewTransform(Location location) => GetMapToViewTransform(location.Latitude, location.Longitude);

        /// <summary>
        /// Transforms geographic coordinates to a Point in view coordinates.
        /// </summary>
        public Point LocationToView(double latitude, double longitude) => ViewTransform.MapToView(MapProjection.LocationToMap(latitude, longitude));

        /// <summary>
        /// Transforms a Location in geographic coordinates to a Point in view coordinates.
        /// </summary>
        public Point LocationToView(Location location) => LocationToView(location.Latitude, location.Longitude);

        /// <summary>
        /// Transforms a Point in view coordinates to a Location in geographic coordinates.
        /// </summary>
        public Location ViewToLocation(Point point) => MapProjection.MapToLocation(ViewTransform.ViewToMap(point));

        /// <summary>
        /// Sets a temporary center point in view coordinates for scaling and rotation transformations.
        /// This center point is automatically reset when the Center property is set by application code
        /// or by the methods TranslateMap, TransformMap, ZoomMap and ZoomToBounds.
        /// </summary>
        public void SetTransformCenter(Point center)
        {
            viewCenter = center;
            transformCenter = ViewToLocation(center);
        }

        /// <summary>
        /// Resets the temporary transform center point set by SetTransformCenter.
        /// </summary>
        public void ResetTransformCenter()
        {
            viewCenter = new Point(ActualWidth / 2d, ActualHeight / 2d);
            transformCenter = null;
        }

        /// <summary>
        /// Changes the Center property according to the specified translation in view coordinates.
        /// </summary>
        public void TranslateMap(Point translation)
        {
            if (translation.X != 0d || translation.Y != 0d)
            {
                Center = ViewToLocation(new Point(viewCenter.X - translation.X, viewCenter.Y - translation.Y));
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
        public void ZoomToBounds(BoundingBox bounds)
        {
            (var rect, var _) = MapProjection.BoundingBoxToMap(bounds);
            var scale = Math.Min(ActualWidth / rect.Width, ActualHeight / rect.Height);
            TargetZoomLevel = ScaleToZoomLevel(scale);
            TargetCenter = new Location((bounds.South + bounds.North) / 2d, (bounds.West + bounds.East) / 2d);
            TargetHeading = 0d;
        }

        internal bool InsideViewBounds(Point point)
        {
            return point.X >= 0d && point.Y >= 0d && point.X <= ActualWidth && point.Y <= ActualHeight;
        }

        internal double NearestLongitude(double longitude)
        {
            longitude = Location.NormalizeLongitude(longitude);
            
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
                center = new Location(0d, 0d);
            }
            else if (center.Latitude < -maxLatitude || center.Latitude > maxLatitude ||
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

            if (projection.IsNormalCylindrical)
            {
                maxLatitude = projection.MapToLocation(0d, 180d * MapProjection.Wgs84MeterPerDegree).Latitude;
                Center = CoerceCenterProperty(Center);
            }

            ResetTransformCenter();
            UpdateTransform(false, true);
        }

        private void UpdateTransform(bool resetTransformCenter = false, bool projectionChanged = false)
        {
            var transformCenterChanged = false;
            var viewScale = ZoomLevelToScale(ZoomLevel);
            var mapCenter = MapProjection.LocationToMap(transformCenter ?? Center);

            ViewTransform.SetTransform(mapCenter, viewCenter, viewScale, -Heading);

            if (transformCenter != null)
            {
                var center = ViewToLocation(new Point(ActualWidth / 2d, ActualHeight / 2d));
                var latitude = center.Latitude;
                var longitude = Location.NormalizeLongitude(center.Longitude);

                if (latitude < -maxLatitude || latitude > maxLatitude)
                {
                    latitude = Math.Min(Math.Max(latitude, -maxLatitude), maxLatitude);
                    resetTransformCenter = true;
                }

                if (!center.Equals(latitude, longitude))
                {
                    center = new Location(latitude, longitude);
                }

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
                    mapCenter = MapProjection.LocationToMap(center);
                    ViewTransform.SetTransform(mapCenter, viewCenter, viewScale, -Heading);
                }
            }

            ViewScale = ViewTransform.Scale;

            // Check if view center has moved across 180° longitude.
            //
            transformCenterChanged = transformCenterChanged || Math.Abs(Center.Longitude - centerLongitude) > 180d;
            centerLongitude = Center.Longitude;

            OnViewportChanged(new ViewportChangedEventArgs(projectionChanged, transformCenterChanged));
        }

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            base.OnViewportChanged(e);
            ViewportChanged?.Invoke(this, e);
        }
    }
}
