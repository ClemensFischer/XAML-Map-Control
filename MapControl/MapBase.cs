// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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
    /// <summary>
    /// The map control. Displays map content provided by the TileLayer or TileLayers property.
    /// The visible map area is defined by the Center and ZoomLevel properties.
    /// The map can be rotated by an angle that is given by the Heading property.
    /// MapBase can contain map overlay child elements like other MapPanels or MapItemsControls.
    /// </summary>
    public partial class MapBase : MapPanel
    {
        private const double MaximumZoomLevel = 22d;

        public static readonly DependencyProperty TileLayerProperty = DependencyProperty.Register(
            "TileLayer", typeof(TileLayer), typeof(MapBase),
            new PropertyMetadata(null, (o, e) => ((MapBase)o).TileLayerPropertyChanged((TileLayer)e.NewValue)));

        public static readonly DependencyProperty TileLayersProperty = DependencyProperty.Register(
            "TileLayers", typeof(IList<TileLayer>), typeof(MapBase),
            new PropertyMetadata(null, (o, e) => ((MapBase)o).TileLayersPropertyChanged((IList<TileLayer>)e.OldValue, (IList<TileLayer>)e.NewValue)));

        public static readonly DependencyProperty MinZoomLevelProperty = DependencyProperty.Register(
            "MinZoomLevel", typeof(double), typeof(MapBase),
            new PropertyMetadata(1d, (o, e) => ((MapBase)o).MinZoomLevelPropertyChanged((double)e.NewValue)));

        public static readonly DependencyProperty MaxZoomLevelProperty = DependencyProperty.Register(
            "MaxZoomLevel", typeof(double), typeof(MapBase),
            new PropertyMetadata(19d, (o, e) => ((MapBase)o).MaxZoomLevelPropertyChanged((double)e.NewValue)));

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

        private readonly PanelBase tileLayerPanel = new PanelBase();
        private readonly MapTransform mapTransform = new MercatorTransform();
        private readonly MatrixTransform viewportTransform = new MatrixTransform();
        private readonly ScaleTransform scaleTransform = new ScaleTransform();
        private readonly RotateTransform rotateTransform = new RotateTransform();
        private readonly TransformGroup scaleRotateTransform = new TransformGroup();

        private Location transformOrigin;
        private PointAnimation centerAnimation;
        private DoubleAnimation zoomLevelAnimation;
        private DoubleAnimation headingAnimation;
        private bool internalPropertyChange;

        public MapBase()
        {
            Initialize();

            scaleRotateTransform.Children.Add(scaleTransform);
            scaleRotateTransform.Children.Add(rotateTransform);

            Children.Add(tileLayerPanel);
            TileLayers = new ObservableCollection<TileLayer>();
        }

        partial void Initialize(); // Windows Runtime and Silverlight only
        partial void RemoveAnimation(DependencyProperty property); // WPF only

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
        /// Gets or sets the base TileLayer used by the Map control.
        /// </summary>
        public TileLayer TileLayer
        {
            get { return (TileLayer)GetValue(TileLayerProperty); }
            set { SetValue(TileLayerProperty, value); }
        }

        /// <summary>
        /// Gets or sets optional multiple TileLayers that are used simultaneously.
        /// The first element in the collection is equal to the value of the TileLayer
        /// property. The additional TileLayers usually have transparent backgrounds.
        /// </summary>
        public IList<TileLayer> TileLayers
        {
            get { return (IList<TileLayer>)GetValue(TileLayersProperty); }
            set { SetValue(TileLayersProperty, value); }
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
        /// Gets the transformation from geographic coordinates to cartesian map coordinates.
        /// </summary>
        public MapTransform MapTransform
        {
            get { return mapTransform; }
        }

        /// <summary>
        /// Gets the transformation from cartesian map coordinates to viewport coordinates (i.e. pixels).
        /// </summary>
        public MatrixTransform ViewportTransform
        {
            get { return viewportTransform; }
        }

        /// <summary>
        /// Gets the scaling transformation from meters to viewport coordinate units at the Center location.
        /// </summary>
        public ScaleTransform ScaleTransform
        {
            get { return scaleTransform; }
        }

        /// <summary>
        /// Gets the transformation that rotates by the value of the Heading property.
        /// </summary>
        public RotateTransform RotateTransform
        {
            get { return rotateTransform; }
        }

        /// <summary>
        /// Gets the combination of ScaleTransform and RotateTransform
        /// </summary>
        public TransformGroup ScaleRotateTransform
        {
            get { return scaleRotateTransform; }
        }

        internal Point MapOrigin { get; private set; }
        internal Point ViewportOrigin { get; private set; }

        /// <summary>
        /// Gets the scaling factor from cartesian map coordinates to viewport coordinates.
        /// </summary>
        public double ViewportScale { get; private set; }

        /// <summary>
        /// Gets the scaling factor from meters to viewport coordinate units at the Center location.
        /// </summary>
        public double CenterScale { get; private set; }

        /// <summary>
        /// Gets the map scale at the specified location as viewport coordinate units per meter.
        /// </summary>
        public double GetMapScale(Location location)
        {
            return mapTransform.RelativeScale(location) *
                Math.Pow(2d, ZoomLevel) * (double)TileSource.TileSize / (TileSource.MetersPerDegree * 360d);
        }

        /// <summary>
        /// Transforms a geographic location to a viewport coordinates point.
        /// </summary>
        public Point LocationToViewportPoint(Location location)
        {
            return viewportTransform.Transform(mapTransform.Transform(location));
        }

        /// <summary>
        /// Transforms a viewport coordinates point to a geographic location.
        /// </summary>
        public Location ViewportPointToLocation(Point point)
        {
            return mapTransform.Transform(viewportTransform.Inverse.Transform(point));
        }

        /// <summary>
        /// Sets a temporary origin location in geographic coordinates for scaling and rotation transformations.
        /// This origin location is automatically reset when the Center property is set by application code.
        /// </summary>
        public void SetTransformOrigin(Location origin)
        {
            transformOrigin = origin;
            ViewportOrigin = LocationToViewportPoint(origin);
        }

        /// <summary>
        /// Sets a temporary origin point in viewport coordinates for scaling and rotation transformations.
        /// This origin point is automatically reset when the Center property is set by application code.
        /// </summary>
        public void SetTransformOrigin(Point origin)
        {
            transformOrigin = ViewportPointToLocation(origin);
            ViewportOrigin = origin;
        }

        /// <summary>
        /// Resets the temporary transform origin point set by SetTransformOrigin.
        /// </summary>
        public void ResetTransformOrigin()
        {
            transformOrigin = null;
            ViewportOrigin = new Point(RenderSize.Width / 2d, RenderSize.Height / 2d);
        }

        /// <summary>
        /// Changes the Center property according to the specified map translation in viewport coordinates.
        /// </summary>
        public void TranslateMap(Point translation)
        {
            if (transformOrigin != null)
            {
                ResetTransformOrigin();
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

                translation.X /= -ViewportScale;
                translation.Y /= ViewportScale;

                Center = mapTransform.Transform(Center, MapOrigin, translation);
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
                transformOrigin = ViewportPointToLocation(origin);
                ViewportOrigin = new Point(origin.X + translation.X, origin.Y + translation.Y);

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
        /// Sets the value of the TargetZoomLevel property while retaining the specified origin point
        /// in viewport coordinates.
        /// </summary>
        public void ZoomMap(Point origin, double zoomLevel)
        {
            zoomLevel = Math.Min(Math.Max(zoomLevel, MinZoomLevel), MaxZoomLevel);

            if (TargetZoomLevel != zoomLevel)
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
                var p1 = mapTransform.Transform(southWest);
                var p2 = mapTransform.Transform(northEast);
                var lonScale = RenderSize.Width / (p2.X - p1.X) * 360d / TileSource.TileSize;
                var latScale = RenderSize.Height / (p2.Y - p1.Y) * 360d / TileSource.TileSize;
                var lonZoom = Math.Log(lonScale, 2d);
                var latZoom = Math.Log(latScale, 2d);

                TargetZoomLevel = Math.Min(lonZoom, latZoom);
                TargetCenter = mapTransform.Transform(new Point((p1.X + p2.X) / 2d, (p1.Y + p2.Y) / 2d));
                TargetHeading = 0d;
            }
        }

        private void TileLayerPropertyChanged(TileLayer tileLayer)
        {
            if (tileLayer != null)
            {
                if (TileLayers == null)
                {
                    TileLayers = new ObservableCollection<TileLayer>(new TileLayer[] { tileLayer });
                }
                else if (TileLayers.Count == 0)
                {
                    TileLayers.Add(tileLayer);
                }
                else if (TileLayers[0] != tileLayer)
                {
                    TileLayers[0] = tileLayer;
                }
            }
        }

        private void TileLayersPropertyChanged(IList<TileLayer> oldTileLayers, IList<TileLayer> newTileLayers)
        {
            if (oldTileLayers != null)
            {
                var oldCollection = oldTileLayers as INotifyCollectionChanged;
                if (oldCollection != null)
                {
                    oldCollection.CollectionChanged -= TileLayerCollectionChanged;
                }

                SetTileLayer(null);
                ClearTileLayers();
            }

            if (newTileLayers != null)
            {
                SetTileLayer(newTileLayers.FirstOrDefault());
                AddTileLayers(0, newTileLayers);

                var newCollection = newTileLayers as INotifyCollectionChanged;
                if (newCollection != null)
                {
                    newCollection.CollectionChanged += TileLayerCollectionChanged;
                }
            }
        }

        private void TileLayerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddTileLayers(e.NewStartingIndex, e.NewItems.Cast<TileLayer>());
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveTileLayers(e.OldStartingIndex, e.OldItems.Count);
                    break;
#if !SILVERLIGHT
                case NotifyCollectionChangedAction.Move:
#endif
                case NotifyCollectionChangedAction.Replace:
                    RemoveTileLayers(e.NewStartingIndex, e.OldItems.Count);
                    AddTileLayers(e.NewStartingIndex, e.NewItems.Cast<TileLayer>());
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ClearTileLayers();
                    if (e.NewItems != null)
                    {
                        AddTileLayers(0, e.NewItems.Cast<TileLayer>());
                    }
                    break;

                default:
                    break;
            }

            var tileLayer = TileLayers.FirstOrDefault();

            if (TileLayer != tileLayer)
            {
                SetTileLayer(tileLayer);
            }
        }

        private void AddTileLayers(int index, IEnumerable<TileLayer> tileLayers)
        {
            foreach (var tileLayer in tileLayers)
            {
                if (index == 0)
                {
                    if (tileLayer.Background != null)
                    {
                        Background = tileLayer.Background;
                    }

                    if (tileLayer.Foreground != null)
                    {
                        Foreground = tileLayer.Foreground;
                    }
                }

                tileLayerPanel.Children.Insert(index++, tileLayer);
            }
        }

        private void RemoveTileLayers(int index, int count)
        {
            while (count-- > 0)
            {
                tileLayerPanel.Children.RemoveAt(index + count);
            }

            if (index == 0)
            {
                ClearValue(BackgroundProperty);
                ClearValue(ForegroundProperty);
            }
        }

        private void ClearTileLayers()
        {
            tileLayerPanel.Children.Clear();
            ClearValue(BackgroundProperty);
            ClearValue(ForegroundProperty);
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
                UpdateTransform();

                if (centerAnimation == null)
                {
                    InternalSetValue(TargetCenterProperty, center);
                    InternalSetValue(CenterPointProperty, mapTransform.Transform(center));
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
                        From = mapTransform.Transform(Center),
                        To = mapTransform.Transform(new Location(
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
                InternalSetValue(CenterPointProperty, mapTransform.Transform(TargetCenter));
                RemoveAnimation(CenterPointProperty); // remove holding animation in WPF
                UpdateTransform();
            }
        }

        private void CenterPointPropertyChanged(Point centerPoint)
        {
            if (!internalPropertyChange)
            {
                centerPoint.X = Location.NormalizeLongitude(centerPoint.X);
                InternalSetValue(CenterProperty, mapTransform.Transform(centerPoint));
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

        private void UpdateTransform(bool resetOrigin = false)
        {
            var center = transformOrigin ?? Center;

            SetViewportTransform(center);

            if (transformOrigin != null)
            {
                center = ViewportPointToLocation(new Point(RenderSize.Width / 2d, RenderSize.Height / 2d));
                center.Longitude = Location.NormalizeLongitude(center.Longitude);

                if (center.Latitude < -mapTransform.MaxLatitude || center.Latitude > mapTransform.MaxLatitude)
                {
                    center.Latitude = Math.Min(Math.Max(center.Latitude, -mapTransform.MaxLatitude), mapTransform.MaxLatitude);
                    resetOrigin = true;
                }

                InternalSetValue(CenterProperty, center);

                if (centerAnimation == null)
                {
                    InternalSetValue(TargetCenterProperty, center);
                    InternalSetValue(CenterPointProperty, mapTransform.Transform(center));
                }

                if (resetOrigin)
                {
                    ResetTransformOrigin();
                    SetViewportTransform(center);
                }
            }

            CenterScale = ViewportScale * mapTransform.RelativeScale(center) / TileSource.MetersPerDegree;
            scaleTransform.ScaleX = CenterScale;
            scaleTransform.ScaleY = CenterScale;
            rotateTransform.Angle = Heading;

            OnViewportChanged();
        }

        protected override void OnViewportChanged()
        {
            base.OnViewportChanged();

            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
