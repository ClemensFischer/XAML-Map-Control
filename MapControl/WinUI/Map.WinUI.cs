// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

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

        private bool mouseMoveEnabled;
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
            if (mouseMoveEnabled || e.Delta.Rotation == 0d && e.Delta.Scale == 1d)
            {
                MoveMap(e.Position);
            }
            else if (e.PointerDeviceType != PointerDeviceType.Mouse)
            {
                TransformMap(e.Position, e.Delta.Translation, e.Delta.Rotation, e.Delta.Scale);
            }
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);

            mouseMoveEnabled = e.Pointer.PointerDeviceType == PointerDeviceType.Mouse &&
                               e.KeyModifiers == VirtualKeyModifiers.None &&
                               point.Properties.IsLeftButtonPressed;

            if (mouseMoveEnabled || e.Pointer.PointerDeviceType != PointerDeviceType.Mouse)
            {
                SetTransformCenter(point.Position);
            }
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            mouseMoveEnabled = false;
            EndMoveMap();
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
