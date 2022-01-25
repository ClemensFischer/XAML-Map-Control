// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
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
            nameof(MouseWheelZoomDelta), typeof(double), typeof(Map), new PropertyMetadata(1d));

        private Point? mousePosition;

        public Map()
        {
            ManipulationMode = ManipulationModes.Scale
                | ManipulationModes.TranslateX
                | ManipulationModes.TranslateY
                | ManipulationModes.TranslateInertia;

            PointerWheelChanged += OnPointerWheelChanged;
            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
            PointerMoved += OnPointerMoved;
            ManipulationDelta += OnManipulationDelta;
        }

        /// <summary>
        /// Gets or sets the amount by which the ZoomLevel property changes during a MouseWheel event.
        /// The default value is 1.
        /// </summary>
        public double MouseWheelZoomDelta
        {
            get { return (double)GetValue(MouseWheelZoomDeltaProperty); }
            set { SetValue(MouseWheelZoomDeltaProperty, value); }
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                var point = e.GetCurrentPoint(this);
                var zoomLevel = TargetZoomLevel + MouseWheelZoomDelta * Math.Sign(point.Properties.MouseWheelDelta);

                ZoomMap(point.Position, MouseWheelZoomDelta * Math.Round(zoomLevel / MouseWheelZoomDelta));
            }
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
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse &&
                mousePosition.HasValue)
            {
                mousePosition = null;
                ReleasePointerCaptures();
            }
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            // Perform translation by explicit Mouse input because with Manipulation pointer capture is
            // lost when Map content changes, e.g. when a MapTileLayer or WmsImageLayer loads new images.

            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse &&
                mousePosition.HasValue)
            {
                Point position = e.GetCurrentPoint(this).Position;
                var translation = position - mousePosition.Value;
                mousePosition = position;

                TranslateMap(translation);
            }
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (!mousePosition.HasValue)
            {
                TransformMap(e.Position, e.Delta.Translation, e.Delta.Rotation, e.Delta.Scale);
            }
        }
    }
}
