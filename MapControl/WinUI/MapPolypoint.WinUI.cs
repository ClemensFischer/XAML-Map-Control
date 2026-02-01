using System;
using System.Collections.Generic;
using System.Linq;
#if UWP
using Windows.UI.Xaml.Media;
#else
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl
{
    public partial class MapPolypoint : MapPath
    {
        protected void UpdateData(IEnumerable<Location> locations, bool closed)
        {
            var figures = ((PathGeometry)Data).Figures;
            figures.Clear();

            if (ParentMap != null && locations != null)
            {
                var longitudeOffset = GetLongitudeOffset(locations);

                AddPolylinePoints(figures, locations, longitudeOffset, closed);
            }
        }

        protected void UpdateData(IEnumerable<IEnumerable<Location>> polygons)
        {
            var figures = ((PathGeometry)Data).Figures;
            figures.Clear();

            if (ParentMap != null && polygons != null)
            {
                var longitudeOffset = GetLongitudeOffset(polygons.FirstOrDefault());

                foreach (var locations in polygons)
                {
                    AddPolylinePoints(figures, locations, longitudeOffset, true);
                }
            }
        }

        private void AddPolylinePoints(PathFigureCollection figures, IEnumerable<Location> locations, double longitudeOffset, bool closed)
        {
            var points = locations.Select(location => LocationToView(location, longitudeOffset));

            if (points.Any())
            {
                var start = points.First();
                var polyline = new PolyLineSegment();
                var minX = start.X;
                var maxX = start.X;
                var minY = start.Y;
                var maxY = start.Y;

                foreach (var point in points.Skip(1))
                {
                    polyline.Points.Add(point);
                    minX = Math.Min(minX, point.X);
                    maxX = Math.Max(maxX, point.X);
                    minY = Math.Min(minY, point.Y);
                    maxY = Math.Max(maxY, point.Y);
                }

                if (maxX >= 0d && minX <= ParentMap.ActualWidth &&
                    maxY >= 0d && minY <= ParentMap.ActualHeight)
                {
                    var figure = new PathFigure
                    {
                        StartPoint = start,
                        IsClosed = closed,
                        IsFilled = true
                    };

                    figure.Segments.Add(polyline);
                    figures.Add(figure);
                }
            }
        }
    }
}
