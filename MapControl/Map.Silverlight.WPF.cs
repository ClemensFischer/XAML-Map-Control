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
        partial void Initialize()
        {
#if !SILVERLIGHT
            ManipulationDelta += OnManipulationDelta;
#endif
            MouseWheel += OnMouseWheel;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
        }

#if !SILVERLIGHT
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            var d = e.DeltaManipulation;
            TransformMap(e.ManipulationOrigin, (Point)d.Translation, d.Rotation, (d.Scale.X + d.Scale.Y) / 2d);
        }
#endif

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomMap(e.GetPosition(this), TargetZoomLevel + mouseWheelZoom * Math.Sign(e.Delta));
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
