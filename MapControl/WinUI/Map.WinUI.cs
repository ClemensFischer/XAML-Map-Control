// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using Windows.System;
#if WINUI
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
#else
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
#endif

namespace MapControl
{
    /// <summary>
    /// MapBase with default input event handling.
    /// </summary>
    public class Map : MapBase
    {
        public static readonly DependencyProperty MouseWheelZoomDeltaProperty = DependencyProperty.Register(
            nameof(MouseWheelZoomDelta), typeof(double), typeof(Map), new PropertyMetadata(0.25));

        private bool manipulationEnabled;
        private double mouseWheelDelta;

        public Map()
        {
            ManipulationMode
                = ManipulationModes.Scale
                | ManipulationModes.TranslateX
                | ManipulationModes.TranslateY
                | ManipulationModes.TranslateInertia;

            ManipulationDelta += OnManipulationDelta;
            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
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
            if (manipulationEnabled)
            {
                TransformMap(e.Position, e.Delta.Translation, e.Delta.Rotation, e.Delta.Scale);
            }
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            manipulationEnabled = e.Pointer.PointerDeviceType != PointerDeviceType.Mouse ||
                                  e.KeyModifiers == VirtualKeyModifiers.None &&
                                  e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            manipulationEnabled = false;
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

                    ZoomMap(point.Position,
                        MouseWheelZoomDelta * Math.Round(TargetZoomLevel / MouseWheelZoomDelta + mouseWheelDelta));

                    mouseWheelDelta = 0d;
                }
            }
        }
    }
}
