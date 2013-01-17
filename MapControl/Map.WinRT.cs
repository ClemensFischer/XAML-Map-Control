// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Input;

namespace MapControl
{
    public partial class Map
    {
        private Point? mousePosition;

        public Map()
        {
            MouseWheelZoomChange = 1d;
            ManipulationMode = ManipulationModes.All;
            ManipulationDelta += OnManipulationDelta;
            PointerWheelChanged += OnPointerWheelChanged;
            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
            PointerCanceled += OnPointerReleased;
            PointerCaptureLost += OnPointerReleased;
            PointerMoved += OnPointerMoved;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Mouse)
            {
                TransformMap(e.Position, e.Delta.Translation, e.Delta.Rotation, e.Delta.Scale);
            }
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            ZoomMap(point.Position, TargetZoomLevel + MouseWheelZoomChange * Math.Sign(point.Properties.MouseWheelDelta));
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse &&
                CapturePointer(e.Pointer))
            {
                mousePosition = e.GetCurrentPoint(this).Position;
            }
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                mousePosition = null;
                ReleasePointerCapture(e.Pointer);
            }
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (mousePosition.HasValue)
            {
                var position = e.GetCurrentPoint(this).Position;
                TranslateMap(new Point(position.X - mousePosition.Value.X, position.Y - mousePosition.Value.Y));
                mousePosition = position;
            }
        }
    }
}
