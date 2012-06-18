// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Input;

namespace MapControl
{
    public partial class Map
    {
        private double mouseWheelZoom = 1d;
        private Point? mousePosition;

        public double MouseWheelZoom
        {
            get { return mouseWheelZoom; }
            set { mouseWheelZoom = value; }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs eventArgs)
        {
            base.OnMouseWheel(eventArgs);

            ZoomMap(eventArgs.GetPosition(this), TargetZoomLevel + mouseWheelZoom * Math.Sign(eventArgs.Delta));
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs eventArgs)
        {
            base.OnMouseRightButtonDown(eventArgs);

            if (eventArgs.ClickCount == 2)
            {
                ZoomMap(eventArgs.GetPosition(this), Math.Ceiling(ZoomLevel - 1.5));
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs eventArgs)
        {
            base.OnMouseLeftButtonDown(eventArgs);

            if (eventArgs.ClickCount == 1)
            {
                mousePosition = eventArgs.GetPosition(this);
                CaptureMouse();
            }
            else if (eventArgs.ClickCount == 2)
            {
                ZoomMap(eventArgs.GetPosition(this), Math.Floor(ZoomLevel + 1.5));
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs eventArgs)
        {
            base.OnMouseLeftButtonUp(eventArgs);

            if (mousePosition.HasValue)
            {
                mousePosition = null;
                ReleaseMouseCapture();
            }
        }

        protected override void OnMouseMove(MouseEventArgs eventArgs)
        {
            base.OnMouseMove(eventArgs);

            if (mousePosition.HasValue)
            {
                Point position = eventArgs.GetPosition(this);
                TranslateMap(position - mousePosition.Value);
                mousePosition = position;
            }
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs eventArgs)
        {
            base.OnManipulationDelta(eventArgs);

            ManipulationDelta d = eventArgs.DeltaManipulation;
            TransformMap(eventArgs.ManipulationOrigin, d.Translation, d.Rotation, (d.Scale.X + d.Scale.Y) / 2d);
        }
    }
}
