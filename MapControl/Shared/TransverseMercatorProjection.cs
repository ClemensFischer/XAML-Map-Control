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
        private readonly double[] alpha = new double[3]; // α_j
        private readonly double[] beta = new double[3];  // β_j
        private readonly double[] delta = new double[3]; // δ_j
        private double f1; // A / a
        private double f2; // 2 * sqrt(n) / (1+n)

        public double Flattening
        {
            get;
            set
            {
                field = value;
                // n, n^2, n^3
                var n = field / (2d - field);
                var n2 = n * n;
                var n3 = n * n2;
                // A / a
                f1 = (1d + n2 / 4d + n2 * n2 / 64d) / (1d + n);
                // 2 * sqrt(n) / (1+n)
                f2 = 2d * Math.Sqrt(n) / (1d + n);
                // α_j
                alpha[0] = n / 2d - n2 * 2d / 3d + n3 * 5d / 16d;
                alpha[1] = n2 * 13d / 48d - n3 * 3d / 5d;
                alpha[2] = n3 * 61d / 240d;
                // β_j
                beta[0] = n / 2d - n2 * 2d / 3d + n3 * 37d / 96d;
                beta[1] = n2 / 48d + n3 / 15d;
                beta[2] = n3 * 17d / 480d;
                // δ_j
                delta[0] = n * 2d - n2 * 2d / 3d - n3 * 2d;
                delta[1] = n2 * 7d / 3d - n3 * 8d / 5d;
                delta[2] = n3 * 56d / 15d;
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
            return new Point(ScaleFactor, ScaleFactor);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
#if NETFRAMEWORK
            static double Atanh(double x) => Math.Log((1d + x) / (1d - x)) / 2d;
#else
            static double Atanh(double x) => Math.Atanh(x);
#endif
            // k_0 * A
            var k0A = ScaleFactor * EquatorialRadius * f1;
            // φ
            var phi = latitude * Math.PI / 180d;
            var sinPhi = Math.Sin(phi);
            // t
            var t = Math.Sinh(Atanh(sinPhi) - f2 * Atanh(f2 * sinPhi));
            // λ - λ0
            var lambda = (longitude - CentralMeridian) * Math.PI / 180d;
            // ξ'
            var xi_ = Math.Atan(t / Math.Cos(lambda));
            // η'
            var eta_ = Atanh(Math.Sin(lambda) / Math.Sqrt(1d + t * t));
            // ξ
            var xi = xi_
                + alpha[0] * Math.Sin(2d * xi_) * Math.Cosh(2d * eta_)
                + alpha[1] * Math.Sin(4d * xi_) * Math.Cosh(4d * eta_)
                + alpha[2] * Math.Sin(6d * xi_) * Math.Cosh(6d * eta_);
            // η
            var eta = eta_
                + alpha[0] * Math.Cos(2d * xi_) * Math.Sinh(2d * eta_)
                + alpha[1] * Math.Cos(4d * xi_) * Math.Sinh(4d * eta_)
                + alpha[2] * Math.Cos(6d * xi_) * Math.Sinh(6d * eta_);

            return new Point(
                k0A * eta + FalseEasting,
                k0A * xi + FalseNorthing);
        }

        public override Location MapToLocation(double x, double y)
        {
            // k_0 * A
            var k0A = ScaleFactor * EquatorialRadius * f1;
            // ξ
            var xi = (y - FalseNorthing) / k0A;
            // η
            var eta = (x - FalseEasting) / k0A;
            // ξ'
            var xi_ = xi
                - beta[0] * Math.Sin(2d * xi) * Math.Cosh(2d * eta)
                - beta[1] * Math.Sin(4d * xi) * Math.Cosh(4d * eta)
                - beta[2] * Math.Sin(6d * xi) * Math.Cosh(6d * eta);
            // η'
            var eta_ = eta
                - beta[0] * Math.Cos(2d * xi) * Math.Sinh(2d * eta)
                - beta[1] * Math.Cos(4d * xi) * Math.Sinh(4d * eta)
                - beta[2] * Math.Cos(6d * xi) * Math.Sinh(6d * eta);
            // χ
            var chi = Math.Asin(Math.Sin(xi_) / Math.Cosh(eta_));
            // φ
            var phi = chi
                + delta[0] * Math.Sin(2d * chi)
                + delta[1] * Math.Sin(4d * chi)
                + delta[2] * Math.Sin(6d * chi);
            // λ - λ0
            var lambda = Math.Atan(Math.Sinh(eta_) / Math.Cos(xi_));

            return new Location(
                phi * 180d / Math.PI,
                lambda * 180d / Math.PI + CentralMeridian);
        }
    }
}
