using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapPolypoint : MapPath
    {
        protected void UpdateData(IEnumerable<Location> locations, bool closed)
        {
            using var context = ((StreamGeometry)Data).Open();

            if (ParentMap != null && locations != null)
            {
                var longitudeOffset = GetLongitudeOffset(Location ?? locations.FirstOrDefault());

                AddPolylinePoints(context, locations, longitudeOffset, closed);
            }
        }

        protected void UpdateData(IEnumerable<IEnumerable<Location>> polygons)
        {
            using var context = ((StreamGeometry)Data).Open();

            if (ParentMap != null && polygons != null)
            {
                var longitudeOffset = GetLongitudeOffset(Location);

                foreach (var locations in polygons)
                {
                    AddPolylinePoints(context, locations, longitudeOffset, true);
                }
            }
        }

        private void AddPolylinePoints(StreamGeometryContext context, IEnumerable<Location> locations, double longitudeOffset, bool closed)
        {
            var points = locations
                .Select(location => LocationToView(location, longitudeOffset))
                .Where(point => point.HasValue)
                .Select(point => point.Value);

            if (points.Any())
            {
                var start = points.First();
                var polyline = points.Skip(1).ToList();
                var minX = start.X;
                var maxX = start.X;
                var minY = start.Y;
                var maxY = start.Y;

                foreach (var point in polyline)
                {
                    minX = Math.Min(minX, point.X);
                    maxX = Math.Max(maxX, point.X);
                    minY = Math.Min(minY, point.Y);
                    maxY = Math.Max(maxY, point.Y);
                }

                if (maxX >= 0d && minX <= ParentMap.ActualWidth &&
                    maxY >= 0d && minY <= ParentMap.ActualHeight)
                {
                    context.BeginFigure(start, true, closed);
                    context.PolyLineTo(polyline, true, true);
                }
            }
        }
    }
}
