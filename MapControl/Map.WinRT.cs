// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace MapControl
{
    /// <summary>
    /// Default input event handling.
    /// </summary>
    public class Map : MapBase
    {
        public static readonly DependencyProperty MouseWheelZoomChangeProperty = DependencyProperty.Register(
            "MouseWheelZoomChange", typeof(double), typeof(Map), new PropertyMetadata(1d));

        private Point? mousePosition;

        public Map()
        {
            ManipulationMode = ManipulationModes.Scale | ManipulationModes.ScaleInertia |
                ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.TranslateInertia;

            ManipulationDelta += OnManipulationDelta;
            PointerWheelChanged += OnPointerWheelChanged;
            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
            PointerCanceled += OnPointerReleased;
            PointerCaptureLost += OnPointerReleased;
            PointerMoved += OnPointerMoved;
        }

        /// <summary>
        /// Gets or sets the amount by which the ZoomLevel property changes during a MouseWheel event.
        /// </summary>
        public double MouseWheelZoomChange
        {
            get { return (double)GetValue(MouseWheelZoomChangeProperty); }
            set { SetValue(MouseWheelZoomChangeProperty, value); }
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            var zoomChange = MouseWheelZoomChange * (double)point.Properties.MouseWheelDelta / 120d;
            ZoomMap(point.Position, TargetZoomLevel + zoomChange);
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

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType != PointerDeviceType.Mouse)
            {
                TransformMap(e.Position, e.Delta.Translation, e.Delta.Rotation, e.Delta.Scale);
            }
        }
    }
}
