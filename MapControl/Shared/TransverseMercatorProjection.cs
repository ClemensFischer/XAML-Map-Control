// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WPF
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Universal Transverse Mercator Projection.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.57-64.
    /// </summary>
    public class TransverseMercatorProjection : MapProjection
    {
        public const double DefaultScaleFactor = 0.9996;

        public TransverseMercatorProjection()
        {
            Type = MapProjectionType.TransverseCylindrical;
        }

        public double EquatorialRadius { get; set; } = Wgs84EquatorialRadius;
        public double Flattening { get; set; } = Wgs84Flattening;
        public double ScaleFactor { get; set; } = DefaultScaleFactor;
        public double CentralMeridian { get; set; }
        public double FalseEasting { get; set; }
        public double FalseNorthing { get; set; }

        public override Point GetRelativeScale(Location location)
        {
            var k = ScaleFactor;

            if (location.Latitude > -90d && location.Latitude < 90d)
            {
                var lat = location.Latitude * Math.PI / 180d;
                var lon = (location.Longitude - CentralMeridian) * Math.PI / 180d;
                var cosLat = Math.Cos(lat);
                var tanLat = Math.Tan(lat);

                var e_2 = (2d - Flattening) * Flattening;
                var E_2 = e_2 / (1d - e_2); // p.61 (8-12)
                var T = tanLat * tanLat; // p.61 (8-13)
                var C = E_2 * cosLat * cosLat; // p.61 (8-14)
                var A = lon * cosLat; // p.61 (8-15)

                var T_2 = T * T;
                var C_2 = C * C;
                var A_2 = A * A;
                var A_4 = A_2 * A_2;
                var A_6 = A_2 * A_4;

                k = ScaleFactor * (1d
                    + (1d + C) * A_2 / 2d
                    + (5d - 4d * T + 42d * C + 13d * C_2 + 28d * E_2) * A_4 / 24d
                    + (61d - 148d * T + 16d * T_2) * A_6 / 720d); // p.61 (8-11)
            }

            return new Point(k, k);
        }

        public override Point? LocationToMap(Location location)
        {
            double x, y;

            var lat = location.Latitude * Math.PI / 180d;
            var e_2 = (2d - Flattening) * Flattening;
            var e_4 = e_2 * e_2;
            var e_6 = e_2 * e_4;
            var M = EquatorialRadius * (1d - e_2 / 4d - 3d * e_4 / 64d - 5d * e_6 / 256d) * lat; // p.61 (3-21)

            if (lat > -Math.PI / 2d && lat < Math.PI / 2d)
            {
                var lon = (location.Longitude - CentralMeridian) * Math.PI / 180d;
                var sinLat = Math.Sin(lat);
                var cosLat = Math.Cos(lat);
                var tanLat = Math.Tan(lat);

                var E_2 = e_2 / (1d - e_2); // p.61 (8-12)
                var T = tanLat * tanLat; // p.61 (8-13)
                var C = E_2 * cosLat * cosLat; // p.61 (8-14)
                var A = lon * cosLat; // p.61 (8-15)

                var N = EquatorialRadius / Math.Sqrt(1d - e_2 * sinLat * sinLat); // p.61 (4-20)

                M += EquatorialRadius *
                    (-(3d * e_2 / 8d + 3d * e_4 / 32d + 45d * e_6 / 1024d) * Math.Sin(2d * lat)
                    + (15d * e_4 / 256d + 45d * e_6 / 1024d) * Math.Sin(4d * lat)
                    - (35d * e_6 / 3072d) * Math.Sin(6d * lat)); // p.61 (3-21)

                var T_2 = T * T;
                var C_2 = C * C;
                var A_2 = A * A;
                var A_3 = A * A_2;
                var A_4 = A * A_3;
                var A_5 = A * A_4;
                var A_6 = A * A_5;

                x = ScaleFactor * N * (A
                    + (1d - T + C) * A_3 / 6d
                    + (5d - 18d * T + T_2 + 72d * C - 58d * E_2) * A_5 / 120d); // p.61 (8-9)

                y = ScaleFactor * (M + N * tanLat * (A_2 / 2d
                    + (5d - T + 9d * C + 4d * C_2) * A_4 / 24d
                    + (61d - 58d * T + T_2 + 600d * C - 330d * E_2) * A_6 / 720d)); // p.61 (8-10)
            }
            else
            {
                x = 0d;
                y = ScaleFactor * M;
            }

            return new Point(x + FalseEasting, y + FalseNorthing);
        }

        public override Location MapToLocation(Point point)
        {
            var x = point.X - FalseEasting;
            var y = point.Y - FalseNorthing;

            var e_2 = (2d - Flattening) * Flattening;
            var e_4 = e_2 * e_2;
            var e_6 = e_2 * e_4;

            var M = y / ScaleFactor; // p.63 (8-20)
            var mu = M / (EquatorialRadius * (1d - e_2 / 4d - 3d * e_4 / 64d - 5d * e_6 / 256d)); // p.63 (7-19)
            var e1 = (1d - Math.Sqrt(1d - e_2)) / (1d + Math.Sqrt(1d - e_2)); // p.63 (3-24)
            var e1_2 = e1 * e1;
            var e1_3 = e1 * e1_2;
            var e1_4 = e1 * e1_3;

            var lat1 = mu
                + (3d * e1 / 2d - 27d * e1_3 / 32d) * Math.Sin(2d * mu)
                + (21d * e1_2 / 16d - 55d * e1_4 / 32d) * Math.Sin(4d * mu)
                + (151d * e1_3 / 96d) * Math.Sin(6d * mu)
                + (1097d * e1_4 / 512d) * Math.Sin(8d * mu); // p.63 (3-26)

            var sinLat1 = Math.Sin(lat1);
            var cosLat1 = Math.Cos(lat1);
            var tanLat1 = Math.Tan(lat1);

            var E_2 = e_2 / (1d - e_2); // p.64 (8-12)
            var C1 = E_2 * cosLat1 * cosLat1; // p.64 (8-21)
            var T1 = tanLat1 * tanLat1; // p.64 (8-22)
            var N1 = EquatorialRadius / Math.Sqrt(1d - e_2 * sinLat1 * sinLat1); // p.64 (8-23)
            var R1 = EquatorialRadius * (1d - e_2) / Math.Pow(1d - e_2 * sinLat1 * sinLat1, 1.5); // p.64 (8-24)
            var D = x / (N1 * ScaleFactor); // p.64 (8-25)

            var C1_2 = C1 * C1;
            var T1_2 = T1 * T1;
            var D_2 = D * D;
            var D_3 = D * D_2;
            var D_4 = D * D_3;
            var D_5 = D * D_4;
            var D_6 = D * D_5;

            var lat = lat1 - (N1 * tanLat1 / R1) * (D_2 / 2d
                - (5d + 3d * T1 + 10d * C1 - 4d * C1_2 - 9d * E_2) * D_4 / 24d
                + (61d + 90d * T1 + 298d * C1 + 45d * T1_2 - 252d * E_2 - 3d * C1_2) * D_6 / 720d); // p.63 (8-17)

            var lon = (D
                - (1d + 2d * T1 + C1) * D_3 / 6d
                + (5d - 2d * C1 + 28d * T1 - 3d * C1_2 + 8d * E_2 + 24d * T1_2) * D_5 / 120d)
                / cosLat1; // p.63 (8-18)

            return new Location(lat * 180d / Math.PI, lon * 180d / Math.PI + CentralMeridian);
        }
    }
}
