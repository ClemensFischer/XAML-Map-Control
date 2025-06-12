using Windows.System;
#if UWP
using Windows.Devices.Input;
using Windows.UI.Xaml.Input;
#else
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;
#endif

namespace MapControl
{
    public partial class Map
    {
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

        private bool? manipulationEnabled;

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                var point = e.GetCurrentPoint(this);

                OnMouseWheel(point.Position, point.Properties.MouseWheelDelta);
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
