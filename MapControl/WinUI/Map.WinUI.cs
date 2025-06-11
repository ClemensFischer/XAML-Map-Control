using System;
using Windows.System;
#if UWP
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
#else
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
#endif

namespace MapControl
{
    /// <summary>
    /// MapBase with default input event handling.
    /// </summary>
    public class Map : MapBase
    {
        public static readonly DependencyProperty MouseWheelZoomDeltaProperty =
            DependencyPropertyHelper.Register<Map, double>(nameof(MouseWheelZoomDelta), 0.25);

        public static readonly DependencyProperty MouseWheelZoomAnimatedProperty =
            DependencyPropertyHelper.Register<Map, bool>(nameof(MouseWheelZoomAnimated), true);

        private bool? manipulationEnabled;

        public Map()
        {
            ManipulationMode
                = ManipulationModes.Scale
                | ManipulationModes.TranslateX
                | ManipulationModes.TranslateY
                | ManipulationModes.TranslateInertia;

            PointerWheelChanged += OnPointerWheelChanged;
            PointerPressed += OnPointerPressed;
            PointerMoved += OnPointerMoved;
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;
        }

        /// <summary>
        /// Gets or sets the amount by which the ZoomLevel property changes by a PointerWheelChanged event.
        /// The default value is 0.25.
        /// </summary>
        public double MouseWheelZoomDelta
        {
            get => (double)GetValue(MouseWheelZoomDeltaProperty);
            set => SetValue(MouseWheelZoomDeltaProperty, value);
        }

        /// <summary>
        /// Gets or sets a value that controls whether zooming by a PointerWheelChanged event is animated.
        /// The default value is true.
        /// </summary>
        public bool MouseWheelZoomAnimated
        {
            get => (bool)GetValue(MouseWheelZoomAnimatedProperty);
            set => SetValue(MouseWheelZoomAnimatedProperty, value);
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                var point = e.GetCurrentPoint(this);
                var delta = point.Properties.MouseWheelDelta;
                var zoomLevel = TargetZoomLevel + MouseWheelZoomDelta * delta / 120d;
                var animated = false;

                if (delta % 120 == 0)
                {
                    // Zoom to integer multiple of MouseWheelZoomDelta when delta is a multiple of 120,
                    // i.e. when the event was actually raised by a mouse wheel and not by a touch pad
                    // or a similar device with higher resolution.
                    //
                    zoomLevel = MouseWheelZoomDelta * Math.Round(zoomLevel / MouseWheelZoomDelta);
                    animated = MouseWheelZoomAnimated;
                }

                ZoomMap(point.Position, zoomLevel, animated);
            }
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Set manipulationEnabled before ManipulationStarted.
            // IsLeftButtonPressed: input was triggered by the primary action mode of an input device.
            //
            manipulationEnabled =
                e.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
                e.KeyModifiers == VirtualKeyModifiers.None;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            // Set manipulationEnabled before ManipulationStarted when no PointerPressed was received.
            //
            if (!manipulationEnabled.HasValue &&
                e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                manipulationEnabled = e.KeyModifiers == VirtualKeyModifiers.None;
            }
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (manipulationEnabled.HasValue && manipulationEnabled.Value)
            {
                if (e.PointerDeviceType == PointerDeviceType.Mouse)
                {
                    TranslateMap(e.Delta.Translation);
                }
                else
                {
                    TransformMap(e.Position, e.Delta.Translation, e.Delta.Rotation, e.Delta.Scale);
                }
            }
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            manipulationEnabled = null;
        }
    }
}
