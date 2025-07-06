using System;
#if WPF
using System.Windows;
#elif UWP
using Windows.UI.Xaml;
#elif WINUI
using Microsoft.UI.Xaml;
#endif

namespace MapControl
{
    /// <summary>
    /// MapBase with default input event handling.
    /// </summary>
    public partial class Map : MapBase
    {
        public static readonly DependencyProperty MouseWheelZoomDeltaProperty =
            DependencyPropertyHelper.Register<Map, double>(nameof(MouseWheelZoomDelta), 0.25);

        public static readonly DependencyProperty MouseWheelZoomAnimatedProperty =
            DependencyPropertyHelper.Register<Map, bool>(nameof(MouseWheelZoomAnimated), true);

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
        /// Gets or sets a value that specifies whether zooming by a MouseWheel event is animated.
        /// The default value is true.
        /// </summary>
        public bool MouseWheelZoomAnimated
        {
            get => (bool)GetValue(MouseWheelZoomAnimatedProperty);
            set => SetValue(MouseWheelZoomAnimatedProperty, value);
        }

        private void OnMouseWheel(Point position, double delta)
        {
            var zoomLevel = TargetZoomLevel + MouseWheelZoomDelta * delta;
            var animated = false;

            if (delta <= -1d || delta >= 1d)
            {
                // Zoom to integer multiple of MouseWheelZoomDelta when the event was raised by a
                // mouse wheel or by a large movement on a touch pad or other high resolution device.
                //
                zoomLevel = MouseWheelZoomDelta * Math.Round(zoomLevel / MouseWheelZoomDelta);
                animated = MouseWheelZoomAnimated;
            }

            ZoomMap(position, zoomLevel, animated);
        }
    }
}
