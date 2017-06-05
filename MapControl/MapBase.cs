// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
#endif

namespace MapControl
{
    public interface IMapLayer : IMapElement
    {
        Brush MapBackground { get; }
        Brush MapForeground { get; }
    }

    /// <summary>
    /// The map control. Displays map content provided by one or more MapTileLayers or MapImageLayers.
    /// The visible map area is defined by the Center and ZoomLevel properties.
    /// The map can be rotated by an angle that is given by the Heading property.
    /// MapBase can contain map overlay child elements like other MapPanels or MapItemsControls.
    /// </summary>
    public partial class MapBase : MapPanel
    {
        private const double MaximumZoomLevel = 22d;

        public static readonly DependencyProperty MapProjectionProperty = DependencyProperty.Register(
            "MapProjection", typeof(MapProjection), typeof(MapBase),
            new PropertyMetadata(null, (o, e) => ((MapBase)o).ProjectionPropertyChanged()));

        public static readonly DependencyProperty MapLayerProperty = DependencyProperty.Register(
            "MapLayer", typeof(UIElement), typeof(MapBase),
            new PropertyMetadata(null, (o, e) => ((MapBase)o).MapLayerPropertyChanged((UIElement)e.OldValue, (UIElement)e.NewValue)));

        public static readonly DependencyProperty MinZoomLevelProperty = DependencyProperty.Register(
            "MinZoomLevel", typeof(double), typeof(MapBase),
            new PropertyMetadata(1d, (o, e) => ((MapBase)o).MinZoomLevelPropertyChanged((double)e.NewValue),
                (o, e) => ((MapBase)o).MinZoomLevelCoerceValue((double)e)));

        public static readonly DependencyProperty MaxZoomLevelProperty = DependencyProperty.Register(
            "MaxZoomLevel", typeof(double), typeof(MapBase),
            new PropertyMetadata(19d, (o, e) => ((MapBase)o).MaxZoomLevelPropertyChanged((double)e.NewValue),
                (o, e) => ((MapBase)o).MaxZoomLevelCoerceValue((double)e)));

        public static readonly DependencyProperty AnimationDurationProperty = DependencyProperty.Register(
            "AnimationDuration", typeof(TimeSpan), typeof(MapBase),
            new PropertyMetadata(TimeSpan.FromSeconds(0.3)));

        public static readonly DependencyProperty AnimationEasingFunctionProperty = DependencyProperty.Register(
            "AnimationEasingFunction", typeof(EasingFunctionBase), typeof(MapBase),
            new PropertyMetadata(new QuadraticEase { EasingMode = EasingMode.EaseOut }));

        public static readonly DependencyProperty TileFadeDurationProperty = DependencyProperty.Register(
            "TileFadeDuration", typeof(TimeSpan), typeof(MapBase),
            new PropertyMetadata(Tile.FadeDuration, (o, e) => Tile.FadeDuration = (TimeSpan)e.NewValue));

        internal static readonly DependencyProperty CenterPointProperty = DependencyProperty.Register(
            "CenterPoint", typeof(Point), typeof(MapBase),
            new PropertyMetadata(new Point(), (o, e) => ((MapBase)o).CenterPointPropertyChanged((Point)e.NewValue)));

        private PointAnimation centerAnimation;
        private DoubleAnimation zoomLevelAnimation;
        private DoubleAnimation headingAnimation;
        private Location transformCenter;
        private Point viewportCenter;
        private double centerLongitude;
        private bool internalPropertyChange;

        public MapBase()
        {
            Initialize();

            MapProjection = new WebMercatorProjection();
            ScaleRotateTransform.Children.Add(ScaleTransform);
            ScaleRotateTransform.Children.Add(RotateTransform);
        }

        partial void Initialize(); // Windows Runtime and Silverlight only
        partial void RemoveAnimation(DependencyProperty property); // WPF only

        /// <summary>
        /// Raised when the current viewport has changed.
        /// </summary>
        public event EventHandler<ViewportChangedEventArgs> ViewportChanged;

        /// <summary>
        /// Gets or sets the map foreground Brush.
        /// </summary>
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets the MapProjection used by the map control.
        /// </summary>
        public MapProjection MapProjection
        {
            get { return (MapProjection)GetValue(MapProjectionProperty); }
            set { SetValue(MapProjectionProperty, value); }
        }

        /// <summary>
        /// Gets or sets the base map layer, which is added as first element to the Children collection.
        /// If the layer implements IMapLayer (like MapTileLayer or MapImageLayer), its (non-null) MapBackground
        /// and MapForeground property values are used for the MapBase Background and Foreground properties.
        /// </summary>
        public UIElement MapLayer
        {
            get { return (UIElement)GetValue(MapLayerProperty); }
            set { SetValue(MapLayerProperty, value); }
        }

