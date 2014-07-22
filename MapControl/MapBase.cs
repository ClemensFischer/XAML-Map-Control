// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Specialized;
using System.Linq;
#if WINDOWS_RUNTIME
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
    /// <summary>
    /// The map control. Draws map content provided by the TileLayers or the TileLayer property.
    /// The visible map area is defined by the Center and ZoomLevel properties. The map can be rotated
    /// by an angle that is given by the Heading property.
    /// MapBase is a MapPanel and hence can contain map overlays like other MapPanels or MapItemsControls.
    /// </summary>
    public partial class MapBase : MapPanel
    {
        private const double MaximumZoomLevel = 22d;

        public static TimeSpan AnimationDuration = TimeSpan.FromSeconds(0.5);
        public static EasingFunctionBase AnimationEasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };

        public static readonly DependencyProperty TileLayersProperty = DependencyProperty.Register(
            "TileLayers", typeof(TileLayerCollection), typeof(MapBase), new PropertyMetadata(null,
                (o, e) => ((MapBase)o).TileLayersPropertyChanged((TileLayerCollection)e.OldValue, (TileLayerCollection)e.NewValue)));

        public static readonly DependencyProperty TileLayerProperty = DependencyProperty.Register(
            "TileLayer", typeof(TileLayer), typeof(MapBase), new PropertyMetadata(null,
                (o, e) => ((MapBase)o).TileLayerPropertyChanged((TileLayer)e.NewValue)));

        public static readonly DependencyProperty TileOpacityProperty = DependencyProperty.Register(
            "TileOpacity", typeof(double), typeof(MapBase), new PropertyMetadata(1d,
                (o, e) => ((MapBase)o).tileContainer.Opacity = (double)e.NewValue));

        public static readonly DependencyProperty MinZoomLevelProperty = DependencyProperty.Register(
            "MinZoomLevel", typeof(double), typeof(MapBase), new PropertyMetadata(1d,
                (o, e) => ((MapBase)o).MinZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty MaxZoomLevelProperty = DependencyProperty.Register(
            "MaxZoomLevel", typeof(double), typeof(MapBase), new PropertyMetadata(18d,
                (o, e) => ((MapBase)o).MaxZoomLevelPropertyChanged((double)e.NewValue)));

        internal static readonly DependencyProperty CenterPointProperty = DependencyProperty.Register(
            "CenterPoint", typeof(Point), typeof(MapBase), new PropertyMetadata(new Point(),
                (o, e) => ((MapBase)o).CenterPointPropertyChanged((Point)e.NewValue)));

        private readonly TileContainer tileContainer = new TileContainer();
        private readonly MapTransform mapTransform = new MercatorTransform();
        private readonly MatrixTransform scaleTransform = new MatrixTransform();
        private readonly MatrixTransform rotateTransform = new MatrixTransform();
        private readonly MatrixTransform scaleRotateTransform = new MatrixTransform();
        private Location transformOrigin;
        private Point viewportOrigin;
        private PointAnimation centerAnimation;
        private DoubleAnimation zoomLevelAnimation;
        private DoubleAnimation headingAnimation;
        private Brush storedBackground;
        private Brush storedForeground;
        private bool internalPropertyChange;

        public MapBase()
        {
            SetParentMap();

            TileLayers = new TileLayerCollection();
            InternalChildren.Add(tileContainer);
            Initialize();

            Loaded += OnLoaded;
        }

        partial void Initialize();
        partial void RemoveAnimation(DependencyProperty property);

        /// <summary>
        /// Raised when the current viewport has changed.
        /// </summary>
        public event EventHandler ViewportChanged;

        /// <summary>
        /// Gets or sets the map foreground Brush.
        /// </summary>
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets the TileLayers used by this Map.
        /// </summary>
        public TileLayerCollection TileLayers
        {
            get { return (TileLayerCollection)GetValue(TileLayersProperty); }
            set { SetValue(TileLayersProperty, value); }
        }

        /// <summary>
        /// Gets or sets the base TileLayer used by this Map, i.e. TileLayers[0].
        /// </summary>
        public TileLayer TileLayer
        {
            get { return (TileLayer)GetValue(TileLayerProperty); }
            set { SetValue(TileLayerProperty, value); }
        }

        /// <summary>
        /// Gets or sets the opacity of the tile layers.
        /// </summary>
        public double TileOpacity
        {
            get { return (double)GetValue(TileOpacityProperty); }
            set { SetValue(TileOpacityProperty, value); }
        }

        /// <summary>
        /// Gets or sets the location of the center point of the Map.
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
        /// </summary>
        public double MinZoomLevel
        {
            get { return (double)GetValue(MinZoomLevelProperty); }
            set { SetValue(MinZoomLevelProperty, value); }
        }

        /// <summary>
        /// Gets or sets the maximum value of the ZoomLevel and TargetZommLevel properties.
        /// Must be greater than or equal to MinZoomLevel and less than or equal to 20.
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
        /// Gets or sets the map heading, or clockwise rotation angle in degrees.
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
        /// Gets the transformation from geographic coordinates to cartesian map coordinates.
        /// </summary>
        public MapTransform MapTransform
        {
            get { return mapTransform; }
        }

        /// <summary>
        /// Gets the transformation from cartesian map coordinates to viewport coordinates.
        /// </summary>
        public Transform ViewportTransform
        {
            get { return tileContainer.ViewportTransform; }
        }

        /// <summary>
        /// Gets the scaling transformation from meters to viewport coordinate units (pixels) at the Center location.
        /// </summary>
        public Transform ScaleTransform
        {
            get { return scaleTransform; }
        }

        /// <summary>
        /// Gets the transformation that rotates by the value of the Heading property.
        /// </summary>
        public Transform RotateTransform
        {
            get { return rotateTransform; }
        }

        /// <summary>
        /// Gets the combination of ScaleTransform and RotateTransform
        /// </summary>
        public Transform ScaleRotateTransform
        {
            get { return scaleRotateTransform; }
        }

        /// <summary>
        /// Gets the scaling factor from cartesian map coordinates to viewport coordinates.
        /// </summary>
        public double ViewportScale { get; private set; }

        /// <summary>
        /// Gets the scaling factor from meters to viewport coordinate units (pixels) at the Center location.
        /// </summary>
        public double CenterScale { get; private set; }

        /// <summary>
        /// Gets the map scale at the specified location as viewport coordinate units (pixels) per meter.
        /// </summary>
        public double GetMapScale(Location location)
        {
            return mapTransform.RelativeScale(location) * Math.Pow(2d, ZoomLevel) * TileSource.TileSize / (TileSource.MetersPerDegree * 360d);
        }

        /// <summary>
        /// Transforms a geographic location to a viewport coordinates point.
        /// </summary>
        public Point LocationToViewportPoint(Location location)
        {
            return ViewportTransform.Transform(mapTransform.Transform(location));
        }

        /// <summary>
        /// Transforms a viewport coordinates point to a geographic location.
        /// </summary>
        public Location ViewportPointToLocation(Point point)
        {
            return mapTransform.Transform(ViewportTransform.Inverse.Transform(point));
        }

        /// <summary>
        /// Sets a temporary origin location in geographic coordinates for scaling and rotation transformations.
        /// This origin location is automatically removed when the Center property is set by application code.
        /// </summary>
        public void SetTransformOrigin(Location origin)
        {
            transformOrigin = origin;
            viewportOrigin = LocationToViewportPoint(origin);
        }

        /// <summary>
        /// Sets a temporary origin point in viewport coordinates for scaling and rotation transformations.
        /// This origin point is automatically removed when the Center property is set by application code.
        /// </summary>
        public void SetTransformOrigin(Point origin)
        {
            viewportOrigin.X = Math.Min(Math.Max(origin.X, 0d), RenderSize.Width);
            viewportOrigin.Y = Math.Min(Math.Max(origin.Y, 0d), RenderSize.Height);
            transformOrigin = ViewportPointToLocation(viewportOrigin);
        }

        /// <summary>
        /// Removes the temporary transform origin point set by SetTransformOrigin.
        /// </summary>
        public void ResetTransformOrigin()
        {
            transformOrigin = null;
            viewportOrigin = new Point(RenderSize.Width / 2d, RenderSize.Height / 2d);
        }

        /// <summary>
        /// Changes the Center property according to the specified translation in viewport coordinates.
        /// </summary>
        public void TranslateMap(Point translation)
        {
            if (transformOrigin != null)
            {
                ResetTransformOrigin();
            }

            if (translation.X != 0d || translation.Y != 0d)
            {
                Center = ViewportPointToLocation(new Point(viewportOrigin.X - translation.X, viewportOrigin.Y - translation.Y));
            }
        }

        /// <summary>
        /// Changes the Center, Heading and ZoomLevel properties according to the specified
        /// viewport coordinate translation, rotation and scale delta values. Rotation and scaling
        /// is performed relative to the specified origin point in viewport coordinates.
        /// </summary>
        public void TransformMap(Point origin, Point translation, double rotation, double scale)
        {
            SetTransformOrigin(origin);

            viewportOrigin.X += translation.X;
            viewportOrigin.Y += translation.Y;

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

        /// <summary>
        /// Sets the value of the TargetZoomLevel property while retaining the specified origin point
        /// in viewport coordinates.
        /// </summary>
        public void ZoomMap(Point origin, double zoomLevel)
        {
            if (zoomLevel >= MinZoomLevel && zoomLevel <= MaxZoomLevel)
            {
                SetTransformOrigin(origin);
                TargetZoomLevel = zoomLevel;
            }
        }

        /// <summary>
        /// Sets the TargetZoomLevel and TargetCenter properties such that the specified bounding box
        /// fits into the current viewport. The TargetHeading property is set to zero.
        /// </summary>
        public void ZoomToBounds(Location southWest, Location northEast)
        {
            if (southWest.Latitude < northEast.Latitude && southWest.Longitude < northEast.Longitude)
            {
                var p1 = MapTransform.Transform(southWest);
                var p2 = MapTransform.Transform(northEast);
                var lonScale = RenderSize.Width / (p2.X - p1.X) * 360d / TileSource.TileSize;
                var latScale = RenderSize.Height / (p2.Y - p1.Y) * 360d / TileSource.TileSize;
                var lonZoom = Math.Log(lonScale, 2d);
                var latZoom = Math.Log(latScale, 2d);

                TargetZoomLevel = Math.Min(lonZoom, latZoom);
                TargetCenter = MapTransform.Transform(new Point((p1.X + p2.X) / 2d, (p1.Y + p2.Y) / 2d));
                TargetHeading = 0d;
            }
        }

        protected override void OnViewportChanged()
        {
            base.OnViewportChanged();

            if (ViewportChanged != null)
            {
                ViewportChanged(this, EventArgs.Empty);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            if (TileLayer == null)
            {
                TileLayer = TileLayer.Default;
            }

            UpdateTransform();
        }

        private void TileLayerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    tileContainer.AddTileLayers(e.NewStartingIndex, e.NewItems.Cast<TileLayer>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    tileContainer.RemoveTileLayers(e.OldStartingIndex, e.OldItems.Count);
                    break;
#if !SILVERLIGHT
                case NotifyCollectionChangedAction.Move:
#endif
                case NotifyCollectionChangedAction.Replace:
                    tileContainer.RemoveTileLayers(e.NewStartingIndex, e.OldItems.Count);
                    tileContainer.AddTileLayers(e.NewStartingIndex, e.NewItems.Cast<TileLayer>());
                    break;

                case NotifyCollectionChangedAction.Reset:
                    tileContainer.ClearTileLayers();
                    if (e.NewItems != null)
                    {
                        tileContainer.AddTileLayers(0, e.NewItems.Cast<TileLayer>());
                    }
                    break;

                default:
                    break;
            }

            var firstTileLayer = TileLayers.FirstOrDefault();

            if (TileLayer != firstTileLayer)
            {
                TileLayer = firstTileLayer;
            }
        }

        private void TileLayersPropertyChanged(TileLayerCollection oldTileLayers, TileLayerCollection newTileLayers)
        {
            tileContainer.ClearTileLayers();

            if (oldTileLayers != null)
            {
                oldTileLayers.CollectionChanged -= TileLayerCollectionChanged;
            }

            if (newTileLayers != null)
            {
                newTileLayers.CollectionChanged += TileLayerCollectionChanged;
                tileContainer.AddTileLayers(0, newTileLayers);

                var firstTileLayer = TileLayers.FirstOrDefault();

                if (TileLayer != firstTileLayer)
                {
                    TileLayer = firstTileLayer;
                }
            }
            else
            {
                TileLayer = null;
            }
        }

        private void TileLayerPropertyChanged(TileLayer tileLayer)
        {
            if (tileLayer != null)
            {
                if (TileLayers == null)
                {
                    TileLayers = new TileLayerCollection();
                }

                if (TileLayers.Count == 0)
                {
                    TileLayers.Add(tileLayer);
                }
                else if (TileLayers[0] != tileLayer)
                {
                    TileLayers[0] = tileLayer;
                }
            }

            if (tileLayer != null && tileLayer.Background != null)
            {
                if (storedBackground == null)
                {
                    storedBackground = Background;
                }

                Background = tileLayer.Background;
            }
            else if (storedBackground != null)
            {
                Background = storedBackground;
                storedBackground = null;
            }

            if (tileLayer != null && tileLayer.Foreground != null)
            {
                if (storedForeground == null)
                {
                    storedForeground = Foreground;
                }

                Foreground = tileLayer.Foreground;
            }
            else if (storedForeground != null)
            {
                Foreground = storedForeground;
                storedForeground = null;
            }
        }

        private void InternalSetValue(DependencyProperty property, object value)
        {
            internalPropertyChange = true;
            SetValue(property, value);
            internalPropertyChange = false;
        }

        private void AdjustCenterProperty(DependencyProperty property, ref Location center)
        {
            if (center == null)
            {
                center = new Location();
                InternalSetValue(property, center);
            }
            else if (center.Longitude < -180d || center.Longitude > 180d ||
                center.Latitude < -mapTransform.MaxLatitude || center.Latitude > mapTransform.MaxLatitude)
            {
                center = new Location(
                    Math.Min(Math.Max(center.Latitude, -mapTransform.MaxLatitude), mapTransform.MaxLatitude),
                    Location.NormalizeLongitude(center.Longitude));

                InternalSetValue(property, center);
            }
        }

        private void CenterPropertyChanged(Location center)
        {
            if (!internalPropertyChange)
            {
                AdjustCenterProperty(CenterProperty, ref center);
                ResetTransformOrigin();
                UpdateTransform();

                if (centerAnimation == null)
                {
                    InternalSetValue(TargetCenterProperty, center);
                    InternalSetValue(CenterPointProperty, MapTransform.Transform(center));
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
                        From = MapTransform.Transform(Center),
                        To = MapTransform.Transform(targetCenter, Center.Longitude),
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEasingFunction,
                        FillBehavior = FillBehavior.HoldEnd
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
                InternalSetValue(CenterPointProperty, MapTransform.Transform(TargetCenter));
                RemoveAnimation(CenterPointProperty); // remove holding animation in WPF

                ResetTransformOrigin();
                UpdateTransform();
            }
        }

        private void CenterPointPropertyChanged(Point centerPoint)
        {
            if (!internalPropertyChange)
            {
                centerPoint.X = Location.NormalizeLongitude(centerPoint.X);
                InternalSetValue(CenterProperty, MapTransform.Transform(centerPoint));
                ResetTransformOrigin();
                UpdateTransform();
            }
        }

        private void MinZoomLevelPropertyChanged(double minZoomLevel)
        {
            if (minZoomLevel < 0d || minZoomLevel > MaxZoomLevel)
            {
                minZoomLevel = Math.Min(Math.Max(minZoomLevel, 0d), MaxZoomLevel);
                InternalSetValue(MinZoomLevelProperty, minZoomLevel);
            }

            if (ZoomLevel < minZoomLevel)
            {
                ZoomLevel = minZoomLevel;
            }
        }

        private void MaxZoomLevelPropertyChanged(double maxZoomLevel)
        {
            if (maxZoomLevel < MinZoomLevel || maxZoomLevel > MaximumZoomLevel)
            {
                maxZoomLevel = Math.Min(Math.Max(maxZoomLevel, MinZoomLevel), MaximumZoomLevel);
                InternalSetValue(MaxZoomLevelProperty, maxZoomLevel);
            }

            if (ZoomLevel > maxZoomLevel)
            {
                ZoomLevel = maxZoomLevel;
            }
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
                        EasingFunction = AnimationEasingFunction,
                        FillBehavior = FillBehavior.HoldEnd
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
                        EasingFunction = AnimationEasingFunction,
                        FillBehavior = FillBehavior.HoldEnd
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

        private void UpdateTransform(bool resetTransformOrigin = false)
        {
            var center = Center;

            SetViewportTransform(transformOrigin ?? center);

            if (transformOrigin != null)
            {
                center = ViewportPointToLocation(new Point(RenderSize.Width / 2d, RenderSize.Height / 2d));
                center.Longitude = Location.NormalizeLongitude(center.Longitude);

                if (center.Latitude < -mapTransform.MaxLatitude || center.Latitude > mapTransform.MaxLatitude)
                {
                    center.Latitude = Math.Min(Math.Max(center.Latitude, -mapTransform.MaxLatitude), mapTransform.MaxLatitude);
                    resetTransformOrigin = true;
                }

                InternalSetValue(CenterProperty, center);

                if (centerAnimation == null)
                {
                    InternalSetValue(TargetCenterProperty, center);
                    InternalSetValue(CenterPointProperty, MapTransform.Transform(center));
                }

                if (resetTransformOrigin)
                {
                    ResetTransformOrigin();
                    SetViewportTransform(center);
                }
            }

            CenterScale = ViewportScale * mapTransform.RelativeScale(center) / TileSource.MetersPerDegree; // Pixels per meter at center latitude

            SetTransformMatrixes();
            OnViewportChanged();
        }

        private void SetViewportTransform(Location origin)
        {
            ViewportScale = tileContainer.SetViewportTransform(ZoomLevel, Heading, mapTransform.Transform(origin), viewportOrigin, RenderSize);
        }
    }
}
