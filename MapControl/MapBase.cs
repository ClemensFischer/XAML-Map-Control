// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Specialized;
using System.Linq;
#if WINRT
using Windows.Foundation;
using Windows.UI;
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
        public const double MeterPerDegree = 1852d * 60d;
        public static TimeSpan AnimationDuration = TimeSpan.FromSeconds(0.5);
        public static EasingFunctionBase AnimationEasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };

        public static readonly DependencyProperty LightForegroundProperty = DependencyProperty.Register(
            "LightForeground", typeof(Brush), typeof(MapBase), null);

        public static readonly DependencyProperty DarkForegroundProperty = DependencyProperty.Register(
            "DarkForeground", typeof(Brush), typeof(MapBase), null);

        public static readonly DependencyProperty LightBackgroundProperty = DependencyProperty.Register(
            "LightBackground", typeof(Brush), typeof(MapBase), new PropertyMetadata(new SolidColorBrush(Colors.Transparent), null));

        public static readonly DependencyProperty DarkBackgroundProperty = DependencyProperty.Register(
            "DarkBackground", typeof(Brush), typeof(MapBase), new PropertyMetadata(new SolidColorBrush(Colors.Transparent), null));

        public static readonly DependencyProperty TileLayersProperty = DependencyProperty.Register(
            "TileLayers", typeof(TileLayerCollection), typeof(MapBase), new PropertyMetadata(null,
                (o, e) => ((MapBase)o).TileLayersPropertyChanged((TileLayerCollection)e.OldValue, (TileLayerCollection)e.NewValue)));

        public static readonly DependencyProperty TileLayerProperty = DependencyProperty.Register(
            "TileLayer", typeof(TileLayer), typeof(MapBase), new PropertyMetadata(null,
                (o, e) => ((MapBase)o).TileLayerPropertyChanged((TileLayer)e.NewValue)));

        public static readonly DependencyProperty TileOpacityProperty = DependencyProperty.Register(
            "TileOpacity", typeof(double), typeof(MapBase), new PropertyMetadata(1d,
                (o, e) => ((MapBase)o).tileContainer.Opacity = (double)e.NewValue));

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof(Location), typeof(MapBase), new PropertyMetadata(new Location(),
                (o, e) => ((MapBase)o).CenterPropertyChanged((Location)e.NewValue)));

        public static readonly DependencyProperty TargetCenterProperty = DependencyProperty.Register(
            "TargetCenter", typeof(Location), typeof(MapBase), new PropertyMetadata(new Location(),
                (o, e) => ((MapBase)o).TargetCenterPropertyChanged((Location)e.NewValue)));

        internal static readonly DependencyProperty CenterPointProperty = DependencyProperty.Register(
            "CenterPoint", typeof(Point), typeof(MapBase), new PropertyMetadata(new Point(),
                (o, e) => ((MapBase)o).CenterPointPropertyChanged((Point)e.NewValue)));

        public static readonly DependencyProperty MinZoomLevelProperty = DependencyProperty.Register(
            "MinZoomLevel", typeof(double), typeof(MapBase), new PropertyMetadata(1d,
                (o, e) => ((MapBase)o).MinZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty MaxZoomLevelProperty = DependencyProperty.Register(
            "MaxZoomLevel", typeof(double), typeof(MapBase), new PropertyMetadata(18d,
                (o, e) => ((MapBase)o).MaxZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
            "ZoomLevel", typeof(double), typeof(MapBase), new PropertyMetadata(1d,
                (o, e) => ((MapBase)o).ZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty TargetZoomLevelProperty = DependencyProperty.Register(
            "TargetZoomLevel", typeof(double), typeof(MapBase), new PropertyMetadata(1d,
                (o, e) => ((MapBase)o).TargetZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
            "Heading", typeof(double), typeof(MapBase), new PropertyMetadata(0d,
                (o, e) => ((MapBase)o).HeadingPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty TargetHeadingProperty = DependencyProperty.Register(
            "TargetHeading", typeof(double), typeof(MapBase), new PropertyMetadata(0d,
                (o, e) => ((MapBase)o).TargetHeadingPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty CenterScaleProperty = DependencyProperty.Register(
            "CenterScale", typeof(double), typeof(MapBase), null);

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
        private bool internalPropertyChange;

        public MapBase()
        {
            SetValue(MapPanel.ParentMapProperty, this);

            Background = LightBackground;
            TileLayers = new TileLayerCollection();
            Initialize();

            Loaded += OnLoaded;
        }

        partial void Initialize();

        /// <summary>
        /// Raised when the current viewport has changed.
        /// </summary>
        public event EventHandler ViewportChanged;

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public Brush LightForeground
        {
            get { return (Brush)GetValue(LightForegroundProperty); }
            set { SetValue(LightForegroundProperty, value); }
        }

        public Brush DarkForeground
        {
            get { return (Brush)GetValue(DarkForegroundProperty); }
            set { SetValue(DarkForegroundProperty, value); }
        }

        public Brush LightBackground
        {
            get { return (Brush)GetValue(LightBackgroundProperty); }
            set { SetValue(LightBackgroundProperty, value); }
        }

        public Brush DarkBackground
        {
            get { return (Brush)GetValue(DarkBackgroundProperty); }
            set { SetValue(DarkBackgroundProperty, value); }
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
        /// Gets the map scale at the Center location as viewport coordinate units (pixels) per meter.
        /// </summary>
        public double CenterScale
        {
            get { return (double)GetValue(CenterScaleProperty); }
            private set { SetValue(CenterScaleProperty, value); }
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
        /// Gets the scaling transformation from meters to viewport coordinate units (pixels)
        /// at the viewport center point.
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
        /// Gets the map scale at the specified location as viewport coordinate units (pixels) per meter.
        /// </summary>
        public double GetMapScale(Location location)
        {
            return mapTransform.RelativeScale(location) * Math.Pow(2d, ZoomLevel) * 256d / (MeterPerDegree * 360d);
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
            transformOrigin = CoerceLocation(ViewportPointToLocation(viewportOrigin));
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
            if (translation.X != 0d || translation.Y != 0d)
            {
                if (transformOrigin != null)
                {
                    viewportOrigin.X += translation.X;
                    viewportOrigin.Y += translation.Y;
                    UpdateTransform();
                }
                else
                {
                    Center = ViewportPointToLocation(new Point(viewportOrigin.X - translation.X, viewportOrigin.Y - translation.Y));
                }
            }
        }

        /// <summary>
        /// Changes the Center, Heading and ZoomLevel properties according to the specified
        /// viewport coordinate translation, rotation and scale delta values. Rotation and scaling
        /// is performed relative to the specified origin point in viewport coordinates.
        /// </summary>
        public void TransformMap(Point origin, Point translation, double rotation, double scale)
        {
            if (rotation != 0d || scale != 1d)
            {
                SetTransformOrigin(origin);
                SetProperty(HeadingProperty, CoerceHeading(Heading + rotation));
                SetProperty(ZoomLevelProperty, CoerceZoomLevel(ZoomLevel + Math.Log(scale, 2d)));
                UpdateTransform();
            }

            ResetTransformOrigin();
            TranslateMap(translation);
        }

        /// <summary>
        /// Sets the value of the ZoomLevel property while retaining the specified origin point
        /// in viewport coordinates.
        /// </summary>
        public void ZoomMap(Point origin, double zoomLevel)
        {
            var targetZoomLebel = TargetZoomLevel;
            TargetZoomLevel = zoomLevel;

            if (TargetZoomLevel != targetZoomLebel) // TargetZoomLevel might be coerced
            {
                SetTransformOrigin(origin);
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

            UpdateTileLayer();
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
            }

            UpdateTileLayer();
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

            if (tileLayer != null && tileLayer.HasDarkBackground)
            {
                if (DarkForeground != null)
                {
                    Foreground = DarkForeground;
                }

                if (DarkBackground != null)
                {
                    Background = DarkBackground;
                }
            }
            else
            {
                if (LightForeground != null)
                {
                    Foreground = LightForeground;
                }

                if (LightBackground != null)
                {
                    Background = LightBackground;
                }
            }
        }

        private void UpdateTileLayer()
        {
            var tileLayer = TileLayers.FirstOrDefault();

            if (TileLayer != tileLayer)
            {
                TileLayer = tileLayer;
            }
        }

        private void SetProperty(DependencyProperty property, object value)
        {
            internalPropertyChange = true;
            SetValue(property, value);
            internalPropertyChange = false;
        }

        private Location CoerceLocation(Location location)
        {
            return new Location(
                Math.Min(Math.Max(location.Latitude, -mapTransform.MaxLatitude), mapTransform.MaxLatitude),
                Location.NormalizeLongitude(location.Longitude));
        }

        private bool CoerceCenterProperty(DependencyProperty property, ref Location value)
        {
            Location coercedValue = CoerceLocation(value);

            if (!coercedValue.Equals(value))
            {
                SetProperty(property, coercedValue);
            }

            return coercedValue != value;
        }

        private void CenterPropertyChanged(Location center)
        {
            if (!internalPropertyChange)
            {
                CoerceCenterProperty(CenterProperty, ref center);
                ResetTransformOrigin();
                UpdateTransform();

                if (centerAnimation == null)
                {
                    SetProperty(TargetCenterProperty, center);
                    SetProperty(CenterPointProperty, new Point(center.Longitude, center.Latitude));
                }
            }
        }

        private void TargetCenterPropertyChanged(Location targetCenter)
        {
            if (!internalPropertyChange)
            {
                CoerceCenterProperty(TargetCenterProperty, ref targetCenter);

                if (targetCenter != Center)
                {
                    if (centerAnimation != null)
                    {
                        centerAnimation.Completed -= CenterAnimationCompleted;
                    }

                    // animate private CenterPoint property by PointAnimation
                    centerAnimation = new PointAnimation
                    {
                        From = new Point(Center.Longitude, Center.Latitude),
                        To = new Point(targetCenter.Longitude, targetCenter.Latitude),
                        Duration = AnimationDuration,
                        FillBehavior = AnimationFillBehavior,
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

                SetProperty(CenterProperty, TargetCenter);
                SetProperty(CenterPointProperty, new Point(TargetCenter.Longitude, TargetCenter.Latitude));
                ResetTransformOrigin();
                UpdateTransform();
            }
        }

        private void CenterPointPropertyChanged(Point centerPoint)
        {
            if (!internalPropertyChange)
            {
                SetProperty(CenterProperty, new Location(centerPoint.Y, centerPoint.X));
                ResetTransformOrigin();
                UpdateTransform();
            }
        }

        private void MinZoomLevelPropertyChanged(double minZoomLevel)
        {
            var coercedValue = Math.Min(Math.Max(minZoomLevel, 0d), MaxZoomLevel);

            if (coercedValue != minZoomLevel)
            {
                SetProperty(MinZoomLevelProperty, coercedValue);
            }
            else if (ZoomLevel < minZoomLevel)
            {
                ZoomLevel = minZoomLevel;
            }
        }

        private void MaxZoomLevelPropertyChanged(double maxZoomLevel)
        {
            var coercedValue = Math.Min(Math.Max(maxZoomLevel, MinZoomLevel), 20d);

            if (coercedValue != maxZoomLevel)
            {
                SetProperty(MaxZoomLevelProperty, coercedValue);
            }
            else if (ZoomLevel > maxZoomLevel)
            {
                ZoomLevel = maxZoomLevel;
            }
        }

        private double CoerceZoomLevel(double value)
        {
            return Math.Min(Math.Max(value, MinZoomLevel), MaxZoomLevel);
        }

        private bool CoerceZoomLevelProperty(DependencyProperty property, ref double value)
        {
            var coercedValue = CoerceZoomLevel(value);

            if (coercedValue != value)
            {
                SetProperty(property, coercedValue);
            }

            return coercedValue != value;
        }

        private void ZoomLevelPropertyChanged(double zoomLevel)
        {
            if (!internalPropertyChange &&
                !CoerceZoomLevelProperty(ZoomLevelProperty, ref zoomLevel))
            {
                UpdateTransform();

                if (zoomLevelAnimation == null)
                {
                    SetProperty(TargetZoomLevelProperty, zoomLevel);
                }
            }
        }

        private void TargetZoomLevelPropertyChanged(double targetZoomLevel)
        {
            if (!internalPropertyChange &&
                !CoerceZoomLevelProperty(TargetZoomLevelProperty, ref targetZoomLevel) &&
                targetZoomLevel != ZoomLevel)
            {
                if (zoomLevelAnimation != null)
                {
                    zoomLevelAnimation.Completed -= ZoomLevelAnimationCompleted;
                }

                zoomLevelAnimation = new DoubleAnimation
                {
                    To = targetZoomLevel,
                    Duration = AnimationDuration,
                    FillBehavior = AnimationFillBehavior,
                    EasingFunction = AnimationEasingFunction
                };

                zoomLevelAnimation.Completed += ZoomLevelAnimationCompleted;
                this.BeginAnimation(ZoomLevelProperty, zoomLevelAnimation);
            }
        }

        private void ZoomLevelAnimationCompleted(object sender, object e)
        {
            if (zoomLevelAnimation != null)
            {
                zoomLevelAnimation.Completed -= ZoomLevelAnimationCompleted;
                zoomLevelAnimation = null;

                SetProperty(ZoomLevelProperty, TargetZoomLevel);
                UpdateTransform();
                ResetTransformOrigin();
            }
        }

        private double CoerceHeading(double value)
        {
            return (value >= -180d && value <= 360d) ? value : ((value + 360d) % 360d);
        }

        private bool CoerceHeadingProperty(DependencyProperty property, ref double value)
        {
            var coercedValue = CoerceHeading(value);

            if (coercedValue != value)
            {
                SetProperty(property, coercedValue);
            }

            return coercedValue != value;
        }

        private void HeadingPropertyChanged(double heading)
        {
            if (!internalPropertyChange)
            {
                CoerceHeadingProperty(HeadingProperty, ref heading);
                UpdateTransform();

                if (headingAnimation == null)
                {
                    SetProperty(TargetHeadingProperty, heading);
                }
            }
        }

        private void TargetHeadingPropertyChanged(double targetHeading)
        {
            if (!internalPropertyChange)
            {
                CoerceHeadingProperty(TargetHeadingProperty, ref targetHeading);

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
                        FillBehavior = AnimationFillBehavior,
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

                SetProperty(HeadingProperty, TargetHeading);
                UpdateTransform();
            }
        }

        private void UpdateTransform()
        {
            double scale;

            if (transformOrigin != null)
            {
                scale = tileContainer.SetViewportTransform(ZoomLevel, Heading, mapTransform.Transform(transformOrigin), viewportOrigin, RenderSize);
                SetProperty(CenterProperty, ViewportPointToLocation(new Point(RenderSize.Width / 2d, RenderSize.Height / 2d)));
            }
            else
            {
                scale = tileContainer.SetViewportTransform(ZoomLevel, Heading, mapTransform.Transform(Center), viewportOrigin, RenderSize);
            }

            scale *= mapTransform.RelativeScale(Center) / MeterPerDegree; // Pixels per meter at center latitude
            CenterScale = scale;
            SetTransformMatrixes(scale);

            OnViewportChanged();
        }
    }
}
