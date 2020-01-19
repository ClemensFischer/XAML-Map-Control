// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MapControl
{
    public partial class Map
    {
        public static readonly DependencyProperty ManipulationModeProperty = DependencyProperty.Register(
            nameof(ManipulationMode), typeof(ManipulationModes), typeof(Map), new PropertyMetadata(ManipulationModes.All));

        public static readonly DependencyProperty TransformDelayProperty = DependencyProperty.Register(
            nameof(TransformDelay), typeof(TimeSpan), typeof(Map), new PropertyMetadata(TimeSpan.FromMilliseconds(50)));

        private Point? mousePosition;

        static Map()
        {
            IsManipulationEnabledProperty.OverrideMetadata(typeof(Map), new FrameworkPropertyMetadata(true));
        }

        public Map()
        {
            ManipulationStarted += OnManipulationStarted;
            ManipulationDelta += OnManipulationDelta;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
            MouseWheel += OnMouseWheel;
        }

        /// <summary>
        /// Gets or sets a value that specifies how the map control handles manipulations.
        /// </summary>
        public ManipulationModes ManipulationMode
        {
            get { return (ManipulationModes)GetValue(ManipulationModeProperty); }
            set { SetValue(ManipulationModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets a delay interval between adjacent calls to TranslateMap or TransformMap during mouse pan and manipulation.
        /// The default value is 50 milliseconds.
        /// </summary>
        public TimeSpan TransformDelay
        {
            get { return (TimeSpan)GetValue(TransformDelayProperty); }
            set { SetValue(TransformDelayProperty, value); }
        }

        private async Task InvokeTransformAsync(Action action)
        {
            if (!transformPending)
            {
                transformPending = true;

                if (TransformDelay > TimeSpan.Zero)
                {
                    await Task.Delay(TransformDelay);
                }

                await Dispatcher.InvokeAsync(action);

                ResetTransform();
            }
        }

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            Manipulation.SetManipulationMode(this, ManipulationMode);
        }

        private async void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            translation.X += e.DeltaManipulation.Translation.X;
            translation.Y += e.DeltaManipulation.Translation.Y;
            rotation += e.DeltaManipulation.Rotation;
            scale *= e.DeltaManipulation.Scale.LengthSquared / 2d;

            await InvokeTransformAsync(() => TransformMap(e.ManipulationOrigin, translation, rotation, scale));
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CaptureMouse())
            {
                mousePosition = e.GetPosition(this);
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mousePosition.HasValue)
            {
                mousePosition = null;
                ReleaseMouseCapture();
            }
        }

        private async void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (mousePosition.HasValue)
            {
                var position = e.GetPosition(this);
                translation += position - mousePosition.Value;
                mousePosition = position;

                await InvokeTransformAsync(() => TranslateMap(translation));
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var zoomDelta = MouseWheelZoomDelta * e.Delta / 120d;

            ZoomMap(e.GetPosition(this), TargetZoomLevel + zoomDelta);
        }
    }
}
