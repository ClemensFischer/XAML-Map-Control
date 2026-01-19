using System;

namespace MapControl
{
    /// <summary>
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395).
    /// </summary>
    public abstract class AzimuthalProjection : MapProjection
    {
        protected AzimuthalProjection()
        {
            Type = MapProjectionType.Azimuthal;
        }

        public double EarthRadius { get; set; } = Wgs84MeanRadius;

        protected (double, double, double) GetPointValues(double latitude, double longitude)
        {
            var phi = latitude * Math.PI / 180d;
            var phi1 = Center.Latitude * Math.PI / 180d;
            var dLambda = (longitude - Center.Longitude) * Math.PI / 180d; // λ - λ0
            var cosPhi = Math.Cos(phi);
            var sinPhi = Math.Sin(phi);
            var cosPhi1 = Math.Cos(phi1);
            var sinPhi1 = Math.Sin(phi1);
            var cosLambda = Math.Cos(dLambda);
            var sinLambda = Math.Sin(dLambda);
            var cosC = sinPhi1 * sinPhi + cosPhi1 * cosPhi * cosLambda; // (5-3)
            var x = cosPhi * sinLambda;
            var y = cosPhi1 * sinPhi - sinPhi1 * cosPhi * cosLambda;

            return (cosC, x, y);
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
