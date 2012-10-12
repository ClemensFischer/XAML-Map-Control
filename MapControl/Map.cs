// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Input;

namespace MapControl
{
    /// <summary>
    /// MapBase with input event handling.
    /// </summary>
    public class Map : MapBase
    {
        private double mouseWheelZoom = 1d;
        private Point? mousePosition;

        public double MouseWheelZoom
        {
            get { return mouseWheelZoom; }
            set { mouseWheelZoom = value; }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            ZoomMap(e.GetPosition(this), TargetZoomLevel + mouseWheelZoom * Math.Sign(e.Delta));
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            if (e.ClickCount == 2)
            {
                ZoomMap(e.GetPosition(this), Math.Ceiling(ZoomLevel - 1.5));
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (e.ClickCount == 1)
            {
                mousePosition = e.GetPosition(this);
                CaptureMouse();
            }
            else if (e.ClickCount == 2)
            {
                ZoomMap(e.GetPosition(this), Math.Floor(ZoomLevel + 1.5));
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
                Point position = e.GetPosition(this);
                TranslateMap(position - mousePosition.Value);
                mousePosition = position;
            }
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            ManipulationDelta d = e.DeltaManipulation;
            TransformMap(e.ManipulationOrigin, d.Translation, d.Rotation, (d.Scale.X + d.Scale.Y) / 2d);
        }
    }
}
