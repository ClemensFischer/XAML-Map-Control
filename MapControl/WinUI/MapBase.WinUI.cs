// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
#else
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#endif

namespace MapControl
{
    public partial class MapBase
    {
        public static readonly DependencyProperty AnimationEasingFunctionProperty =
            DependencyPropertyHelper.Register<MapBase, EasingFunctionBase>(nameof(AnimationEasingFunction),
                new QuadraticEase { EasingMode = EasingMode.EaseOut });

        public static readonly DependencyProperty CenterProperty =
            DependencyPropertyHelper.Register<MapBase, Location>(nameof(Center), new Location(), true,
                (map, oldValue, newValue) => map.CenterPropertyChanged(newValue));

        public static readonly DependencyProperty TargetCenterProperty =
           DependencyPropertyHelper.Register<MapBase, Location>(nameof(TargetCenter), new Location(), true,
                (map, oldValue, newValue) => map.TargetCenterPropertyChanged(newValue));

        public static readonly DependencyProperty MinZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(MinZoomLevel), 1d, false,
                (map, oldValue, newValue) => map.MinZoomLevelPropertyChanged(newValue));

        public static readonly DependencyProperty MaxZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(MaxZoomLevel), 20d, false,
                (map, oldValue, newValue) => map.MaxZoomLevelPropertyChanged(newValue));

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(ZoomLevel), 1d, true,
                (map, oldValue, newValue) => map.ZoomLevelPropertyChanged(newValue));

        public static readonly DependencyProperty TargetZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(TargetZoomLevel), 1d, true,
                (map, oldValue, newValue) => map.TargetZoomLevelPropertyChanged(newValue));

        public static readonly DependencyProperty HeadingProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(Heading), 0d, true,
                (map, oldValue, newValue) => map.HeadingPropertyChanged(newValue));

        public static readonly DependencyProperty TargetHeadingProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(TargetHeading), 0d, true,
                (map, oldValue, newValue) => map.TargetHeadingPropertyChanged(newValue));

        public static readonly DependencyProperty ViewScaleProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(ViewScale), 0d);

        private static readonly DependencyProperty AnimatedCenterProperty =
            DependencyPropertyHelper.Register<MapBase, Windows.Foundation.Point>(nameof(AnimatedCenter),
                new Windows.Foundation.Point(), false, (map, oldValue, newValue) => map.Center = new Location(newValue.Y, newValue.X));

        private Windows.Foundation.Point AnimatedCenter => (Windows.Foundation.Point)GetValue(AnimatedCenterProperty);

        private PointAnimation centerAnimation;
        private DoubleAnimation zoomLevelAnimation;
        private DoubleAnimation headingAnimation;

        public MapBase()
        {
            // Set Background by Style to enable resetting by ClearValue in MapLayerPropertyChanged.
            //
            var style = new Style(typeof(MapBase));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Colors.White)));
            Style = style;

            SizeChanged += OnSizeChanged;
        }

        /// <summary>
        /// Gets or sets the EasingFunction of the Center, ZoomLevel and Heading animations.
        /// The default value is a QuadraticEase with EasingMode.EaseOut.
        /// </summary>
        public EasingFunctionBase AnimationEasingFunction
        {
            get => (EasingFunctionBase)GetValue(AnimationEasingFunctionProperty);
            set => SetValue(AnimationEasingFunctionProperty, value);
        }

        /// <summary>
        /// Gets the scaling factor from projected map coordinates to view coordinates,
        /// as pixels per meter.
        /// </summary>
        public double ViewScale
        {
            get => (double)GetValue(ViewScaleProperty);
            private set => SetValue(ViewScaleProperty, value);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Clip = new RectangleGeometry
            {
                Rect = new Windows.Foundation.Rect(0d, 0d, e.NewSize.Width, e.NewSize.Height)
            };

            ResetTransformCenter();
            UpdateTransform();
        }

        private void CenterPropertyChanged(Location value)
        {
            if (!internalPropertyChange)
            {
                var center = CoerceCenterProperty(value);

                if (!center.Equals(value))
                {
                    SetValueInternal(CenterProperty, center);
                }

                UpdateTransform();

                if (centerAnimation == null)
                {
                    SetValueInternal(TargetCenterProperty, center);
                }
            }
        }

        private void TargetCenterPropertyChanged(Location value)
        {
            if (!internalPropertyChange)
            {
                ResetTransformCenter();

                var targetCenter = CoerceCenterProperty(value);

                if (!targetCenter.Equals(value))
                {
                    SetValueInternal(TargetCenterProperty, targetCenter);
                }

                if (!targetCenter.Equals(Center))
                {
                    if (centerAnimation != null)
                    {
                        centerAnimation.Completed -= CenterAnimationCompleted;
                    }

                    centerAnimation = new PointAnimation
                    {
                        From = new Windows.Foundation.Point(Center.Longitude, Center.Latitude),
                        To = new Windows.Foundation.Point(CoerceLongitude(targetCenter.Longitude), targetCenter.Latitude),
                        Duration = AnimationDuration,
                        EasingFunction = AnimationEasingFunction,
                        EnableDependentAnimation = true
                    };

                    centerAnimation.Completed += CenterAnimationCompleted;

                    BeginAnimation(nameof(AnimatedCenter), centerAnimation);
                }
            }
        }

        private void CenterAnimationCompleted(object sender, object e)
        {
            if (centerAnimation != null)
            {
                SetValueInternal(CenterProperty, TargetCenter);
                UpdateTransform();

                centerAnimation.Completed -= CenterAnimationCompleted;
                centerAnimation = null;
            }
        }

        private void MinZoomLevelPropertyChanged(double value)
        {
            var minZoomLevel = CoerceMinZoomLevelProperty(value);

            if (minZoomLevel != value)
            {
                SetValueInternal(MinZoomLevelProperty, minZoomLevel);
            }

            if (ZoomLevel < minZoomLevel)
            {
                ZoomLevel = minZoomLevel;
            }
        }

        private void MaxZoomLevelPropertyChanged(double value)
        {
            var maxZoomLevel = CoerceMaxZoomLevelProperty(value);

            if (maxZoomLevel != value)
            {
                SetValueInternal(MaxZoomLevelProperty, maxZoomLevel);
            }

            if (ZoomLevel > maxZoomLevel)
            {
                ZoomLevel = maxZoomLevel;
            }
        }

        private void ZoomLevelPropertyChanged(double value)
        {
            if (!internalPropertyChange)
            {
                var zoomLevel = CoerceZoomLevelProperty(value);

                if (zoomLevel != value)
                {
                    SetValueInternal(ZoomLevelProperty, zoomLevel);
                }

                UpdateTransform();

                if (zoomLevelAnimation == null)
                {
                    SetValueInternal(TargetZoomLevelProperty, zoomLevel);
                }
            }
        }

        private void TargetZoomLevelPropertyChanged(double value)
        {
            if (!internalPropertyChange)
            {
                var targetZoomLevel = CoerceZoomLevelProperty(value);

                if (targetZoomLevel != value)
                {
                    SetValueInternal(TargetZoomLevelProperty, targetZoomLevel);
                }

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
                        EnableDependentAnimation = true
                    };

                    zoomLevelAnimation.Completed += ZoomLevelAnimationCompleted;

                    BeginAnimation(nameof(ZoomLevel), zoomLevelAnimation);
                }
            }
        }

        private void ZoomLevelAnimationCompleted(object sender, object e)
        {
            if (zoomLevelAnimation != null)
            {
                SetValueInternal(ZoomLevelProperty, TargetZoomLevel);
                UpdateTransform(true);

                zoomLevelAnimation.Completed -= ZoomLevelAnimationCompleted;
                zoomLevelAnimation = null;
            }
        }

        private void HeadingPropertyChanged(double value)
        {
            if (!internalPropertyChange)
            {
                var heading = CoerceHeadingProperty(value);

                if (heading != value)
                {
                    SetValueInternal(HeadingProperty, heading);
                }

                UpdateTransform();

                if (headingAnimation == null)
                {
                    SetValueInternal(TargetHeadingProperty, heading);
                }
            }
        }

        private void TargetHeadingPropertyChanged(double value)
        {
            if (!internalPropertyChange)
            {
                var targetHeading = CoerceHeadingProperty(value);

                if (targetHeading != value)
                {
                    SetValueInternal(TargetHeadingProperty, targetHeading);
                }

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
                        EnableDependentAnimation = true
                    };

                    headingAnimation.Completed += HeadingAnimationCompleted;

                    BeginAnimation(nameof(Heading), headingAnimation);
                }
            }
        }

        private void HeadingAnimationCompleted(object sender, object e)
        {
            if (headingAnimation != null)
            {
                SetValueInternal(HeadingProperty, TargetHeading);
                UpdateTransform();

                headingAnimation.Completed -= HeadingAnimationCompleted;
                headingAnimation = null;
            }
        }

        private void BeginAnimation(string property, Timeline animation)
        {
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, property);

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }
}
