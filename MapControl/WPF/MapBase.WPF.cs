// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MapControl
{
    public partial class MapBase
    {
        public static readonly DependencyProperty AnimationEasingFunctionProperty =
            DependencyPropertyHelper.Register<MapBase, IEasingFunction>(nameof(AnimationEasingFunction),
                new QuadraticEase { EasingMode = EasingMode.EaseOut });

        public static readonly DependencyProperty CenterProperty =
            DependencyPropertyHelper.Register<MapBase, Location>(nameof(Center), new Location(), true,
                (map, oldValue, newValue) => map.CenterPropertyChanged(newValue),
                (map, value) => map.CoerceCenterProperty(value));

        public static readonly DependencyProperty TargetCenterProperty =
           DependencyPropertyHelper.Register<MapBase, Location>(nameof(TargetCenter), new Location(), true,
                (map, oldValue, newValue) => map.TargetCenterPropertyChanged(newValue),
                (map, value) => map.CoerceCenterProperty(value));

        public static readonly DependencyProperty MinZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(MinZoomLevel), 1d, false,
                (map, oldValue, newValue) => map.MinZoomLevelPropertyChanged(newValue),
                (map, value) => map.CoerceMinZoomLevelProperty(value));

        public static readonly DependencyProperty MaxZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(MaxZoomLevel), 20d, false,
                (map, oldValue, newValue) => map.MaxZoomLevelPropertyChanged(newValue),
                (map, value) => map.CoerceMinZoomLevelProperty(value));

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(ZoomLevel), 1d, true,
                (map, oldValue, newValue) => map.ZoomLevelPropertyChanged(newValue),
                (map, value) => map.CoerceZoomLevelProperty(value));

        public static readonly DependencyProperty TargetZoomLevelProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(TargetZoomLevel), 1d, true,
                (map, oldValue, newValue) => map.TargetZoomLevelPropertyChanged(newValue),
                (map, value) => map.CoerceZoomLevelProperty(value));

        public static readonly DependencyProperty HeadingProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(Heading), 0d, true,
                (map, oldValue, newValue) => map.HeadingPropertyChanged(newValue),
                (map, value) => map.CoerceHeadingProperty(value));

        public static readonly DependencyProperty TargetHeadingProperty =
            DependencyPropertyHelper.Register<MapBase, double>(nameof(TargetHeading), 0d, true,
                (map, oldValue, newValue) => map.TargetHeadingPropertyChanged(newValue),
                (map, value) => map.CoerceHeadingProperty(value));

        private static readonly DependencyPropertyKey ViewScalePropertyKey =
            DependencyPropertyHelper.RegisterReadOnly<MapBase, double>(nameof(ViewScale), 0d);

        public static readonly DependencyProperty ViewScaleProperty = ViewScalePropertyKey.DependencyProperty;

        private LocationAnimation centerAnimation;
        private DoubleAnimation zoomLevelAnimation;
        private DoubleAnimation headingAnimation;

        static MapBase()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(MapBase), new FrameworkPropertyMetadata(true));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapBase), new FrameworkPropertyMetadata(typeof(MapBase)));
        }

        /// <summary>
        /// Gets or sets the EasingFunction of the Center, ZoomLevel and Heading animations.
        /// The default value is a QuadraticEase with EasingMode.EaseOut.
        /// </summary>
        public IEasingFunction AnimationEasingFunction
        {
            get => (IEasingFunction)GetValue(AnimationEasingFunctionProperty);
            set => SetValue(AnimationEasingFunctionProperty, value);
        }

        /// <summary>
        /// Gets the scaling factor from projected map coordinates to view coordinates,
        /// as pixels per meter.
        /// </summary>
        public double ViewScale => (double)GetValue(ViewScaleProperty);

        /// <summary>
        /// Gets a transform Matrix for scaling and rotating objects that are anchored
        /// at a Location from map coordinates (i.e. meters) to view coordinates.
        /// </summary>
        public Matrix GetMapTransform(Location location)
        {
            var scale = GetScale(location);

            var transform = new Matrix(scale.X, 0d, 0d, scale.Y, 0d, 0d);
            transform.Rotate(ViewTransform.Rotation);

            return transform;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            ResetTransformCenter();
            UpdateTransform();
        }

        private void SetViewScale(double scale)
        {
            SetValue(ViewScalePropertyKey, scale);
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

        private void TargetCenterPropertyChanged(Location targetCenter)
        {
            if (!internalPropertyChange && !targetCenter.Equals(Center))
            {
                ResetTransformCenter();

                if (centerAnimation != null)
                {
                    centerAnimation.Completed -= CenterAnimationCompleted;
                }

                centerAnimation = new LocationAnimation
                {
                    To = new Location(targetCenter.Latitude, CoerceLongitude(targetCenter.Longitude)),
                    Duration = AnimationDuration,
                    EasingFunction = AnimationEasingFunction,
                    FillBehavior = FillBehavior.Stop
                };

                centerAnimation.Completed += CenterAnimationCompleted;

                BeginAnimation(CenterProperty, centerAnimation);
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

                BeginAnimation(CenterProperty, null);
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

        private void TargetZoomLevelPropertyChanged(double targetZoomLevel)
        {
            if (!internalPropertyChange && targetZoomLevel != ZoomLevel)
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
                    FillBehavior = FillBehavior.Stop
                };

                zoomLevelAnimation.Completed += ZoomLevelAnimationCompleted;

                BeginAnimation(ZoomLevelProperty, zoomLevelAnimation);
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

                BeginAnimation(ZoomLevelProperty, null);
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

        private void TargetHeadingPropertyChanged(double targetHeading)
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

                if (headingAnimation != null)
                {
                    headingAnimation.Completed -= HeadingAnimationCompleted;
                }

                headingAnimation = new DoubleAnimation
                {
                    By = delta,
                    Duration = AnimationDuration,
                    EasingFunction = AnimationEasingFunction,
                    FillBehavior = FillBehavior.Stop
                };

                headingAnimation.Completed += HeadingAnimationCompleted;

                BeginAnimation(HeadingProperty, headingAnimation);
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

                BeginAnimation(HeadingProperty, null);
            }
        }
    }
}
