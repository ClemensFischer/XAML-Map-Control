// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

global using Avalonia;
global using Avalonia.Animation;
global using Avalonia.Animation.Easings;
global using Avalonia.Controls;
global using Avalonia.Controls.Documents;
global using Avalonia.Controls.Shapes;
global using Avalonia.Data;
global using Avalonia.Input;
global using Avalonia.Interactivity;
global using Avalonia.Media;
global using Avalonia.Media.Imaging;
global using Avalonia.Platform;
global using Avalonia.Styling;
global using Avalonia.Threading;
global using Brush = Avalonia.Media.IBrush;
global using ImageSource = Avalonia.Media.IImage;
global using DependencyObject = Avalonia.AvaloniaObject;
global using DependencyProperty = Avalonia.AvaloniaProperty;
global using FrameworkElement = Avalonia.Controls.Control;
global using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
global using VerticalAlignment = Avalonia.Layout.VerticalAlignment;
global using PathFigureCollection = Avalonia.Media.PathFigures;
global using PointCollection = System.Collections.Generic.List<Avalonia.Point>;
global using PropertyPath = System.String;

using System.Threading;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class MapBase
    {
        public static readonly StyledProperty<Brush> ForegroundProperty =
            DependencyPropertyHelper.AddOwner<MapBase, Brush>(TextElement.ForegroundProperty);

        public static readonly StyledProperty<Easing> AnimationEasingProperty =
            DependencyPropertyHelper.Register<MapBase, Easing>(nameof(AnimationEasing), new QuadraticEaseOut());

        public static readonly StyledProperty<Location> CenterProperty =
            DependencyPropertyHelper.Register<MapBase, Location>(nameof(Center), new Location(),
                (map, oldValue, newValue) => map.CenterPropertyChanged(newValue),
                (map, value) => map.CoerceCenterProperty(value),
                true);

        public static readonly StyledProperty<Location> TargetCenterProperty =
           DependencyPropertyHelper.Register<MapBase, Location>(nameof(TargetCenter), new Location(),
                async (map, oldValue, newValue) => await map.TargetCenterPropertyChanged(newValue),
                (map, value) => map.CoerceCenterProperty(value),
                true);

        public static readonly StyledProperty<double> MinZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(MinZoomLevel), 1d,
                (map, oldValue, newValue) => map.MinZoomLevelPropertyChanged(newValue),
                (map, value) => map.CoerceMinZoomLevelProperty(value));

        public static readonly StyledProperty<double> MaxZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(MaxZoomLevel), 20d,
                (map, oldValue, newValue) => map.MaxZoomLevelPropertyChanged(newValue),
                (map, value) => map.CoerceMaxZoomLevelProperty(value));

        public static readonly StyledProperty<double> ZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(ZoomLevel), 1d,
                (map, oldValue, newValue) => map.ZoomLevelPropertyChanged(newValue),
                (map, value) => map.CoerceZoomLevelProperty(value),
                true);

        public static readonly StyledProperty<double> TargetZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(TargetZoomLevel), 1d,
                async (map, oldValue, newValue) => await map.TargetZoomLevelPropertyChanged(newValue),
                (map, value) => map.CoerceZoomLevelProperty(value),
                true);

        public static readonly StyledProperty<double> HeadingProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(Heading), 0d,
                (map, oldValue, newValue) => map.HeadingPropertyChanged(newValue),
                (map, value) => map.CoerceHeadingProperty(value),
                true);

        public static readonly StyledProperty<double> TargetHeadingProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(TargetHeading), 0d,
                async (map, oldValue, newValue) => await map.TargetHeadingPropertyChanged(newValue),
                (map, value) => map.CoerceHeadingProperty(value),
                true);

        public static readonly DirectProperty<MapBase, double> ViewScaleProperty =
            AvaloniaProperty.RegisterDirect<MapBase, double>(nameof(ViewScale), map => map.ViewTransform.Scale);

        private CancellationTokenSource centerCts;
        private CancellationTokenSource zoomLevelCts;
        private CancellationTokenSource headingCts;
        private Animation centerAnimation;
        private Animation zoomLevelAnimation;
        private Animation headingAnimation;

        static MapBase()
        {
            BackgroundProperty.OverrideDefaultValue(typeof(MapBase), Brushes.White);
            ClipToBoundsProperty.OverrideDefaultValue(typeof(MapBase), true);

            Animation.RegisterCustomAnimator<Location, LocationAnimator>();
        }

        internal Size RenderSize => Bounds.Size;

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            ResetTransformCenter();
            UpdateTransform();
        }

        /// <summary>
        /// Gets or sets the Easing of the Center, ZoomLevel and Heading animations.
        /// The default value is a QuadraticEaseOut.
        /// </summary>
        public Easing AnimationEasing
        {
            get => GetValue(AnimationEasingProperty);
            set => SetValue(AnimationEasingProperty, value);
        }

        /// <summary>
        /// Gets the scaling factor from projected map coordinates to view coordinates,
        /// as pixels per meter.
        /// </summary>
        public double ViewScale
        {
            get => GetValue(ViewScaleProperty);
            private set => RaisePropertyChanged(ViewScaleProperty, double.NaN, value);
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
                ResetTransformCenter();

                centerCts?.Cancel();

                centerAnimation = new Animation
                {
                    FillMode = FillMode.Forward,
                    Duration = AnimationDuration,
                    Easing = AnimationEasing,
                    Children =
                    {
                        new KeyFrame
                        {
                            KeyTime = AnimationDuration,
                            Setters = { new Setter(CenterProperty, new Location(targetCenter.Latitude, CoerceLongitude(targetCenter.Longitude))) }
                        }
                    }
                };

                centerCts = new CancellationTokenSource();

                await centerAnimation.RunAsync(this, centerCts.Token);

                if (!centerCts.IsCancellationRequested)
                {
                    UpdateTransform();
                }

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
                    Easing = AnimationEasing,
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
                    UpdateTransform(true); // reset transform center
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
                    Easing = AnimationEasing,
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

                if (!headingCts.IsCancellationRequested)
                {
                    UpdateTransform();
                }

                headingCts.Dispose();
                headingCts = null;
                headingAnimation = null;
            }
        }
    }
}
