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

        private void OnMouseWheel(Point position, int delta)
        {
            // Standard mouse wheel delta value is 120.
            //
            OnMouseWheel(position, delta / 120d);
        }

        private void OnMouseWheel(Point position, double delta)
        {
            var zoomLevel = TargetZoomLevel + MouseWheelZoomDelta * delta;
            var animated = false;

            if (delta % 1d == 0d)
            {
                // Zoom to integer multiple of MouseWheelZoomDelta when delta is an integer value,
                // i.e. when the event was actually raised by a mouse wheel and not by a touch pad
                // or a similar device with higher resolution.
                //
                zoomLevel = MouseWheelZoomDelta * Math.Round(zoomLevel / MouseWheelZoomDelta);
                animated = MouseWheelZoomAnimated;
            }

            ZoomMap(position, zoomLevel, animated);
        }
    }
}
