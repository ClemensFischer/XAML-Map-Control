using System;
using System.Collections.Generic;
#if WPF
using System.Windows;
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using Avalonia;
using PathFigureCollection = Avalonia.Media.PathFigures;
#endif

namespace MapControl
{
    /// <summary>
    /// Draws a metric grid overlay.
    /// </summary>
    public partial class MetricGrid : MapGrid
    {
        protected override void DrawGrid(PathFigureCollection figures, List<Label> labels)
        {
            var minLineDistance = MinLineDistance / ParentMap.ViewTransform.Scale;
            var lineDistance = Math.Pow(10d, Math.Ceiling(Math.Log10(minLineDistance)));

            if (lineDistance * 0.5 >= minLineDistance)
            {
                lineDistance *= 0.5;

                if (lineDistance * 0.4 >= minLineDistance)
                {
                    lineDistance *= 0.4;
                }
            }

            var mapRect = ParentMap.ViewTransform.ViewToMapBounds(new Rect(0d, 0d, ParentMap.ActualWidth, ParentMap.ActualHeight));
            var minX = Math.Ceiling(mapRect.X / lineDistance) * lineDistance;
            var minY = Math.Ceiling(mapRect.Y / lineDistance) * lineDistance;

            for (var x = minX; x <= mapRect.X + mapRect.Width; x += lineDistance)
            {
                var p1 = ParentMap.ViewTransform.MapToView(new Point(x, mapRect.Y));
                var p2 = ParentMap.ViewTransform.MapToView(new Point(x, mapRect.Y + mapRect.Height));
                figures.Add(CreateLineFigure(p1, p2));
            }

            for (var y = minY; y <= mapRect.Y + mapRect.Height; y += lineDistance)
            {
                var p1 = ParentMap.ViewTransform.MapToView(new Point(mapRect.X, y));
                var p2 = ParentMap.ViewTransform.MapToView(new Point(mapRect.X + mapRect.Width, y));
                figures.Add(CreateLineFigure(p1, p2));

                for (var x = minX; x <= mapRect.X + mapRect.Width; x += lineDistance)
                {
                    AddLabel(labels, x, y);
                }
            }
        }

        private void AddLabel(List<Label> labels, double x, double y)
        {
            var position = ParentMap.ViewTransform.MapToView(new Point(x, y));

            if (ParentMap.InsideViewBounds(position))
            {
                var rotation = ParentMap.ViewTransform.Rotation;

                if (rotation < -90d)
                {
                    rotation += 180d;
                }
                else if (rotation > 90d)
                {
                    rotation -= 180d;
                }

                var text = string.Format("{0:F0}\n{1:F0}", y, x);

                labels.Add(new Label(text, position.X, position.Y, rotation));
            }
        }
    }
}
