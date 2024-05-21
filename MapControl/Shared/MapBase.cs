// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if AVALONIA
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using DependencyProperty = Avalonia.AvaloniaProperty;
using UIElement = Avalonia.Controls.Control;
#elif WINUI
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#elif UWP
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    public interface IMapLayer : IMapElement
    {
        Brush MapBackground { get; }
        Brush MapForeground { get; }
    }

    /// <summary>
    /// The map control. Displays map content provided by one or more tile or image layers,
    /// such as MapTileLayerBase or MapImageLayer instances.
    /// The visible map area is defined by the Center and ZoomLevel properties.
    /// The map can be rotated by an angle that is given by the Heading property.
    /// MapBase can contain map overlay child elements like other MapPanels or MapItemsControls.
    /// </summary>
    public partial class MapBase : MapPanel
    {
        public static TimeSpan ImageFadeDuration { get; set; } = TimeSpan.FromSeconds(0.1);

        public static readonly DependencyProperty ForegroundProperty =
            DependencyPropertyHelper.Register<MapBase, Brush>(nameof(Foreground), new SolidColorBrush(Colors.Black));

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyPropertyHelper.Register<MapBase, TimeSpan>(nameof(AnimationDuration), TimeSpan.FromSeconds(0.3));

        public static readonly DependencyProperty MapLayerProperty =
            DependencyPropertyHelper.Register<MapBase, UIElement>(nameof(MapLayer), null, false,
                (map, oldValue, newValue) => map.MapLayerPropertyChanged(oldValue, newValue));

        public static readonly DependencyProperty MapProjectionProperty =
            DependencyPropertyHelper.Register<MapBase, MapProjection>(nameof(MapProjection), new WebMercatorProjection(), false,
                (map, oldValue, newValue) => map.MapProjectionPropertyChanged(newValue));

        public static readonly DependencyProperty ProjectionCenterProperty =
            DependencyPropertyHelper.Register<MapBase, Location>(nameof(ProjectionCenter), null, false,
                (map, oldValue, newValue) => map.ProjectionCenterPropertyChanged());

        private Location transformCenter;
        private Point viewCenter;
        private double centerLongitude;
        private double maxLatitude = 90d;
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
        /// Gets or sets the Duration of the Center, ZoomLevel and Heading animations.
        /// The default value is 0.3 seconds.
        /// </summary>
        public TimeSpan AnimationDuration
        {
            get => (TimeSpan)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        /// <summary>
        /// Gets or sets the base map layer, which is added as first element to the Children collection.
        /// If the layer implements IMapLayer (like MapTileLayer or MapImageLayer), its (non-null) MapBackground
        /// and MapForeground property values are used for the MapBase Background and Foreground properties.
        /// </summary>
        public UIElement MapLayer
        {
            get => (UIElement)GetValue(MapLayerProperty);
            set => SetValue(MapLayerProperty, value);
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
        /// Gets the map scale as the horizontal and vertical scaling factors from geographic
        /// coordinates to view coordinates at the specified location, as pixels per meter.
        /// </summary>
        public Point GetScale(Location location)
        {
            var relativeScale = MapProjection.GetRelativeScale(location);

            return new Point(ViewTransform.Scale * relativeScale.X, ViewTransform.Scale * relativeScale.Y);
        }

        /// <summary>
        /// Transforms a Location in geographic coordinates to a Point in view coordinates.
        /// </summary>
        public Point? LocationToView(Location location)
        {
            var point = MapProjection.LocationToMap(location);

            if (!point.HasValue)
            {
                return null;
            }

            return ViewTransform.MapToView(point.Value);
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
            viewCenter = transformCenter != null ? center : new Point(RenderSize.Width / 2d, RenderSize.Height / 2d);
        }

        /// <summary>
        /// Resets the temporary transform center point set by SetTransformCenter.
        /// </summary>
        public void ResetTransformCenter()
        {
            transformCenter = null;
            viewCenter = new Point(RenderSize.Width / 2d, RenderSize.Height / 2d);
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

                viewCenter = new Point(viewCenter.X + translation.X, viewCenter.Y + translation.Y);

                if (rotation != 0d)
                {
                    var heading = (((Heading - rotation) % 360d) + 360d) % 360d;

                    SetValueInternal(HeadingProperty, heading);
                    SetValueInternal(TargetHeadingProperty, heading);
                }

                if (scale != 1d)
                {
                    var zoomLevel = Math.Min(Math.Max(ZoomLevel + Math.Log(scale, 2d), MinZoomLevel), MaxZoomLevel);

                    SetValueInternal(ZoomLevelProperty, zoomLevel);
                    SetValueInternal(TargetZoomLevelProperty, zoomLevel);
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
        /// Sets the value of the TargetZoomLevel property while retaining the specified center point
        /// in view coordinates.
        /// </summary>
        public void ZoomMap(Point center, double zoomLevel)
        {
            zoomLevel = CoerceZoomLevelProperty(zoomLevel);

            if (TargetZoomLevel != zoomLevel)
            {
                SetTransformCenter(center);

                TargetZoomLevel = zoomLevel;
            }
        }

        /// <summary>
        /// Sets the TargetZoomLevel and TargetCenter properties so that the specified bounding box
        /// fits into the current view. The TargetHeading property is set to zero.
        /// </summary>
        public void ZoomToBounds(BoundingBox boundingBox)
        {
            var rect = MapProjection.BoundingBoxToMap(boundingBox);

            if (rect.HasValue)
            {
                var rectCenter = new Point(rect.Value.X + rect.Value.Width / 2d, rect.Value.Y + rect.Value.Height / 2d);
                var targetCenter = MapProjection.MapToLocation(rectCenter);

                if (targetCenter != null)
                {
                    var scale = Math.Min(RenderSize.Width / rect.Value.Width, RenderSize.Height / rect.Value.Height);

                    TargetZoomLevel = ViewTransform.ScaleToZoomLevel(scale);
                    TargetCenter = targetCenter;
                    TargetHeading = 0d;
                }
            }
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

        private void MapLayerPropertyChanged(UIElement oldLayer, UIElement newLayer)
        {
            if (oldLayer != null)
            {
                Children.Remove(oldLayer);

                if (oldLayer is IMapLayer mapLayer)
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

                if (newLayer is IMapLayer mapLayer)
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

        private void UpdateTransform(bool resetTransformCenter = false, bool projectionChanged = false)
        {
            var transformCenterChanged = false;
            var viewScale = ViewTransform.ZoomLevelToScale(ZoomLevel);
            var projection = MapProjection;

            projection.Center = ProjectionCenter ?? Center;

            var mapCenter = projection.LocationToMap(transformCenter ?? Center);

            if (mapCenter.HasValue)
            {
                ViewTransform.SetTransform(mapCenter.Value, viewCenter, viewScale, -Heading);

                if (transformCenter != null)
                {
                    var center = ViewToLocation(new Point(RenderSize.Width / 2d, RenderSize.Height / 2d));

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

                SetViewScale(ViewTransform.Scale);

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
