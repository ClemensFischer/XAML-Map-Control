using System;
using System.Collections.Generic;
#if WPF
using System.Windows;
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
using Avalonia;
using Avalonia.Layout;
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
            var minLineDistance = Math.Max(MinLineDistance / ParentMap.ViewTransform.Scale, 1d);
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

                var text = x.ToString("F0");
                labels.Add(new Label(text, p1.X, p1.Y, 0d, HorizontalAlignment.Left, VerticalAlignment.Bottom));
                labels.Add(new Label(text, p2.X, p2.Y, 0d, HorizontalAlignment.Left, VerticalAlignment.Top));
            }

            for (var y = minY; y <= mapRect.Y + mapRect.Height; y += lineDistance)
            {
                var p1 = ParentMap.ViewTransform.MapToView(new Point(mapRect.X, y));
                var p2 = ParentMap.ViewTransform.MapToView(new Point(mapRect.X + mapRect.Width, y));
                figures.Add(CreateLineFigure(p1, p2));

                var text = y.ToString("F0");
                labels.Add(new Label(text, p1.X, p1.Y, 0d, HorizontalAlignment.Left, VerticalAlignment.Bottom));
                labels.Add(new Label(text, p2.X, p2.Y, 0d, HorizontalAlignment.Right, VerticalAlignment.Bottom));
            }
        }
    }
}
