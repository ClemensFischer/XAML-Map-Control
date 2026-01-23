using System;
#if WPF
using System.Windows;
using System.Windows.Media;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    /// <summary>
    /// Elliptical Transverse Mercator Projection.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/publication/pp1395), p.60-64.
    /// </summary>
    public class TransverseMercatorProjection : MapProjection
    {
        private double M0;

        public TransverseMercatorProjection()
        {
            Type = MapProjectionType.TransverseCylindrical;
        }

        public double ScaleFactor { get; set; } = 0.9996;
        public double CentralMeridian { get; set; }
        public double FalseEasting { get; set; }
        public double FalseNorthing { get; set; }

        public double EquatorialRadius
        {
            get;
            set
            {
                field = value;
                M0 = MeridianDistance(LatitudeOfOrigin * Math.PI / 180d);
            }
        } = Wgs84EquatorialRadius;

        public double Flattening
        {
            get;
            set
            {
                field = value;
                M0 = MeridianDistance(LatitudeOfOrigin * Math.PI / 180d);
            }
        } = Wgs84Flattening;

        public double LatitudeOfOrigin
        {
            get;
            set
            {
                field = value;
                M0 = MeridianDistance(value * Math.PI / 180d);
            }
        }

        private double MeridianDistance(double phi)
        {
            var e2 = (2d - Flattening) * Flattening;
            var e4 = e2 * e2;
            var e6 = e2 * e4;

            return EquatorialRadius *
                ((1d - e2 / 4d - 3d * e4 / 64d - 5d * e6 / 256d) * phi -
                (3d * e2 / 8d + 3d * e4 / 32d + 45d * e6 / 1024d) * Math.Sin(2d * phi) +
                (15d * e4 / 256d + 45d * e6 / 1024d) * Math.Sin(4d * phi) -
                35d * e6 / 3072d * Math.Sin(6d * phi)); // (3-21)
        }

        public override Matrix RelativeScale(double latitude, double longitude)
        {
            var k = ScaleFactor;

            if (latitude > -90d && latitude < 90d)
            {
                var phi = latitude * Math.PI / 180d;
                var cosPhi = Math.Cos(phi);
                var tanPhi = Math.Tan(phi);

                var e2 = (2d - Flattening) * Flattening;
                var e_2 = e2 / (1d - e2); // (8-12)
                var T = tanPhi * tanPhi; // (8-13)
                var C = e_2 * cosPhi * cosPhi; // (8-14)
                var A = (longitude - CentralMeridian) * Math.PI / 180d * cosPhi; // (8-15)
                var A2 = A * A;
                var A4 = A2 * A2;
                var A6 = A2 * A4;

                k *= 1d + (1d + C) * A2 / 2d +
                    (5d - 4d * T + 42d * C + 13d * C * C - 28d * e_2) * A4 / 24d +
                    (61d - 148d * T + 16 * T * T) * A6 / 720d; // (8-11)
            }

            return new Matrix(k, 0d, 0d, k, 0d, 0d);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            var phi = latitude * Math.PI / 180d;
            var M = MeridianDistance(phi);
            double x, y;

            if (latitude > -90d && latitude < 90d)
            {
                var sinPhi = Math.Sin(phi);
                var cosPhi = Math.Cos(phi);
                var tanPhi = sinPhi / cosPhi;

                var e2 = (2d - Flattening) * Flattening;
                var e_2 = e2 / (1d - e2); // (8-12)
                var N = EquatorialRadius / Math.Sqrt(1d - e2 * sinPhi * sinPhi); // (4-20)
                var T = tanPhi * tanPhi; // (8-13)
                var C = e_2 * cosPhi * cosPhi; // (8-14)
                var A = (longitude - CentralMeridian) * Math.PI / 180d * cosPhi; // (8-15)
                var A2 = A * A;
                var A3 = A * A2;
                var A4 = A * A3;
                var A5 = A * A4;
                var A6 = A * A5;

                x = ScaleFactor * N *
                    (A + (1d - T + C) * A3 / 6d + (5d - 18d * T + T * T + 72d * C - 58d * e_2) * A5 / 120d); // (8-9)
                y = ScaleFactor * (M - M0 + N * tanPhi * (A2 / 2d + (5d - T + 9d * C + 4d * C * C) * A4 / 24d +
                    (61d - 58d * T + T * T + 600d * C - 330d * e_2) * A6 / 720d)); // (8-10)
            }
            else
            {
                x = 0d;
                y = ScaleFactor * (M - M0);
            }

            return new Point(x + FalseEasting, y + FalseNorthing);
        }

        public override Location MapToLocation(double x, double y)
        {
            var e2 = (2d - Flattening) * Flattening;
            var e4 = e2 * e2;
            var e6 = e2 * e4;
            var s = Math.Sqrt(1d - e2);
            var e1 = (1d - s) / (1d + s); // (3-24)
            var e12 = e1 * e1;
            var e13 = e1 * e12;
            var e14 = e1 * e13;

            var M = M0 + (y - FalseNorthing) / ScaleFactor; // (8-20)
            var mu = M / (EquatorialRadius * (1d - e2 / 4d - 3d * e4 / 64d - 5d * e6 / 256d)); // (7-19)
            var phi1 = mu +
                (3d * e1 / 2d - 27d * e13 / 32d) * Math.Sin(2d * mu) +
                (21d * e12 / 16d - 55d * e14 / 32d) * Math.Sin(4d * mu) +
                151d * e13 / 96d * Math.Sin(6d * mu) +
                1097d * e14 / 512d * Math.Sin(8d * mu); // (3-26)

            var sinPhi1 = Math.Sin(phi1);
            var cosPhi1 = Math.Cos(phi1);
            var tanPhi1 = sinPhi1 / cosPhi1;

            var e_2 = e2 / (1d - e2); // (8-12)
            var C1 = e_2 * cosPhi1 * cosPhi1; // (8-21)
            var T1 = sinPhi1 * sinPhi1 / (cosPhi1 * cosPhi1); // (8-22)
            s = Math.Sqrt(1d - e2 * sinPhi1 * sinPhi1);
            var N1 = EquatorialRadius / s; // (8-23)
            var R1 = EquatorialRadius * (1d - e2) / (s * s * s); // (8-24)
            var D = (x - FalseEasting) / (N1 * ScaleFactor); // (8-25)
            var D2 = D * D;
            var D3 = D * D2;
            var D4 = D * D3;
            var D5 = D * D4;
            var D6 = D * D5;

            var phi = phi1 - N1 * tanPhi1 / R1 * (D2 / 2d - (5d + 3d * T1 + 10d * C1 - 4d * C1 * C1 - 9d * e_2) * D4 / 24d +
                (61d + 90d * T1 + 45d * T1 * T1 + 298 * C1 - 3d * C1 * C1 - 252d * e_2) * D6 / 720d); // (8-17)

            var dLambda = (D - (1d + 2d * T1 + C1) * D3 / 6d +
                (5d - 2d * C1 - 3d * C1 * C1 + 28d * T1 + 24d * T1 * T1 + 8d * e_2) * D5 / 120d) / cosPhi1; // (8-18)

            return new Location(
                phi * 180d / Math.PI,
                dLambda * 180d / Math.PI + CentralMeridian);
        }
    }
}
