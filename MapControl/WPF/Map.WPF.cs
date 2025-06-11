using System;
using System.Windows;
using System.Windows.Input;

namespace MapControl
{
    /// <summary>
    /// MapBase with default input event handling.
    /// </summary>
    public class Map : MapBase
    {
        public static readonly DependencyProperty ManipulationModeProperty =
            DependencyPropertyHelper.Register<Map, ManipulationModes>(nameof(ManipulationMode), ManipulationModes.Translate | ManipulationModes.Scale);

        public static readonly DependencyProperty MouseWheelZoomDeltaProperty =
            DependencyPropertyHelper.Register<Map, double>(nameof(MouseWheelZoomDelta), 0.25);

        public static readonly DependencyProperty MouseWheelZoomAnimatedProperty =
            DependencyPropertyHelper.Register<Map, bool>(nameof(MouseWheelZoomAnimated), true);

        private Point? mousePosition;

        static Map()
        {
            IsManipulationEnabledProperty.OverrideMetadata(typeof(Map), new FrameworkPropertyMetadata(true));
        }

        /// <summary>
        /// Gets or sets a value that specifies how the map control handles manipulations.
        /// </summary>
        public ManipulationModes ManipulationMode
        {
            get => (ManipulationModes)GetValue(ManipulationModeProperty);
            set => SetValue(ManipulationModeProperty, value);
        }

        /// <summary>
        /// Gets or sets the amount by which the ZoomLevel property changes by a MouseWheel event.
        /// The default value is 0.25.
        /// </summary>
        public double MouseWheelZoomDelta
        {
            get => (double)GetValue(MouseWheelZoomDeltaProperty);
            set => SetValue(MouseWheelZoomDeltaProperty, value);
        }

        /// <summary>
        /// Gets or sets a value that controls whether zooming by a MouseWheel event is animated.
        /// The default value is true.
        /// </summary>
        public bool MouseWheelZoomAnimated
        {
            get => (bool)GetValue(MouseWheelZoomAnimatedProperty);
            set => SetValue(MouseWheelZoomAnimatedProperty, value);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            var zoomLevel = TargetZoomLevel + MouseWheelZoomDelta * e.Delta / 120d;
            var animated = false;

            if (e.Delta % 120 == 0)
            {
                // Zoom to integer multiple of MouseWheelZoomDelta when delta is a multiple of 120,
                // i.e. when the event was actually raised by a mouse wheel and not by a touch pad
                // or a similar device with higher resolution.
                //
                zoomLevel = MouseWheelZoomDelta * Math.Round(zoomLevel / MouseWheelZoomDelta);
                animated = MouseWheelZoomAnimated;
            }

            ZoomMap(e.GetPosition(this), zoomLevel, animated);

            base.OnMouseWheel(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None &&
                CaptureMouse())
            {
                mousePosition = e.GetPosition(this);
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (mousePosition.HasValue)
            {
                mousePosition = null;
                ReleaseMouseCapture();
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mousePosition.HasValue)
            {
                var p = e.GetPosition(this);
                TranslateMap(new Point(p.X - mousePosition.Value.X, p.Y - mousePosition.Value.Y));
                mousePosition = p;
            }
            else if (e.LeftButton == MouseButtonState.Pressed &&
                Keyboard.Modifiers == ModifierKeys.None &&
                CaptureMouse())
            {
                // Set mousePosition when no MouseLeftButtonDown event was received.
                //
                mousePosition = e.GetPosition(this);
            }

            base.OnMouseMove(e);
        }

        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            Manipulation.SetManipulationMode(this, ManipulationMode);

            base.OnManipulationStarted(e);
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            TransformMap(e.ManipulationOrigin,
                (Point)e.DeltaManipulation.Translation,
                e.DeltaManipulation.Rotation,
                e.DeltaManipulation.Scale.LengthSquared / 2d);

            base.OnManipulationDelta(e);
        }
    }
}
