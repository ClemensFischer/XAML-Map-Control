// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Input;

namespace MapControl
{
    /// <summary>
    /// Default input event handling.
    /// </summary>
    public class Map : MapBase
    {
        private Point? mousePosition;

        public Map()
        {
            MouseWheelZoomChange = 1d;
            MouseWheel += OnMouseWheel;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
        }

        /// <summary>
        /// Gets or sets the amount by which the ZoomLevel property changes during a MouseWheel event.
        /// </summary>
        public double MouseWheelZoomChange { get; set; }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var zoomChange = MouseWheelZoomChange * (double)e.Delta / 120d;
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
