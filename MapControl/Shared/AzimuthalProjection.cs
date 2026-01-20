using System;

namespace MapControl
{
    /// <summary>
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.141.
    /// </summary>
    public abstract class AzimuthalProjection : MapProjection
    {
        protected AzimuthalProjection()
            : base(true)
        {
            Type = MapProjectionType.Azimuthal;
        }

        public double EarthRadius { get; set; } = Wgs84MeanRadius;

        public readonly struct ProjectedPoint
        {
            public double X { get; }
            public double Y { get; }
            public double CosC { get; }

            public ProjectedPoint(double centerLatitude, double centerLongitude, double latitude, double longitude)
            {
                var phi = latitude * Math.PI / 180d;
                var phi1 = centerLatitude * Math.PI / 180d;
                var dLambda = (longitude - centerLongitude) * Math.PI / 180d; // λ - λ0
                var cosPhi = Math.Cos(phi);
                var sinPhi = Math.Sin(phi);
                var cosPhi1 = Math.Cos(phi1);
                var sinPhi1 = Math.Sin(phi1);
                var cosLambda = Math.Cos(dLambda);
                var sinLambda = Math.Sin(dLambda);

                X = cosPhi * sinLambda;
                Y = cosPhi1 * sinPhi - sinPhi1 * cosPhi * cosLambda;
                CosC = sinPhi1 * sinPhi + cosPhi1 * cosPhi * cosLambda; // (5-3)
            }
        }

        protected ProjectedPoint GetProjectedPoint(double latitude, double longitude)
        {
            return new ProjectedPoint(Center.Latitude, Center.Longitude, latitude, longitude);
        }

        protected Location GetLocation(double x, double y, double rho, double sinC)
        {
            var cosC = Math.Sqrt(1d - sinC * sinC);
            var phi1 = Center.Latitude * Math.PI / 180d;
            var cosPhi1 = Math.Cos(phi1);
            var sinPhi1 = Math.Sin(phi1);
            var phi = Math.Asin(cosC * sinPhi1 + y * sinC * cosPhi1 / rho); // (20-14)
            var dLambda = Math.Atan2(x * sinC, rho * cosPhi1 * cosC - y * sinPhi1 * sinC); // (20-15), λ - λ0

            return new Location(phi * 180d / Math.PI, dLambda * 180d / Math.PI + Center.Longitude);
        }
    }
}
