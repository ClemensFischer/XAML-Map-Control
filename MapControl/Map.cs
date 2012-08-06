// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MapControl
{
    /// <summary>
    /// The main map control. Draws map content provided by the TileLayers or the MainTileLayer property.
    /// The visible map area is defined by the Center and ZoomLevel properties. The map can be rotated
    /// by an angle that is given by the Heading property.
    /// Map is a MapPanel and hence can contain map overlays like other MapPanels or MapItemsControls.
    /// </summary>
    public partial class Map : MapPanel
    {
        public const double MeterPerDegree = 1852d * 60d;

        public static readonly DependencyProperty FontSizeProperty = Control.FontSizeProperty.AddOwner(typeof(Map));
        public static readonly DependencyProperty FontFamilyProperty = Control.FontFamilyProperty.AddOwner(typeof(Map));
        public static readonly DependencyProperty FontStyleProperty = Control.FontStyleProperty.AddOwner(typeof(Map));
        public static readonly DependencyProperty FontWeightProperty = Control.FontWeightProperty.AddOwner(typeof(Map));
        public static readonly DependencyProperty FontStretchProperty = Control.FontStretchProperty.AddOwner(typeof(Map));
        public static readonly DependencyProperty ForegroundProperty = Control.ForegroundProperty.AddOwner(typeof(Map));

        public static readonly DependencyProperty LightForegroundProperty = DependencyProperty.Register(
            "LightForeground", typeof(Brush), typeof(Map));

        public static readonly DependencyProperty DarkForegroundProperty = DependencyProperty.Register(
            "DarkForeground", typeof(Brush), typeof(Map));

        public static readonly DependencyProperty LightBackgroundProperty = DependencyProperty.Register(
            "LightBackground", typeof(Brush), typeof(Map));

        public static readonly DependencyProperty DarkBackgroundProperty = DependencyProperty.Register(
            "DarkBackground", typeof(Brush), typeof(Map));

        public static readonly DependencyProperty TileLayersProperty = DependencyProperty.Register(
            "TileLayers", typeof(TileLayerCollection), typeof(Map), new FrameworkPropertyMetadata(
                (o, e) => ((Map)o).TileLayersPropertyChanged((TileLayerCollection)e.NewValue),
                (o, v) => ((Map)o).CoerceTileLayersProperty((TileLayerCollection)v)));

        public static readonly DependencyProperty MainTileLayerProperty = DependencyProperty.Register(
            "MainTileLayer", typeof(TileLayer), typeof(Map), new FrameworkPropertyMetadata(
                (o, e) => ((Map)o).MainTileLayerPropertyChanged((TileLayer)e.NewValue),
                (o, v) => ((Map)o).CoerceMainTileLayerProperty((TileLayer)v)));

        public static readonly DependencyProperty TileOpacityProperty = DependencyProperty.Register(
            "TileOpacity", typeof(double), typeof(Map), new FrameworkPropertyMetadata(1d,
                (o, e) => ((Map)o).tileContainer.Opacity = (double)e.NewValue));

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof(Location), typeof(Map), new FrameworkPropertyMetadata(new Location(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((Map)o).CenterPropertyChanged((Location)e.NewValue),
                (o, v) => ((Map)o).CoerceCenterProperty((Location)v)));

        public static readonly DependencyProperty TargetCenterProperty = DependencyProperty.Register(
            "TargetCenter", typeof(Location), typeof(Map), new FrameworkPropertyMetadata(new Location(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((Map)o).TargetCenterPropertyChanged((Location)e.NewValue),
                (o, v) => ((Map)o).CoerceCenterProperty((Location)v)));

        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
            "ZoomLevel", typeof(double), typeof(Map), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((Map)o).ZoomLevelPropertyChanged((double)e.NewValue),
                (o, v) => ((Map)o).CoerceZoomLevelProperty((double)v)));

        public static readonly DependencyProperty TargetZoomLevelProperty = DependencyProperty.Register(
            "TargetZoomLevel", typeof(double), typeof(Map), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((Map)o).TargetZoomLevelPropertyChanged((double)e.NewValue),
                (o, v) => ((Map)o).CoerceZoomLevelProperty((double)v)));

        public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
            "Heading", typeof(double), typeof(Map), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((Map)o).HeadingPropertyChanged((double)e.NewValue),
                (o, v) => ((Map)o).CoerceHeadingProperty((double)v)));

        public static readonly DependencyProperty TargetHeadingProperty = DependencyProperty.Register(
            "TargetHeading", typeof(double), typeof(Map), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((Map)o).TargetHeadingPropertyChanged((double)e.NewValue),
                (o, v) => ((Map)o).CoerceHeadingProperty((double)v)));

        private static readonly DependencyPropertyKey CenterScalePropertyKey = DependencyProperty.RegisterReadOnly(
            "CenterScale", typeof(double), typeof(Map), null);

        public static readonly DependencyProperty CenterScaleProperty = CenterScalePropertyKey.DependencyProperty;

        private readonly TileContainer tileContainer = new TileContainer();
        private readonly MapTransform mapTransform = new MercatorTransform();
        private readonly ScaleTransform scaleTransform = new ScaleTransform();
        private readonly RotateTransform rotateTransform = new RotateTransform();
        private readonly MatrixTransform scaleRotateTransform = new MatrixTransform();
        private Location transformOrigin;
        private Point viewportOrigin;
        private LocationAnimation centerAnimation;
        private DoubleAnimation zoomLevelAnimation;
        private DoubleAnimation headingAnimation;
        private bool updateTransform = true;

        public Map()
        {
            MinZoomLevel = 1;
            MaxZoomLevel = 20;
            AddVisualChild(tileContainer);
            TileLayers = new TileLayerCollection();
            SetValue(ParentMapProperty, this);

            Loaded += (o, e) =>
            {
                if (MainTileLayer == null)
                {
                    MainTileLayer = new TileLayer
                    {
                        Description = "© {y} OpenStreetMap Contributors, CC-BY-SA",
                        TileSource = new TileSource("http://{c}.tile.openstreetmap.org/{z}/{x}/{y}.png")
                    };
                }
            };
        }

        /// <summary>
        /// Raised when the ViewTransform property has changed.
        /// </summary>
        public event EventHandler ViewTransformChanged;

        /// <summary>
        /// Raised when the TileLayers property has changed.
        /// </summary>
        public event EventHandler TileLayersChanged;

        public double MinZoomLevel { get; set; }
        public double MaxZoomLevel { get; set; }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

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
        /// Gets or sets the main TileLayer used by this Map, i.e. TileLayers[0].
        /// </summary>
        public TileLayer MainTileLayer
        {
            get { return (TileLayer)GetValue(MainTileLayerProperty); }
            set { SetValue(MainTileLayerProperty, value); }
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
        /// Gets or sets the map rotation angle in degrees.
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
            private set { SetValue(CenterScalePropertyKey, value); }
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
        /// Transforms a geographic location to a viewport coordinates point.
        /// </summary>
        public Point LocationToViewportPoint(Location location)
        {
            return ViewportTransform.Transform(MapTransform.Transform(location));
        }

        /// <summary>
        /// Transforms a viewport coordinates point to a geographic location.
        /// </summary>
        public Location ViewportPointToLocation(Point point)
        {
            return MapTransform.TransformBack(ViewportTransform.Inverse.Transform(point));
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
            transformOrigin = CoerceCenterProperty(ViewportPointToLocation(viewportOrigin));
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
        public void TranslateMap(Vector translation)
        {
            if (translation.X != 0d || translation.Y != 0d)
            {
                if (transformOrigin != null)
                {
                    viewportOrigin += translation;
                    UpdateViewTransform();
                }
                else
                {
                    Center = ViewportPointToLocation(viewportOrigin - translation);
                }
            }
        }

        /// <summary>
        /// Changes the Center, Heading and ZoomLevel properties according to the specified
        /// viewport coordinate translation, rotation and scale delta values. Rotation and scaling
        /// is performed relative to the specified origin point in viewport coordinates.
        /// </summary>
        public void TransformMap(Point origin, Vector translation, double rotation, double scale)
        {
            if (rotation != 0d || scale != 1d)
            {
                SetTransformOrigin(origin);
                updateTransform = false;
                Heading = (((Heading + rotation) % 360d) + 360d) % 360d;
                ZoomLevel += Math.Log(scale, 2d);
                updateTransform = true;
                UpdateViewTransform();
            }

            TranslateMap(translation);
        }

        /// <summary>
        /// Sets the value of the ZoomLevel property while retaining the specified origin point
        /// in viewport coordinates.
        /// </summary>
        public void ZoomMap(Point origin, double zoomLevel)
        {
            SetTransformOrigin(origin);
            TargetZoomLevel = zoomLevel;
        }

        /// <summary>
        /// Gets the map scale at the specified location as viewport coordinate units (pixels) per meter.
        /// </summary>
        public double GetMapScale(Location location)
        {
            return mapTransform.RelativeScale(location) * Math.Pow(2d, ZoomLevel) * 256d / (MeterPerDegree * 360d);
        }

        protected override int VisualChildrenCount
        {
            get { return InternalChildren.Count + 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0)
            {
                return tileContainer;
            }

            return InternalChildren[index - 1];
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            ResetTransformOrigin();
            UpdateViewTransform();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Background, null, new Rect(RenderSize));
        }

        protected override void OnViewTransformChanged(Map map)
        {
            base.OnViewTransformChanged(map);

            if (ViewTransformChanged != null)
            {
                ViewTransformChanged(this, EventArgs.Empty);
            }
        }

        protected internal virtual void OnTileLayersChanged()
        {
            if (tileContainer.TileLayers != null &&
                tileContainer.TileLayers.Count > 0 &&
                tileContainer.TileLayers[0].HasDarkBackground)
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

            if (TileLayersChanged != null)
            {
                TileLayersChanged(this, EventArgs.Empty);
            }
        }

        private void TileLayersPropertyChanged(TileLayerCollection tileLayers)
        {
            if (tileLayers != null)
            {
                tileContainer.TileLayers = tileLayers;
                MainTileLayer = tileLayers.Count > 0 ? tileLayers[0] : null;
            }
        }

        private TileLayerCollection CoerceTileLayersProperty(TileLayerCollection tileLayers)
        {
            if (tileLayers == null)
            {
                tileLayers = new TileLayerCollection();
            }

            return tileLayers;
        }

        private void MainTileLayerPropertyChanged(TileLayer mainTileLayer)
        {
            if (mainTileLayer != null)
            {
                if (tileContainer.TileLayers.Count == 0)
                {
                    tileContainer.TileLayers.Add(mainTileLayer);
                }
                else if (tileContainer.TileLayers[0] != mainTileLayer)
                {
                    tileContainer.TileLayers[0] = mainTileLayer;
                }
            }
        }

        private TileLayer CoerceMainTileLayerProperty(TileLayer mainTileLayer)
        {
            if (mainTileLayer == null && tileContainer.TileLayers.Count > 0)
            {
                mainTileLayer = tileContainer.TileLayers[0];
            }

            return mainTileLayer;
        }

        private void CenterPropertyChanged(Location center)
        {
            if (updateTransform)
            {
                ResetTransformOrigin();
                UpdateViewTransform();
            }

            if (centerAnimation == null)
            {
                TargetCenter = center;
            }
        }

        private void TargetCenterPropertyChanged(Location targetCenter)
        {
            if (targetCenter != Center)
            {
                if (centerAnimation != null)
                {
                    centerAnimation.Completed -= CenterAnimationCompleted;
                }

                centerAnimation = new LocationAnimation
                {
                    From = Center,
                    To = targetCenter,
                    Duration = TimeSpan.FromSeconds(0.5),
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                centerAnimation.Completed += CenterAnimationCompleted;
                BeginAnimation(CenterProperty, centerAnimation);
            }
        }

        private void CenterAnimationCompleted(object sender, EventArgs eventArgs)
        {
            Center = TargetCenter;
            centerAnimation.Completed -= CenterAnimationCompleted;
            centerAnimation = null;
        }

        private Location CoerceCenterProperty(Location location)
        {
            location.Latitude = Math.Min(Math.Max(location.Latitude, -MapTransform.MaxLatitude), MapTransform.MaxLatitude);
            location.Longitude = Location.NormalizeLongitude(location.Longitude);
            return location;
        }

        private void ZoomLevelPropertyChanged(double zoomLevel)
        {
            if (updateTransform)
            {
                UpdateViewTransform();
            }

            if (zoomLevelAnimation == null)
            {
                TargetZoomLevel = zoomLevel;
            }
        }

        private void TargetZoomLevelPropertyChanged(double targetZoomLevel)
        {
            if (targetZoomLevel != ZoomLevel)
            {
                if (zoomLevelAnimation != null)
                {
                    zoomLevelAnimation.Completed -= ZoomLevelAnimationCompleted;
                }

                zoomLevelAnimation = new DoubleAnimation
                {
                    From = ZoomLevel,
                    To = targetZoomLevel,
                    Duration = TimeSpan.FromSeconds(0.5),
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                zoomLevelAnimation.Completed += ZoomLevelAnimationCompleted;
                BeginAnimation(ZoomLevelProperty, zoomLevelAnimation);
            }
        }

        private void ZoomLevelAnimationCompleted(object sender, EventArgs eventArgs)
        {
            ZoomLevel = TargetZoomLevel;
            zoomLevelAnimation.Completed -= ZoomLevelAnimationCompleted;
            zoomLevelAnimation = null;
            ResetTransformOrigin();
        }

        private double CoerceZoomLevelProperty(double zoomLevel)
        {
            return Math.Min(Math.Max(zoomLevel, MinZoomLevel), MaxZoomLevel);
        }

        private void HeadingPropertyChanged(double heading)
        {
            if (updateTransform)
            {
                UpdateViewTransform();
            }

            if (headingAnimation == null)
            {
                TargetHeading = heading;
            }
        }

        private void TargetHeadingPropertyChanged(double targetHeading)
        {
            if (targetHeading != Heading)
            {
                if (headingAnimation != null)
                {
                    headingAnimation.Completed -= HeadingAnimationCompleted;
                }

                double delta = targetHeading - Heading;

                if (delta > 180d)
                {
                    delta -= 360d;
                }
                else if (delta < -180d)
                {
                    delta += 360d;
                }

                headingAnimation = new DoubleAnimation
                {
                    From = Heading,
                    By = delta,
                    Duration = TimeSpan.FromSeconds(0.5),
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                headingAnimation.Completed += HeadingAnimationCompleted;
                BeginAnimation(HeadingProperty, headingAnimation);
            }
        }

        private void HeadingAnimationCompleted(object sender, EventArgs eventArgs)
        {
            Heading = TargetHeading;
            headingAnimation.Completed -= HeadingAnimationCompleted;
            headingAnimation = null;
        }

        private double CoerceHeadingProperty(double heading)
        {
            return ((heading % 360d) + 360d) % 360d;
        }

        private void UpdateViewTransform()
        {
            double scale;

            if (transformOrigin != null)
            {
                scale = tileContainer.SetTransform(ZoomLevel, Heading, MapTransform.Transform(transformOrigin), viewportOrigin, RenderSize);
                updateTransform = false;
                Center = ViewportPointToLocation(new Point(RenderSize.Width / 2d, RenderSize.Height / 2d));
                updateTransform = true;
            }
            else
            {
                scale = tileContainer.SetTransform(ZoomLevel, Heading, MapTransform.Transform(Center), viewportOrigin, RenderSize);
            }

            scale *= MapTransform.RelativeScale(Center) / MeterPerDegree; // Pixels per meter at center latitude

            CenterScale = scale;
            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;
            rotateTransform.Angle = Heading;
            scaleRotateTransform.Matrix = scaleTransform.Value * rotateTransform.Value;

            OnViewTransformChanged(this);
        }
    }
}
