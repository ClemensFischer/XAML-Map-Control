// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

global using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class MapBase
    {
        public static readonly StyledProperty<Location> CenterProperty
            = AvaloniaProperty.Register<MapBase, Location>(nameof(Center), new Location(), false,
                BindingMode.TwoWay, null, (map, center) => ((MapBase)map).CoerceCenterProperty(center));

        public static readonly StyledProperty<Location> TargetCenterProperty
            = AvaloniaProperty.Register<MapBase, Location>(nameof(TargetCenter), new Location(), false,
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

        public static readonly StyledProperty<double> TargetZoomLevelProperty
            = AvaloniaProperty.Register<MapBase, double>(nameof(TargetZoomLevel), 1d, false,
                BindingMode.TwoWay, null, (map, zoomLevel) => ((MapBase)map).CoerceZoomLevelProperty(zoomLevel));

        public static readonly StyledProperty<double> HeadingProperty
            = AvaloniaProperty.Register<MapBase, double>(nameof(Heading), 0d, false,
                BindingMode.TwoWay, null, (map, heading) => CoerceHeadingProperty(heading));

        public static readonly StyledProperty<double> TargetHeadingProperty
            = AvaloniaProperty.Register<MapBase, double>(nameof(TargetHeading), 0d, false,
                BindingMode.TwoWay, null, (map, heading) => CoerceHeadingProperty(heading));

        public static readonly DirectProperty<MapBase, double> ViewScaleProperty
            = AvaloniaProperty.RegisterDirect<MapBase, double>(nameof(ViewScale), map => map.ViewScale);

        private CancellationTokenSource centerCts;
        private CancellationTokenSource zoomLevelCts;
        private CancellationTokenSource headingCts;
        private Animation centerAnimation;
        private Animation zoomLevelAnimation;
        private Animation headingAnimation;

        static MapBase()
        {
            ClipToBoundsProperty.OverrideDefaultValue(typeof(MapBase), true);

            CenterProperty.Changed.AddClassHandler<MapBase, Location>(
                (map, args) => map.CenterPropertyChanged(args.NewValue.Value));

            ZoomLevelProperty.Changed.AddClassHandler<MapBase, double>(
                (map, args) => map.ZoomLevelPropertyChanged(args.NewValue.Value));

            TargetZoomLevelProperty.Changed.AddClassHandler<MapBase, double>(
                async (map, args) => await map.TargetZoomLevelPropertyChanged(args.NewValue.Value));

            HeadingProperty.Changed.AddClassHandler<MapBase, double>(
                (map, args) => map.HeadingPropertyChanged(args.NewValue.Value));

            TargetHeadingProperty.Changed.AddClassHandler<MapBase, double>(
                async (map, args) => await map.TargetHeadingPropertyChanged(args.NewValue.Value));
        }

        public MapBase()
        {
            MapProjectionPropertyChanged(MapProjection);
        }

        internal Size RenderSize => Bounds.Size;

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            ResetTransformCenter();
            UpdateTransform();
        }

        /// <summary>
        /// Gets the scaling factor from projected map coordinates to view coordinates,
        /// as pixels per meter.
        /// </summary>
        public double ViewScale
        {
            get => ViewTransform.Scale;
        }

        private void SetViewScale(double viewScale)
        {
            RaisePropertyChanged(ViewScaleProperty, double.NaN, viewScale);
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

        private void CenterPropertyChanged(Location center)
        {
            if (!internalPropertyChange)
            {
                UpdateTransform();
            }
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

        private void ZoomLevelPropertyChanged(double zoomLevel)
        {
            if (!internalPropertyChange)
            {
                UpdateTransform();

                if (zoomLevelAnimation == null)
                {
                    SetValueInternal(TargetZoomLevelProperty, zoomLevel);
                }
            }
        }

        private async Task TargetZoomLevelPropertyChanged(double targetZoomLevel)
        {
            if (!internalPropertyChange && targetZoomLevel != ZoomLevel)
            {
                zoomLevelCts?.Cancel();

                zoomLevelAnimation = new Animation
                {
                    FillMode = FillMode.Forward,
                    Duration = AnimationDuration,
                    Children =
                    {
                        new KeyFrame
                        {
                            KeyTime = AnimationDuration,
                            Setters = { new Setter(ZoomLevelProperty, targetZoomLevel) }
                        }
                    }
                };

                zoomLevelCts = new CancellationTokenSource();

                await zoomLevelAnimation.RunAsync(this, zoomLevelCts.Token);

                zoomLevelCts.Dispose();
                zoomLevelCts = null;
                zoomLevelAnimation = null;
            }
        }

        private static double CoerceHeadingProperty(double heading)
        {
            return ((heading % 360d) + 360d) % 360d;
        }

        private void HeadingPropertyChanged(double heading)
        {
            if (!internalPropertyChange)
            {
                UpdateTransform();

                if (headingAnimation == null)
                {
                    SetValueInternal(TargetHeadingProperty, heading);
                }
            }
        }

        private async Task TargetHeadingPropertyChanged(double targetHeading)
        {
            if (!internalPropertyChange && targetHeading != Heading)
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

                targetHeading = Heading + delta;

                headingCts?.Cancel();

                headingAnimation = new Animation
                {
                    FillMode = FillMode.Forward,
                    Duration = AnimationDuration,
                    Children =
                    {
                        new KeyFrame
                        {
                            KeyTime = AnimationDuration,
                            Setters = { new Setter(HeadingProperty, targetHeading) }
                        }
                    }
                };

                headingCts = new CancellationTokenSource();

                await headingAnimation.RunAsync(this, headingCts.Token);

                headingCts.Dispose();
                headingCts = null;
                headingAnimation = null;
            }
        }
    }
}
