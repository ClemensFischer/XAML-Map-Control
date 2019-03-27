// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if WINDOWS_UWP
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace MapControl
{
    public class AutoEquirectangularProjection : MapProjection
    {
        public Location ProjectionCenter { get; private set; } = new Location();

        public AutoEquirectangularProjection()
            : this("AUTO2:42004")
        {
        }

        public AutoEquirectangularProjection(string crsId)
        {
            CrsId = crsId;
            IsNormalCylindrical = true;
        }

        public override Point LocationToPoint(Location location)
        {
            var xScale = MetersPerDegree * Math.Cos(ProjectionCenter.Latitude * Math.PI / 180d);

            return new Point(
                xScale * (location.Longitude - ProjectionCenter.Longitude),
                MetersPerDegree * location.Latitude);
        }

        public override Location PointToLocation(Point point)
        {
            var xScale = MetersPerDegree * Math.Cos(ProjectionCenter.Latitude * Math.PI / 180d);

            return new Location(
                point.Y / MetersPerDegree,
                point.X / xScale + ProjectionCenter.Longitude);
        }

        public override void SetViewportTransform(Location projectionCenter, Location mapCenter, Point viewportCenter, double zoomLevel, double heading)
        {
            ProjectionCenter = projectionCenter;

            base.SetViewportTransform(projectionCenter, mapCenter, viewportCenter, zoomLevel, heading);
        }

        public override string WmsQueryParameters(BoundingBox boundingBox)
        {
            if (string.IsNullOrEmpty(CrsId))
            {
                return null;
            }

            var rect = BoundingBoxToRect(boundingBox);
            var width = (int)Math.Round(ViewportScale * rect.Width);
            var height = (int)Math.Round(ViewportScale * rect.Height);

            return string.Format(CultureInfo.InvariantCulture,
                "CRS={0},1,{1},{2}&BBOX={3},{4},{5},{6}&WIDTH={7}&HEIGHT={8}",
                CrsId, ProjectionCenter.Longitude, ProjectionCenter.Latitude,
                rect.X, rect.Y, (rect.X + rect.Width), (rect.Y + rect.Height), width, height);
        }
    }
}
