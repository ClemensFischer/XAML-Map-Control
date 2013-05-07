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
        }

        /// <summary>
        /// Gets or sets the amount by which the ZoomLevel property changes during a MouseWheel event.
        /// </summary>
        public double MouseWheelZoomChange { get; set; }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            var zoomChange = MouseWheelZoomChange * (double)e.Delta / 120d;
            ZoomMap(e.GetPosition(this), TargetZoomLevel + zoomChange);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (CaptureMouse())
            {
                mousePosition = e.GetPosition(this);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (mousePosition.HasValue)
            {
                mousePosition = null;
                ReleaseMouseCapture();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mousePosition.HasValue)
            {
                var position = e.GetPosition(this);
                TranslateMap((Point)(position - mousePosition));
                mousePosition = position;
            }
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            TransformMap(e.ManipulationOrigin,
                (Point)e.DeltaManipulation.Translation, e.DeltaManipulation.Rotation,
                (e.DeltaManipulation.Scale.X + e.DeltaManipulation.Scale.Y) / 2d);
        }
    }
}
