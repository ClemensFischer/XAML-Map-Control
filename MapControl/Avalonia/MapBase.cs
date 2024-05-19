// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

global using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl
{
    public interface IMapLayer
    {
        IBrush MapBackground { get; }
        IBrush MapForeground { get; }
    }

    public class MapBase : MapPanel
    {
        public static TimeSpan ImageFadeDuration { get; set; } = TimeSpan.FromSeconds(0.1);

        public static readonly StyledProperty<IBrush> ForegroundProperty
            = AvaloniaProperty.Register<MapBase, IBrush>(nameof(Foreground));

        public static readonly StyledProperty<TimeSpan> AnimationDurationProperty
            = AvaloniaProperty.Register<MapBase, TimeSpan>(nameof(AnimationDuration), TimeSpan.FromSeconds(0.3));

        public static readonly StyledProperty<Control> MapLayerProperty
            = AvaloniaProperty.Register<MapBase, Control>(nameof(MapLayer));

        public static readonly StyledProperty<MapProjection> MapProjectionProperty
            = AvaloniaProperty.Register<MapBase, MapProjection>(nameof(MapProjection), new WebMercatorProjection());

        public static readonly StyledProperty<Location> ProjectionCenterProperty
            = AvaloniaProperty.Register<MapBase, Location>(nameof(ProjectionCenter));

        public static readonly StyledProperty<Location> CenterProperty
            = AvaloniaProperty.Register<MapBase, Location>(nameof(Center), new Location(), false,
                BindingMode.TwoWay, null, (map, center) => ((MapBase)map).CoerceCenterProperty(center));

        public static readonly StyledProperty<double> MinZoomLevelProperty
            = AvaloniaProperty.Register<MapBase, double>(nameof(MinZoomLevel), 1d, false,
                BindingMode.OneWay, null, (map, minZoomLevel) => ((MapBase)map).CoerceMinZoomLevelProperty(minZoomLevel));

        public static readonly StyledProperty<double> MaxZoomLevelProperty
            = AvaloniaProperty.Register<MapBase, double>(nameof(MaxZoomLevel), 20d, false,
                BindingMode.OneWay, null, (map, maxZoomLevel) => ((MapBase)map).CoerceMaxZoomLevelProperty(maxZoomLevel));

        public static readonly StyledProperty<double> ZoomLevelProperty
            = AvaloniaProperty.Register<MapBase, double>(nameof(ZoomLevel), 1d, false,
                BindingMode.TwoWay, null, (map, zoomLevel) => ((MapBase)map).CoerceZoomLevelProperty(zoomLevel));

        public static readonly StyledProperty<double> HeadingProperty
            = AvaloniaProperty.Register<MapBase, double>(nameof(Heading), 0d, false,
                BindingMode.TwoWay, null, (map, heading) => ((heading % 360d) + 360d) % 360d);

        public static readonly DirectProperty<MapBase, double> ViewScaleProperty
            = AvaloniaProperty.RegisterDirect<MapBase, double>(nameof(ViewScale), map => map.ViewScale);

        private Location transformCenter;
        private Point viewCenter;
        private double centerLongitude;
        private double maxLatitude = 90d;

        static MapBase()
        {
            MapLayerProperty.Changed.AddClassHandler<MapBase, Control>(
                (map, args) => map.MapLayerPropertyChanged(args));

            MapProjectionProperty.Changed.AddClassHandler<MapBase, MapProjection>(
                (map, args) => map.MapProjectionPropertyChanged(args.NewValue.Value));

            ProjectionCenterProperty.Changed.AddClassHandler<MapBase, Location>(
                (map, args) => map.ProjectionCenterPropertyChanged());

            CenterProperty.Changed.AddClassHandler<MapBase, Location>(
                (map, args) => map.UpdateTransform());

            ZoomLevelProperty.Changed.AddClassHandler<MapBase, double>(
                (map, args) => map.UpdateTransform());

            HeadingProperty.Changed.AddClassHandler<MapBase, double>(
                (map, args) => map.UpdateTransform());
        }

        public MapBase()
        {
            MapProjectionPropertyChanged(MapProjection);
        }

        /// <summary>
        /// Raised when the current map viewport has changed.
        /// </summary>
        public event EventHandler<ViewportChangedEventArgs> ViewportChanged;

        /// <summary>
        /// Gets or sets the map foreground Brush.
        /// </summary>
        public IBrush Foreground
        {
            get => GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the Duration of the Center, ZoomLevel and Heading animations.
        /// The default value is 0.3 seconds.
        /// </summary>
        public TimeSpan AnimationDuration
        {
            get => GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        /// <summary>
        /// Gets or sets the base map layer, which is added as first element to the Children collection.
        /// If the layer implements IMapLayer (like MapTileLayer or MapImageLayer), its (non-null) MapBackground
        /// and MapForeground property values are used for the MapBase Background and Foreground properties.
        /// </summary>
        public Control MapLayer
        {
            get => GetValue(MapLayerProperty);
            set => SetValue(MapLayerProperty, value);
        }

        /// <summary>
        /// Gets or sets the MapProjection used by the map control.
        /// </summary>
        public MapProjection MapProjection
        {
            get => GetValue(MapProjectionProperty);
            set => SetValue(MapProjectionProperty, value);
        }

        /// <summary>
        /// Gets or sets an optional center (reference point) for azimuthal projections.
        /// If ProjectionCenter is null, the Center property value will be used instead.
        /// </summary>
        public Location ProjectionCenter
        {
            get => GetValue(ProjectionCenterProperty);
            set => SetValue(ProjectionCenterProperty, value);
        }

        /// <summary>
        /// Gets or sets the location of the center point of the map.
        /// </summary>
        public Location Center
        {
            get => GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }

        /// <summary>
        /// Gets or sets the minimum value of the ZoomLevel property.
        /// Must not be less than zero or greater than MaxZoomLevel. The default value is 1.
        /// </summary>
        public double MinZoomLevel
        {
            get => GetValue(MinZoomLevelProperty);
            set => SetValue(MinZoomLevelProperty, value);
        }

        /// <summary>
        /// Gets or sets the maximum value of the ZoomLevel property.
        /// Must not be less than MinZoomLevel. The default value is 20.
        /// </summary>
        public double MaxZoomLevel
        {
            get => GetValue(MaxZoomLevelProperty);
            set => SetValue(MaxZoomLevelProperty, value);
        }

        /// <summary>
        /// Gets or sets the map zoom level.
        /// </summary>
        public double ZoomLevel
        {
            get => GetValue(ZoomLevelProperty);
            set => SetValue(ZoomLevelProperty, value);
        }

        /// <summary>
        /// Gets or sets the map heading, a counter-clockwise rotation angle in degrees.
        /// </summary>
        public double Heading
        {
            get => GetValue(HeadingProperty);
            set => SetValue(HeadingProperty, value);
        }

        /// <summary>
        /// Gets the scaling factor from projected map coordinates to view coordinates,
        /// as pixels per meter.
        /// </summary>
        public double ViewScale
        {
            get => ViewTransform.Scale;
        }

        /// <summary>
        /// Gets the ViewTransform instance that is used to transform between projected
        /// map coordinates and view coordinates.
        /// </summary>
        public ViewTransform ViewTransform { get; } = new ViewTransform();

        /// <summary>
        /// Gets the map scale as the horizontal and vertical scaling factors from geographic
        /// coordinates to view coordinates at the specified location, as pixels per meter.
        /// </summary>
        public Point GetScale(Location location)
        {
            return MapProjection.GetRelativeScale(location) * ViewTransform.Scale;
        }

        /// <summary>
        /// Gets a transform Matrix for scaling and rotating objects that are anchored
        /// at a Location from map coordinates (i.e. meters) to view coordinates.
        /// </summary>
        public Matrix GetMapTransform(Location location)
        {
            var scale = GetScale(location);

            return new Matrix(scale.X, 0d, 0d, scale.Y, 0d, 0d)
                .Append(Matrix.CreateRotation(ViewTransform.Rotation));
        }

        /// <summary>
        /// Transforms a Location in geographic coordinates to a Point in view coordinates.
        /// </summary>
        public Point? LocationToView(Location location)
        {
            var point = MapProjection.LocationToMap(location);

            return point.HasValue ? ViewTransform.MapToView(point.Value) : null;
        }

        /// <summary>
        /// Transforms a Point in view coordinates to a Location in geographic coordinates.
        /// </summary>
        public Location ViewToLocation(Point point)
        {
            return MapProjection.MapToLocation(ViewTransform.ViewToMap(point));
        }

        /// <summary>
        /// Transforms a Rect in view coordinates to a BoundingBox in geographic coordinates.
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

            return MapProjection.MapToBoundingBox(new Rect(x1, y1, x2, y2));
        }

        /// <summary>
        /// Sets a temporary center point in view coordinates for scaling and rotation transformations.
        /// This center point is automatically reset when the Center property is set by application code
        /// or by the methods TranslateMap, TransformMap, ZoomMap and ZoomToBounds.
        /// </summary>
        public void SetTransformCenter(Point center)
        {
            transformCenter = ViewToLocation(center);
            viewCenter = transformCenter != null ? center : new Point(Bounds.Width / 2d, Bounds.Height / 2d);
        }

        /// <summary>
        /// Resets the temporary transform center point set by SetTransformCenter.
        /// </summary>
        public void ResetTransformCenter()
        {
            transformCenter = null;
            viewCenter = new Point(Bounds.Width / 2d, Bounds.Height / 2d);
        }

        /// <summary>
        /// Changes the Center property according to the specified translation in view coordinates.
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
            if (rotation != 0d || scale != 1d)
            {
                SetTransformCenter(center);

                viewCenter += translation;

                if (rotation != 0d)
                {
                    Heading = (((Heading - rotation) % 360d) + 360d) % 360d;
                }

                if (scale != 1d)
                {
                    ZoomLevel = Math.Min(Math.Max(ZoomLevel + Math.Log(scale, 2d), MinZoomLevel), MaxZoomLevel);
                }

                UpdateTransform(true);
            }
            else
            {
                // More accurate than SetTransformCenter.
                //
                TranslateMap(translation);
            }
        }

        /// <summary>
        /// Animates the value of the ZoomLevel property while retaining the specified center point
        /// in view coordinates.
        /// </summary>
        public async Task ZoomMap(Point center, double zoomLevel, CancellationToken cancellationToken = default)
        {
            zoomLevel = Math.Min(Math.Max(zoomLevel, MinZoomLevel), MaxZoomLevel);

            if (zoomLevel != ZoomLevel)
            {
                SetTransformCenter(center);

                var animation = new Animation
                {
                    FillMode = FillMode.Forward,
                    Duration = AnimationDuration,
                    Children =
                    {
                        new KeyFrame
                        {
                            KeyTime = AnimationDuration,
                            Setters = { new Setter(ZoomLevelProperty, zoomLevel) }
                        }
                    }
                };

                await animation.RunAsync(this, cancellationToken);
            }
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            ResetTransformCenter();
            UpdateTransform();
        }

        internal double ConstrainedLongitude(double longitude)
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

        private void UpdateTransform(bool resetTransformCenter = false, bool projectionChanged = false)
        {
            var transformCenterChanged = false;
            var oldViewScale = ViewScale;
            var viewScale = ViewTransform.ZoomLevelToScale(ZoomLevel);
            var projection = MapProjection;

            projection.Center = ProjectionCenter ?? Center;

            var mapCenter = projection.LocationToMap(transformCenter ?? Center);

            if (mapCenter.HasValue)
            {
                ViewTransform.SetTransform(mapCenter.Value, viewCenter, viewScale, -Heading);

                if (transformCenter != null)
                {
                    var center = ViewToLocation(new Point(Bounds.Width / 2d, Bounds.Height / 2d));

                    if (center != null)
                    {
                        var centerLatitude = center.Latitude;
                        var centerLongitude = Location.NormalizeLongitude(center.Longitude);

                        if (centerLatitude < -maxLatitude || centerLatitude > maxLatitude)
                        {
                            centerLatitude = Math.Min(Math.Max(centerLatitude, -maxLatitude), maxLatitude);
                            resetTransformCenter = true;
                        }

                        Center = new Location(centerLatitude, centerLongitude);

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

                RaisePropertyChanged(ViewScaleProperty, oldViewScale, ViewScale);

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

        private void MapLayerPropertyChanged(AvaloniaPropertyChangedEventArgs<Control> args)
        {
            if (args.OldValue.Value != null)
            {
                Children.Remove(args.OldValue.Value);

                if (args.OldValue.Value is IMapLayer mapLayer)
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

            if (args.NewValue.Value != null)
            {
                Children.Insert(0, args.NewValue.Value);

                if (args.NewValue.Value is IMapLayer mapLayer)
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

        private void MapProjectionPropertyChanged(MapProjection projection)
        {
            maxLatitude = 90d;

            if (projection.Type <= MapProjectionType.NormalCylindrical)
            {
                var maxLocation = projection.MapToLocation(new Point(0d, 180d * MapProjection.Wgs84MeterPerDegree));

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
    }
}
