// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Input;

namespace MapControl
{
    public partial class Map
    {
        private Point? mousePosition;

        public Map()
        {
            MouseWheelZoomChange = 1d;
            MouseWheel += OnMouseWheel;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;

#if !SILVERLIGHT
            ManipulationDelta += (o, e) => TransformMap(
                e.ManipulationOrigin, (Point)e.DeltaManipulation.Translation, e.DeltaManipulation.Rotation,
                (e.DeltaManipulation.Scale.X + e.DeltaManipulation.Scale.Y) / 2d); ;
#endif
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomMap(e.GetPosition(this), TargetZoomLevel + MouseWheelZoomChange * Math.Sign(e.Delta));
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
