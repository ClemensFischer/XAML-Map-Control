using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MapControl
{
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
                (o, e) => ((Map)o).SetTileLayers((TileLayerCollection)e.NewValue)));

        public static readonly DependencyProperty BaseTileLayerProperty = DependencyProperty.Register(
            "BaseTileLayer", typeof(TileLayer), typeof(Map), new FrameworkPropertyMetadata(
                (o, e) => ((Map)o).SetBaseTileLayer((TileLayer)e.NewValue),
                (o, v) => ((Map)o).CoerceBaseTileLayer((TileLayer)v)));

        public static readonly DependencyProperty TileOpacityProperty = DependencyProperty.Register(
            "TileOpacity", typeof(double), typeof(Map), new FrameworkPropertyMetadata(1d,
                (o, e) => ((Map)o).tileContainer.Opacity = (double)e.NewValue));

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof(Point), typeof(Map), new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((Map)o).SetCenter((Point)e.NewValue),
                (o, v) => ((Map)o).CoerceCenter((Point)v)));

        public static readonly DependencyProperty TargetCenterProperty = DependencyProperty.Register(
            "TargetCenter", typeof(Point), typeof(Map), new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((Map)o).SetTargetCenter((Point)e.NewValue),
                (o, v) => ((Map)o).CoerceCenter((Point)v)));

        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
            "ZoomLevel", typeof(double), typeof(Map), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((Map)o).SetZoomLevel((double)e.NewValue),
                (o, v) => ((Map)o).CoerceZoomLevel((double)v)));

        public static readonly DependencyProperty TargetZoomLevelProperty = DependencyProperty.Register(
            "TargetZoomLevel", typeof(double), typeof(Map), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((Map)o).SetTargetZoomLevel((double)e.NewValue),
                (o, v) => ((Map)o).CoerceZoomLevel((double)v)));

        public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
            "Heading", typeof(double), typeof(Map), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((Map)o).SetHeading((double)e.NewValue),
                (o, v) => ((Map)o).CoerceHeading((double)v)));

        public static readonly DependencyProperty TargetHeadingProperty = DependencyProperty.Register(
            "TargetHeading", typeof(double), typeof(Map), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((Map)o).SetTargetHeading((double)e.NewValue),
                (o, v) => ((Map)o).CoerceHeading((double)v)));

        private static readonly DependencyPropertyKey CenterScalePropertyKey = DependencyProperty.RegisterReadOnly(
            "CenterScale", typeof(double), typeof(Map), null);

        public static readonly DependencyProperty CenterScaleProperty = CenterScalePropertyKey.DependencyProperty;

        private readonly TileContainer tileContainer = new TileContainer();
        private readonly MapViewTransform mapViewTransform = new MapViewTransform();
        private readonly ScaleTransform scaleTransform = new ScaleTransform();
        private readonly RotateTransform rotateTransform = new RotateTransform();
        private readonly MatrixTransform scaleRotateTransform = new MatrixTransform();
        private Point? transformOrigin;
        private Point viewOrigin;
        private PointAnimation centerAnimation;
        private DoubleAnimation zoomLevelAnimation;
        private DoubleAnimation headingAnimation;
        private bool updateTransform = true;

        public Map()
        {
            MinZoomLevel = 1;
            MaxZoomLevel = 20;

            AddVisualChild(tileContainer);
            mapViewTransform.ViewTransform = tileContainer.ViewTransform;

            BaseTileLayer = new TileLayer
            {
                Description = "© {y} OpenStreetMap Contributors, CC-BY-SA",
                TileSource = new OpenStreetMapTileSource { UriFormat = "http://{c}.tile.openstreetmap.org/{z}/{x}/{y}.png" }
            };

            SetValue(ParentMapProperty, this);
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
        /// Gets or sets the base TileLayer used by this Map, i.e. TileLayers[0].
        /// </summary>
        public TileLayer BaseTileLayer
        {
            get { return (TileLayer)GetValue(BaseTileLayerProperty); }
            set { SetValue(BaseTileLayerProperty, value); }
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
        /// Gets or sets the world coordinates (latitude and longitude) of the center point.
        /// </summary>
        public Point Center
        {
            get { return (Point)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        /// <summary>
        /// Gets or sets the target value of a Center animation.
        /// </summary>
        public Point TargetCenter
        {
            get { return (Point)GetValue(TargetCenterProperty); }
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
        /// Gets the map scale at the Center point as view coordinate units (pixels) per meter.
        /// </summary>
        public double CenterScale
        {
            get { return (double)GetValue(CenterScaleProperty); }
            private set { SetValue(CenterScalePropertyKey, value); }
        }

        /// <summary>
        /// Gets the transformation from world coordinates (latitude and longitude)
        /// to cartesian map coordinates.
        /// </summary>
        public MapTransform MapTransform
        {
            get { return mapViewTransform.MapTransform; }
        }

        /// <summary>
        /// Gets the transformation from cartesian map coordinates to view coordinates.
        /// </summary>
        public Transform ViewTransform
        {
            get { return mapViewTransform.ViewTransform; }
        }

        /// <summary>
        /// Gets the combination of MapTransform and ViewTransform, i.e. the transformation
        /// from longitude and latitude values to view coordinates.
        /// </summary>
        public GeneralTransform MapViewTransform
        {
            get { return mapViewTransform; }
        }

        /// <summary>
        /// Gets the scaling transformation from meters to view coordinate units (pixels)
        /// at the view center point.
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
        /// Sets an intermittent origin point in world coordinates for scaling and rotation transformations.
        /// This origin point is automatically removed when the Center property is set by application code.
        /// </summary>
        public void SetTransformOrigin(Point origin)
        {
            transformOrigin = origin;
            viewOrigin = MapViewTransform.Transform(origin);
        }

        /// <summary>
        /// Sets an intermittent origin point in view coordinates for scaling and rotation transformations.
        /// This origin point is automatically removed when the Center property is set by application code.
        /// </summary>
        public void SetTransformViewOrigin(Point origin)
        {
            viewOrigin.X = Math.Min(Math.Max(origin.X, 0d), RenderSize.Width);
            viewOrigin.Y = Math.Min(Math.Max(origin.Y, 0d), RenderSize.Height);
            transformOrigin = CoerceCenter(MapViewTransform.Inverse.Transform(viewOrigin));
        }

        /// <summary>
        /// Removes the intermittent transform origin point set by SetTransformOrigin.
        /// </summary>
        public void ResetTransformOrigin()
        {
            transformOrigin = null;
            viewOrigin = new Point(RenderSize.Width / 2d, RenderSize.Height / 2d);
        }

        /// <summary>
        /// Changes the Center property according to the specified translation in view coordinates.
        /// </summary>
        public void TranslateMap(Vector translation)
        {
            if (translation.X != 0d || translation.Y != 0d)
            {
                ResetTransformOrigin();
                Center = MapViewTransform.Inverse.Transform(viewOrigin - translation);
            }
        }

        /// <summary>
        /// Changes the Center, Heading and ZoomLevel properties according to the specified
        /// view coordinate translation, rotation and scale delta values. Rotation and scaling
        /// is performed relative to the specified origin point in view coordinates.
        /// </summary>
        public void TransformMap(Point origin, Vector translation, double rotation, double scale)
        {
            if (rotation != 0d || scale != 1d)
            {
                SetTransformViewOrigin(origin);
                updateTransform = false;
                Heading += rotation;
                ZoomLevel += Math.Log(scale, 2d);
                updateTransform = true;
                SetViewTransform();
            }

            TranslateMap(translation);
        }

        /// <summary>
        /// Sets the value of the ZoomLevel property while retaining the specified origin point
        /// in view coordinates.
        /// </summary>
        public void ZoomMap(Point origin, double zoomLevel)
        {
            SetTransformViewOrigin(origin);
            TargetZoomLevel = zoomLevel;
        }

        /// <summary>
        /// Gets the map scale at the specified world coordinate point
        /// as view coordinate units (pixels) per meter.
        /// </summary>
        public double GetMapScale(Point point)
        {
            return MapTransform.RelativeScale(point) * Math.Pow(2d, ZoomLevel) * 256d / (MeterPerDegree * 360d);
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
            SetViewTransform();
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

        private void SetTileLayers(TileLayerCollection tileLayers)
        {
            BaseTileLayer = tileLayers.Count > 0 ? tileLayers[0] : null;
            tileContainer.TileLayers = tileLayers;
        }

        private void SetBaseTileLayer(TileLayer baseTileLayer)
        {
            if (baseTileLayer != null)
            {
                if (TileLayers == null)
                {
                    TileLayers = new TileLayerCollection(baseTileLayer);
                }
                else if (TileLayers.Count == 0)
                {
                    TileLayers.Add(baseTileLayer);
                }
                else if (TileLayers[0] != baseTileLayer)
                {
                    TileLayers[0] = baseTileLayer;
                }
            }
        }

        private TileLayer CoerceBaseTileLayer(TileLayer baseTileLayer)
        {
            if (baseTileLayer == null && TileLayers.Count > 0)
            {
                baseTileLayer = TileLayers[0];
            }

            return baseTileLayer;
        }

        private void SetCenter(Point center)
        {
            if (updateTransform)
            {
                ResetTransformOrigin();
                SetViewTransform();
            }

            if (centerAnimation == null)
            {
                TargetCenter = center;
            }
        }

        private void SetTargetCenter(Point targetCenter)
        {
            if (targetCenter != Center)
            {
                if (centerAnimation != null)
                {
                    centerAnimation.Completed -= CenterAnimationCompleted;
                }

                centerAnimation = new PointAnimation
                {
                    From = Center,
                    To = targetCenter,
                    Duration = TimeSpan.FromSeconds(0.5),
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                centerAnimation.Completed += CenterAnimationCompleted;

                updateTransform = false;
                Center = targetCenter;
                updateTransform = true;

                BeginAnimation(CenterProperty, centerAnimation);
            }
        }

        private void CenterAnimationCompleted(object sender, EventArgs eventArgs)
        {
            centerAnimation = null;
        }

        private Point CoerceCenter(Point point)
        {
            point.X = ((point.X + 180d) % 360d + 360d) % 360d - 180d;
            point.Y = Math.Min(Math.Max(point.Y, -MapTransform.MaxLatitude), MapTransform.MaxLatitude);
            return point;
        }

        private void SetZoomLevel(double zoomLevel)
        {
            if (updateTransform)
            {
                SetViewTransform();
            }

            if (zoomLevelAnimation == null)
            {
                TargetZoomLevel = zoomLevel;
            }
        }

        private void SetTargetZoomLevel(double targetZoomLevel)
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

                updateTransform = false;
                ZoomLevel = targetZoomLevel;
                updateTransform = true;

                BeginAnimation(ZoomLevelProperty, zoomLevelAnimation);
            }
        }

        private void ZoomLevelAnimationCompleted(object sender, EventArgs eventArgs)
        {
            zoomLevelAnimation = null;
            ResetTransformOrigin();
        }

        private double CoerceZoomLevel(double zoomLevel)
        {
            return Math.Min(Math.Max(zoomLevel, MinZoomLevel), MaxZoomLevel);
        }

        private void SetHeading(double heading)
        {
            if (updateTransform)
            {
                SetViewTransform();
            }

            if (headingAnimation == null)
            {
                TargetHeading = heading;
            }
        }

        private void SetTargetHeading(double targetHeading)
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

                updateTransform = false;
                Heading = targetHeading;
                updateTransform = true;

                BeginAnimation(HeadingProperty, headingAnimation);
            }
        }

        private void HeadingAnimationCompleted(object sender, EventArgs eventArgs)
        {
            headingAnimation = null;
        }

        private double CoerceHeading(double heading)
        {
            return ((heading % 360d) + 360d) % 360d;
        }

        private void SetViewTransform()
        {
            double scale;

            if (transformOrigin.HasValue)
            {
                scale = tileContainer.SetTransform(ZoomLevel, Heading, MapTransform.Transform(transformOrigin.Value), viewOrigin, RenderSize);
                updateTransform = false;
                Center = MapViewTransform.Inverse.Transform(new Point(RenderSize.Width / 2d, RenderSize.Height / 2d));
                updateTransform = true;
            }
            else
            {
                scale = tileContainer.SetTransform(ZoomLevel, Heading, MapTransform.Transform(Center), viewOrigin, RenderSize);
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
