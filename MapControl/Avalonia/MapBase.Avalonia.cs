// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

global using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using System.Threading;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class MapBase
    {
        public static readonly StyledProperty<Location> CenterProperty =
            DependencyPropertyHelper.Register<MapBase, Location>(nameof(Center), new Location(), true,
                (map, oldValue, newValue) => map.CenterPropertyChanged(newValue),
                (map, value) => map.CoerceCenterProperty(value));

        public static readonly StyledProperty<Location> TargetCenterProperty =
           DependencyPropertyHelper.Register<MapBase, Location>(nameof(TargetCenter), new Location(), true,
                async (map, oldValue, newValue) => await map.TargetCenterPropertyChanged(newValue),
                (map, value) => map.CoerceCenterProperty(value));

        public static readonly StyledProperty<double> MinZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(MinZoomLevel), 1d, false,
                (map, oldValue, newValue) => map.MinZoomLevelPropertyChanged(newValue),
                (map, value) => map.CoerceMinZoomLevelProperty(value));

        public static readonly StyledProperty<double> MaxZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(MaxZoomLevel), 20d, false,
                (map, oldValue, newValue) => map.MaxZoomLevelPropertyChanged(newValue),
                (map, value) => map.CoerceMinZoomLevelProperty(value));

        public static readonly StyledProperty<double> ZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(ZoomLevel), 1d, true,
                (map, oldValue, newValue) => map.ZoomLevelPropertyChanged(newValue),
                (map, value) => map.CoerceZoomLevelProperty(value));

        public static readonly StyledProperty<double> TargetZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(TargetZoomLevel), 1d, true,
                async (map, oldValue, newValue) => await map.TargetZoomLevelPropertyChanged(newValue),
                (map, value) => map.CoerceZoomLevelProperty(value));

        public static readonly StyledProperty<double> HeadingProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(Heading), 0d, true,
                (map, oldValue, newValue) => map.HeadingPropertyChanged(newValue),
                (map, value) => map.CoerceHeadingProperty(value));

        public static readonly StyledProperty<double> TargetHeadingProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(TargetHeading), 0d, true,
                async (map, oldValue, newValue) => await map.TargetHeadingPropertyChanged(newValue),
                (map, value) => map.CoerceHeadingProperty(value));

        public static readonly DirectProperty<MapBase, double> ViewScaleProperty =
            DependencyPropertyHelper.RegisterReadOnly<MapBase, double>(nameof(ViewScale), map => map.ViewScale);

        private CancellationTokenSource centerCts;
        private CancellationTokenSource zoomLevelCts;
        private CancellationTokenSource headingCts;
        private Animation centerAnimation;
        private Animation zoomLevelAnimation;
        private Animation headingAnimation;

        static MapBase()
        {
            ClipToBoundsProperty.OverrideDefaultValue(typeof(MapBase), true);

            Animation.RegisterCustomAnimator<Location, LocationAnimator>();
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

        private void CenterPropertyChanged(Location center)
        {
            if (!internalPropertyChange)
            {
                UpdateTransform();

                if (centerAnimation == null)
                {
                    SetValueInternal(TargetCenterProperty, center);
                }
            }
        }

        private async Task TargetCenterPropertyChanged(Location targetCenter)
        {
            if (!internalPropertyChange && !targetCenter.Equals(Center))
            {
                centerCts?.Cancel();

                centerAnimation = new Animation
                {
                    FillMode = FillMode.Forward,
                    Duration = AnimationDuration,
                    Children =
                    {
                        new KeyFrame
                        {
                            KeyTime = AnimationDuration,
                            Setters = { new Setter(CenterProperty, new Location(targetCenter.Latitude, ConstrainedLongitude(targetCenter.Longitude))) }
                        }
                    }
                };

                centerCts = new CancellationTokenSource();

                await centerAnimation.RunAsync(this, centerCts.Token);

                centerCts.Dispose();
                centerCts = null;
                centerAnimation = null;
            }
        }

        private void MinZoomLevelPropertyChanged(double minZoomLevel)
        {
            if (ZoomLevel < minZoomLevel)
            {
                ZoomLevel = minZoomLevel;
            }
        }

        private void MaxZoomLevelPropertyChanged(double maxZoomLevel)
        {
            if (ZoomLevel > maxZoomLevel)
            {
                ZoomLevel = maxZoomLevel;
            }
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

                if (!zoomLevelCts.IsCancellationRequested)
                {
                    UpdateTransform(true);
                }

                zoomLevelCts.Dispose();
                zoomLevelCts = null;
                zoomLevelAnimation = null;
            }
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
