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

        private double mouseWheelDelta;
        private bool? manipulationEnabled;

        public Map()
        {
            ManipulationMode
                = ManipulationModes.Scale
                | ManipulationModes.TranslateX
                | ManipulationModes.TranslateY
                | ManipulationModes.TranslateInertia;

            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;
            PointerPressed += OnPointerPressed;
            PointerMoved += OnPointerMoved;
            PointerWheelChanged += OnPointerWheelChanged;
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

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // IsLeftButtonPressed: input was triggered by the primary action mode of an input device.
            //
            manipulationEnabled =
                e.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
                e.KeyModifiers == VirtualKeyModifiers.None;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            // Set manipulationEnabled when no PointerPressed was received.
            //
            if (!manipulationEnabled.HasValue &&
                e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                manipulationEnabled = e.KeyModifiers == VirtualKeyModifiers.None;
            }
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                var point = e.GetCurrentPoint(this);

                // Standard mouse wheel delta value is 120.
                //
                mouseWheelDelta += point.Properties.MouseWheelDelta / 120d;

                if (Math.Abs(mouseWheelDelta) >= 1d)
                {
                    // Zoom to integer multiple of MouseWheelZoomDelta.
                    //
                    ZoomMap(point.Position,
                        MouseWheelZoomDelta * Math.Round(TargetZoomLevel / MouseWheelZoomDelta + mouseWheelDelta));

                    mouseWheelDelta = 0d;
                }
            }
        }
    }
}