        /// <summary>
        /// Gets or sets the location of the center point of the map.
        /// </summary>
        public Location Center
        {
            get { return (Location)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        /// <summary>
        /// Gets or sets the target value of a Center animation.
        /// </summary>
        public Location TargetCenter
        {
            get { return (Location)GetValue(TargetCenterProperty); }
            set { SetValue(TargetCenterProperty, value); }
        }

        /// <summary>
        /// Gets or sets the minimum value of the ZoomLevel and TargetZommLevel properties.
        /// Must be greater than or equal to zero and less than or equal to MaxZoomLevel.
        /// The default value is 1.
        /// </summary>
        public double MinZoomLevel
        {
            get { return (double)GetValue(MinZoomLevelProperty); }
            set { SetValue(MinZoomLevelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the maximum value of the ZoomLevel and TargetZommLevel properties.
        /// Must be greater than or equal to MinZoomLevel and less than or equal to 20.
        /// The default value is 19.
        /// </summary>
        public double MaxZoomLevel
        {
            get { return (double)GetValue(MaxZoomLevelProperty); }
            set { SetValue(MaxZoomLevelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the map zoom level.
        /// </summary>
        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the target value of a ZoomLevel animation.
        /// </summary>
        public double TargetZoomLevel
        {
            get { return (double)GetValue(TargetZoomLevelProperty); }
            set { SetValue(TargetZoomLevelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the map heading, i.e. a clockwise rotation angle in degrees.
        /// </summary>
        public double Heading
        {
            get { return (double)GetValue(HeadingProperty); }
            set { SetValue(HeadingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the target value of a Heading animation.
        /// </summary>
        public double TargetHeading
        {
            get { return (double)GetValue(TargetHeadingProperty); }
            set { SetValue(TargetHeadingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Duration of the Center, ZoomLevel and Heading animations.
        /// The default value is 0.3 seconds.
        /// </summary>
        public TimeSpan AnimationDuration
        {
            get { return (TimeSpan)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        /// <summary>
        /// Gets or sets the EasingFunction of the Center, ZoomLevel and Heading animations.
        /// The default value is a QuadraticEase with EasingMode.EaseOut.
        /// </summary>
        public EasingFunctionBase AnimationEasingFunction
        {
            get { return (EasingFunctionBase)GetValue(AnimationEasingFunctionProperty); }
            set { SetValue(AnimationEasingFunctionProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Duration of the Tile Opacity animation.
        /// The default value is 0.2 seconds.
        /// </summary>
        public TimeSpan TileFadeDuration
        {
            get { return (TimeSpan)GetValue(TileFadeDurationProperty); }
            set { SetValue(TileFadeDurationProperty, value); }
        }

        /// <summary>
        /// Gets the scaling transformation from meters to viewport coordinate units at the Center location.
        /// </summary>
        public ScaleTransform ScaleTransform { get; } = new ScaleTransform();

        /// <summary>
        /// Gets the transformation that rotates by the value of the Heading property.
        /// </summary>
        public RotateTransform RotateTransform { get; } = new RotateTransform();

        /// <summary>
        /// Gets the combination of ScaleTransform and RotateTransform
        /// </summary>
        public TransformGroup ScaleRotateTransform { get; } = new TransformGroup();

        /// <summary>
        /// Transforms a Location in geographic coordinates to a Point in viewport coordinates.
        /// </summary>
        public Point LocationToViewportPoint(Location location)
        {
            return MapProjection.LocationToViewportPoint(location);
        }

        /// <summary>
        /// Transforms a Point in viewport coordinates to a Location in geographic coordinates.
        /// </summary>
        public Location ViewportPointToLocation(Point point)
        {
            return MapProjection.ViewportPointToLocation(point);
        }

        /// <summary>
        /// Sets a temporary center point in viewport coordinates for scaling and rotation transformations.
        /// This center point is automatically reset when the Center property is set by application code.
        /// </summary>
        public void SetTransformCenter(Point center)
        {
            transformCenter = MapProjection.ViewportPointToLocation(center);
            viewportCenter = center;
        }

        /// <summary>
        /// Resets the temporary transform center point set by SetTransformCenter.
        /// </summary>
        public void ResetTransformCenter()
        {
            transformCenter = null;
            viewportCenter = new Point(RenderSize.Width / 2d, RenderSize.Height / 2d);
        }

        /// <summary>
        /// Changes the Center property according to the specified map translation in viewport coordinates.
        /// </summary>
        public void TranslateMap(Point translation)
        {
            if (transformCenter != null)
            {
                ResetTransformCenter();
                UpdateTransform();
            }

            if (translation.X != 0d || translation.Y != 0d)
            {
                if (Heading != 0d)
                {
                    var cos = Math.Cos(Heading / 180d * Math.PI);
                    var sin = Math.Sin(Heading / 180d * Math.PI);

                    translation = new Point(
                        translation.X * cos + translation.Y * sin,
                        translation.Y * cos - translation.X * sin);
                }

                translation.X = -translation.X;
                translation.Y = -translation.Y;

                Center = MapProjection.TranslateLocation(Center, translation);
            }
        }

        /// <summary>
        /// Changes the Center, Heading and ZoomLevel properties according to the specified
        /// viewport coordinate translation, rotation and scale delta values. Rotation and scaling
        /// is performed relative to the specified center point in viewport coordinates.
        /// </summary>
        public void TransformMap(Point center, Point translation, double rotation, double scale)
        {
            if (rotation != 0d || scale != 1d)
            {
                transformCenter = MapProjection.ViewportPointToLocation(center);
                viewportCenter = new Point(center.X + translation.X, center.Y + translation.Y);

                if (rotation != 0d)
                {
                    var heading = (((Heading + rotation) % 360d) + 360d) % 360d;
                    InternalSetValue(HeadingProperty, heading);
                    InternalSetValue(TargetHeadingProperty, heading);
                }

                if (scale != 1d)
                {
                    var zoomLevel = Math.Min(Math.Max(ZoomLevel + Math.Log(scale, 2d), MinZoomLevel), MaxZoomLevel);
                    InternalSetValue(ZoomLevelProperty, zoomLevel);
                    InternalSetValue(TargetZoomLevelProperty, zoomLevel);
                }

                UpdateTransform(true);
            }
            else
            {
                TranslateMap(translation); // more precise
            }
        }

        /// <summary>
        /// Sets the value of the TargetZoomLevel property while retaining the specified center point
        /// in viewport coordinates.
        /// </summary>
        public void ZoomMap(Point center, double zoomLevel)
        {
            zoomLevel = Math.Min(Math.Max(zoomLevel, MinZoomLevel), MaxZoomLevel);

            if (TargetZoomLevel != zoomLevel)
            {
                SetTransformCenter(center);

                if (double.IsNaN(MapProjection.LongitudeScale))
                {
                    ZoomLevel = zoomLevel;
                }
                else
                {
                    TargetZoomLevel = zoomLevel;
                }
            }
        }

        /// <summary>
        /// Sets the TargetZoomLevel and TargetCenter properties so that the specified bounding box
        /// fits into the current viewport. The TargetHeading property is set to zero.
        /// </summary>
        public void ZoomToBounds(BoundingBox boundingBox)
        {
            if (boundingBox != null && boundingBox.HasValidBounds)
            {
                var rect = MapProjection.BoundingBoxToRect(boundingBox);
                var center = new Point(rect.X + rect.Width / 2d, rect.Y + rect.Height / 2d);
                var scale0 = 1d / MapProjection.GetViewportScale(0d);
                var lonScale = scale0 * RenderSize.Width / rect.Width;
                var latScale = scale0 * RenderSize.Height / rect.Height;
                var lonZoom = Math.Log(lonScale, 2d);
                var latZoom = Math.Log(latScale, 2d);

                TargetZoomLevel = Math.Min(lonZoom, latZoom);
                TargetCenter = MapProjection.PointToLocation(center);
                TargetHeading = 0d;
            }
        }

        private void InternalSetValue(DependencyProperty property, object value)
        {
            internalPropertyChange = true;
            SetValue(property, value);
            internalPropertyChange = false;
        }

        private void ProjectionPropertyChanged()
        {
            ResetTransformCenter();
            UpdateTransform(false, true);
        }

        private void MapLayerPropertyChanged(UIElement oldLayer, UIElement newLayer)
        {
            if (oldLayer != null)
            {
                Children.Remove(oldLayer);

                var mapLayer = oldLayer as IMapLayer;
                if (mapLayer != null)
                {
                    if (mapLayer.MapBackground != null)
                    {
                        ClearValue(BackgroundProperty);
                    }
                    if (mapLayer.MapForeground != null)
                    {
                        ClearValue(ForegroundProperty);
                    }
                }
            }

            if (newLayer != null)
            {
                Children.Insert(0, newLayer);

                var mapLayer = newLayer as IMapLayer;
                if (mapLayer != null)
                {
                    if (mapLayer.MapBackground != null)
                    {
                        Background = mapLayer.MapBackground;
                    }
                    if (mapLayer.MapForeground != null)
                    {
                        Foreground = mapLayer.MapForeground;
                    }
                }
            }
        }

        private void AdjustCenterProperty(DependencyProperty property, ref Location center)
        {
            if (center == null)
            {
                center = new Location();
                InternalSetValue(property, center);
            }
            else if (center.Longitude < -180d || center.Longitude > 180d ||
                center.Latitude < -MapProjection.MaxLatitude || center.Latitude > MapProjection.MaxLatitude)
            {
                center = new Location(
                    Math.Min(Math.Max(center.Latitude, -MapProjection.MaxLatitude), MapProjection.MaxLatitude),
                    Location.NormalizeLongitude(center.Longitude));
                InternalSetValue(property, center);
            }
        }

        private void CenterPropertyChanged(Location center)
        {
            if (!internalPropertyChange)
            {
                AdjustCenterProperty(CenterProperty, ref center);
                UpdateTransform();

                if (centerAnimation == null)
                {
                    InternalSetValue(TargetCenterProperty, center);
                    InternalSetValue(CenterPointProperty, MapProjection.LocationToPoint(center));
                }
            }
        }

        private void TargetCenterPropertyChanged(Location targetCenter)
        {
            if (!internalPropertyChange)
            {
                AdjustCenterProperty(TargetCenterProperty, ref targetCenter);

                if (!targetCenter.Equals(Center))
                {
                    if (centerAnimation != null)
                    {
                        centerAnimation.Completed -= CenterAnimationCompleted;
                    }

                    // animate private CenterPoint property by PointAnimation
                    centerAnimation = new PointAnimation
                    {
                        From = MapProjection.LocationToPoint(Center),
                        To = MapProjection.LocationToPoint(new Location(
                            targetCenter.Latitude,
                            Location.NearestLongitude(targetCenter.Longitude, Center.Longitude))),
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEasingFunction
                    };

                    centerAnimation.Completed += CenterAnimationCompleted;
                    this.BeginAnimation(CenterPointProperty, centerAnimation);
                }
            }
        }

        private void CenterAnimationCompleted(object sender, object e)
        {
            if (centerAnimation != null)
            {
                centerAnimation.Completed -= CenterAnimationCompleted;
                centerAnimation = null;

                InternalSetValue(CenterProperty, TargetCenter);
                InternalSetValue(CenterPointProperty, MapProjection.LocationToPoint(TargetCenter));
                RemoveAnimation(CenterPointProperty); // remove holding animation in WPF
                UpdateTransform();
            }
        }

        private void CenterPointPropertyChanged(Point centerPoint)
        {
            if (!internalPropertyChange)
            {
                var center = MapProjection.PointToLocation(centerPoint);
                center.Longitude = Location.NormalizeLongitude(center.Longitude);

                InternalSetValue(CenterProperty, center);
                UpdateTransform();
            }
        }

        private void MinZoomLevelPropertyChanged(double minZoomLevel)
        {
            if (ZoomLevel < minZoomLevel)
            {
                ZoomLevel = minZoomLevel;
            }
            CoerceValue(MaxZoomLevelProperty);
        }
        private object MinZoomLevelCoerceValue(double minZoomLevel)
        {
            return Math.Min(Math.Max(minZoomLevel, 0d), MaxZoomLevel);
        }

        private void MaxZoomLevelPropertyChanged(double maxZoomLevel)
        {
            if (ZoomLevel > maxZoomLevel)
            {
                ZoomLevel = maxZoomLevel;
            }
            CoerceValue(MinZoomLevelProperty);
        }
        private object MaxZoomLevelCoerceValue(double maxZoomLevel)
        {
            return Math.Min(Math.Max(maxZoomLevel, MinZoomLevel), MaximumZoomLevel);
        }

        private void AdjustZoomLevelProperty(DependencyProperty property, ref double zoomLevel)
        {
            if (zoomLevel < MinZoomLevel || zoomLevel > MaxZoomLevel)
            {
                zoomLevel = Math.Min(Math.Max(zoomLevel, MinZoomLevel), MaxZoomLevel);
                InternalSetValue(property, zoomLevel);
            }
        }

        private void ZoomLevelPropertyChanged(double zoomLevel)
        {
            if (!internalPropertyChange)
            {
                AdjustZoomLevelProperty(ZoomLevelProperty, ref zoomLevel);
                UpdateTransform();

                if (zoomLevelAnimation == null)
                {
                    InternalSetValue(TargetZoomLevelProperty, zoomLevel);
                }
            }
        }

        private void TargetZoomLevelPropertyChanged(double targetZoomLevel)
        {
            if (!internalPropertyChange)
            {
                AdjustZoomLevelProperty(TargetZoomLevelProperty, ref targetZoomLevel);

                if (targetZoomLevel != ZoomLevel)
                {
                    if (zoomLevelAnimation != null)
                    {
                        zoomLevelAnimation.Completed -= ZoomLevelAnimationCompleted;
                    }

                    zoomLevelAnimation = new DoubleAnimation
                    {
                        To = targetZoomLevel,
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEasingFunction
                    };

                    zoomLevelAnimation.Completed += ZoomLevelAnimationCompleted;
                    this.BeginAnimation(ZoomLevelProperty, zoomLevelAnimation);
                }
            }
        }

        private void ZoomLevelAnimationCompleted(object sender, object e)
        {
            if (zoomLevelAnimation != null)
            {
                zoomLevelAnimation.Completed -= ZoomLevelAnimationCompleted;
                zoomLevelAnimation = null;

                InternalSetValue(ZoomLevelProperty, TargetZoomLevel);
                RemoveAnimation(ZoomLevelProperty); // remove holding animation in WPF

                UpdateTransform(true);
            }
        }

        private void AdjustHeadingProperty(DependencyProperty property, ref double heading)
        {
            if (heading < 0d || heading > 360d)
            {
                heading = ((heading % 360d) + 360d) % 360d;
                InternalSetValue(property, heading);
            }
        }

        private void HeadingPropertyChanged(double heading)
        {
            if (!internalPropertyChange)
            {
                AdjustHeadingProperty(HeadingProperty, ref heading);
                UpdateTransform();

                if (headingAnimation == null)
                {
                    InternalSetValue(TargetHeadingProperty, heading);
                }
            }
        }

        private void TargetHeadingPropertyChanged(double targetHeading)
        {
            if (!internalPropertyChange)
            {
                AdjustHeadingProperty(TargetHeadingProperty, ref targetHeading);

                if (targetHeading != Heading)
                {
                    var delta = targetHeading - Heading;

                    if (delta > 180d)
                    {
                        delta -= 360d;
                    }
                    else if (delta < -180d)
                    {
                        delta += 360d;
                    }

                    if (headingAnimation != null)
                    {
                        headingAnimation.Completed -= HeadingAnimationCompleted;
                    }

                    headingAnimation = new DoubleAnimation
                    {
                        By = delta,
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEasingFunction
                    };

                    headingAnimation.Completed += HeadingAnimationCompleted;
                    this.BeginAnimation(HeadingProperty, headingAnimation);
                }
            }
        }

        private void HeadingAnimationCompleted(object sender, object e)
        {
            if (headingAnimation != null)
            {
                headingAnimation.Completed -= HeadingAnimationCompleted;
                headingAnimation = null;

                InternalSetValue(HeadingProperty, TargetHeading);
                RemoveAnimation(HeadingProperty); // remove holding animation in WPF
                UpdateTransform();
            }
        }

        private void UpdateTransform(bool resetCenter = false, bool projectionChanged = false)
        {
            var projection = MapProjection;
            var center = transformCenter ?? Center;

            projection.SetViewportTransform(center, viewportCenter, ZoomLevel, Heading);

            if (transformCenter != null)
            {
                center = projection.ViewportPointToLocation(new Point(RenderSize.Width / 2d, RenderSize.Height / 2d));
                center.Longitude = Location.NormalizeLongitude(center.Longitude);

                if (center.Latitude < -projection.MaxLatitude || center.Latitude > projection.MaxLatitude)
                {
                    center.Latitude = Math.Min(Math.Max(center.Latitude, -projection.MaxLatitude), projection.MaxLatitude);
                    resetCenter = true;
                }

                InternalSetValue(CenterProperty, center);

                if (centerAnimation == null)
                {
                    InternalSetValue(TargetCenterProperty, center);
                    InternalSetValue(CenterPointProperty, projection.LocationToPoint(center));
                }

                if (resetCenter)
                {
                    ResetTransformCenter();
                    projection.SetViewportTransform(center, viewportCenter, ZoomLevel, Heading);
                }
            }

            var scale = projection.GetMapScale(center);
            ScaleTransform.ScaleX = scale.X;
            ScaleTransform.ScaleY = scale.Y;
            RotateTransform.Angle = Heading;

            OnViewportChanged(new ViewportChangedEventArgs(projectionChanged, Center.Longitude - centerLongitude));

            centerLongitude = Center.Longitude;
        }

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            base.OnViewportChanged(e);

            ViewportChanged?.Invoke(this, e);
        }
    }
}
