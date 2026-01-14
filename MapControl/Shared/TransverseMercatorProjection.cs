using System;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    /// <summary>
    /// Transverse Mercator Projection.
    /// See https://en.wikipedia.org/wiki/Transverse_Mercator_projection
    /// and https://en.wikipedia.org/wiki/Universal_Transverse_Mercator_coordinate_system.
    /// </summary>
    public class TransverseMercatorProjection : MapProjection
    {
        private double a1; // α1
        private double a2; // α2
        private double a3; // α3
        private double b1; // β1
        private double b2; // β2
        private double b3; // β3
        private double d1; // δ1
        private double d2; // δ2
        private double d3; // δ3
        private double f1; // A / a
        private double f2; // 2 * sqrt(n) / (1+n)

        public double Flattening
        {
            get;
            set
            {
                field = value;
                var n = field / (2d - field);
                var nn = n * n;
                var nnn = n * nn;
                a1 = n / 2d - nn * 2d / 3d + nnn * 5d / 16d;
                a2 = nn * 13d / 48d - nnn * 3d / 5d;
                a3 = nnn * 61d / 240d;
                b1 = n / 2d - nn * 2d / 3d + nnn * 37d / 96d;
                b2 = nn / 48d + nnn / 15d;
                b3 = nnn * 17d / 480d;
                d1 = n * 2d - nn * 2d / 3d - nnn * 2d;
                d2 = nn * 7d / 3d - nnn * 8d / 5d;
                d3 = nnn * 56d / 15d;
                f1 = (1d + nn / 4d + nn * nn / 64d) / (1d + n);
                f2 = 2d * Math.Sqrt(n) / (1d + n);
            }
        }

        public double EquatorialRadius { get; set; } = Wgs84EquatorialRadius;
        public double ScaleFactor { get; set; } = 0.9996;
        public double CentralMeridian { get; set; }
        public double FalseEasting { get; set; }
        public double FalseNorthing { get; set; }

        public TransverseMercatorProjection()
        {
            Type = MapProjectionType.TransverseCylindrical;
            Flattening = Wgs84Flattening;
        }

        public override Point RelativeScale(double latitude, double longitude)
        {
            return new Point(ScaleFactor, ScaleFactor); // sufficiently precise
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
#if NETFRAMEWORK
            static double Atanh(double x) => Math.Log((1d + x) / (1d - x)) / 2d;
#else
            static double Atanh(double x) => Math.Atanh(x);
#endif
            // k0 * A
            var k0A = ScaleFactor * EquatorialRadius * f1;
            // φ
            var phi = latitude * Math.PI / 180d;
            var sinPhi = Math.Sin(phi);
            // t
            var t = Math.Sinh(Atanh(sinPhi) - f2 * Atanh(f2 * sinPhi));
            // λ - λ0
            var dLambda = (longitude - CentralMeridian) * Math.PI / 180d;
            // ξ'
            var xi_ = Math.Atan(t / Math.Cos(dLambda));
            // η'
            var eta_ = Atanh(Math.Sin(dLambda) / Math.Sqrt(1d + t * t));
            // ξ
            var xi = xi_
                + a1 * Math.Sin(2d * xi_) * Math.Cosh(2d * eta_)
                + a2 * Math.Sin(4d * xi_) * Math.Cosh(4d * eta_)
                + a3 * Math.Sin(6d * xi_) * Math.Cosh(6d * eta_);
            // η
            var eta = eta_
                + a1 * Math.Cos(2d * xi_) * Math.Sinh(2d * eta_)
                + a2 * Math.Cos(4d * xi_) * Math.Sinh(4d * eta_)
                + a3 * Math.Cos(6d * xi_) * Math.Sinh(6d * eta_);

            return new Point(
                k0A * eta + FalseEasting,
                k0A * xi + FalseNorthing);
        }

        public override Location MapToLocation(double x, double y)
        {
            // k0 * A
            var k0A = ScaleFactor * EquatorialRadius * f1;
            // ξ
            var xi = (y - FalseNorthing) / k0A;
            // η
            var eta = (x - FalseEasting) / k0A;
            // ξ'
            var xi_ = xi
                - b1 * Math.Sin(2d * xi) * Math.Cosh(2d * eta)
                - b2 * Math.Sin(4d * xi) * Math.Cosh(4d * eta)
                - b3 * Math.Sin(6d * xi) * Math.Cosh(6d * eta);
            // η'
            var eta_ = eta
                - b1 * Math.Cos(2d * xi) * Math.Sinh(2d * eta)
                - b2 * Math.Cos(4d * xi) * Math.Sinh(4d * eta)
                - b3 * Math.Cos(6d * xi) * Math.Sinh(6d * eta);
            // χ
            var chi = Math.Asin(Math.Sin(xi_) / Math.Cosh(eta_));
            // φ
            var phi = chi
                + d1 * Math.Sin(2d * chi)
                + d2 * Math.Sin(4d * chi)
                + d3 * Math.Sin(6d * chi);
            // λ - λ0
            var dLambda = Math.Atan(Math.Sinh(eta_) / Math.Cos(xi_));

            return new Location(
                phi * 180d / Math.PI,
                dLambda * 180d / Math.PI + CentralMeridian);
        }
    }
}
