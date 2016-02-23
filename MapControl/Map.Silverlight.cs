// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Input;

namespace MapControl
{
    /// <summary>
    /// MapBase with default input event handling.
    /// </summary>
    public class Map : MapBase
    {
        public static readonly DependencyProperty MouseWheelZoomDeltaProperty = DependencyProperty.Register(
            "MouseWheelZoomDelta", typeof(double), typeof(Map), new PropertyMetadata(1d));

        private Point? mousePosition;

        public Map()
        {
            MouseWheel += OnMouseWheel;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
        }

        /// <summary>
        /// Gets or sets the amount by which the ZoomLevel property changes during a MouseWheel event.
        /// </summary>
        public double MouseWheelZoomDelta
        {
            get { return (double)GetValue(MouseWheelZoomDeltaProperty); }
            set { SetValue(MouseWheelZoomDeltaProperty, value); }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var zoomChange = MouseWheelZoomDelta * (double)e.Delta / 120d;
            ZoomMap(e.GetPosition(this), TargetZoomLevel + zoomChange);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CaptureMouse())
            {
                mousePosition = e.GetPosition(this);
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mousePosition.HasValue)
            {
                mousePosition = null;
                ReleaseMouseCapture();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (mousePosition.HasValue)
            {
                var position = e.GetPosition(this);
                TranslateMap(new Point(position.X - mousePosition.Value.X, position.Y - mousePosition.Value.Y));
                mousePosition = position;
            }
        }
    }
}
