using System;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    /// <summary>
    /// Universal Transverse Mercator Projection.
    /// See https://en.wikipedia.org/wiki/Universal_Transverse_Mercator_coordinate_system.
    /// </summary>
    public class TransverseMercatorProjection : MapProjection
    {
        public double EquatorialRadius { get; set; } = Wgs84EquatorialRadius;
        public double Flattening { get; set; } = Wgs84Flattening;
        public double ScaleFactor { get; set; } = 0.9996;
        public double CentralMeridian { get; set; }
        public double FalseEasting { get; set; } = 5e5;
        public double FalseNorthing { get; set; }

        public TransverseMercatorProjection()
        {
            Type = MapProjectionType.TransverseCylindrical;
        }

        public override Point GetRelativeScale(double latitude, double longitude)
        {
            return new Point(ScaleFactor, ScaleFactor);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
#if NETFRAMEWORK || UWP
            static double Atanh(double x) => Math.Log((1d + x) / (1d - x)) / 2d;
#else
            static double Atanh(double x) => Math.Atanh(x);
#endif
            var n = Flattening / (2d - Flattening);
            var n2 = n * n;
            var n3 = n * n2;
            var k0A = ScaleFactor * EquatorialRadius / (1d + n) * (1d + n2 / 4d + n2 * n2 / 64d);

            // α_j
            var alpha1 = n / 2d - n2 * 2d / 3d + n3 * 5d / 16d;
            var alpha2 = n2 * 13d / 48d - n3 * 3d / 5d;
            var alpha3 = n3 * 61d / 240d;

            // φ
            var phi = latitude * Math.PI / 180d;

            // (λ - λ0)
            var lambda = (longitude - CentralMeridian) * Math.PI / 180d;

            var s = 2d * Math.Sqrt(n) / (1d + n);
            var sinPhi = Math.Sin(phi);
            var t = Math.Sinh(Atanh(sinPhi) - s * Atanh(s * sinPhi));

            // ξ'
            var xi_ = Math.Atan(t / Math.Cos(lambda));

            // η'
            var eta_ = Atanh(Math.Sin(lambda) / Math.Sqrt(1d + t * t));

            // ξ
            var xi = xi_
                + alpha1 * Math.Sin(2d * xi_) * Math.Cosh(2d * eta_)
                + alpha2 * Math.Sin(4d * xi_) * Math.Cosh(4d * eta_)
                + alpha3 * Math.Sin(6d * xi_) * Math.Cosh(6d * eta_);

            // η
            var eta = eta_
                + alpha1 * Math.Cos(2d * xi_) * Math.Sinh(2d * eta_)
                + alpha2 * Math.Cos(4d * xi_) * Math.Sinh(4d * eta_)
                + alpha3 * Math.Cos(6d * xi_) * Math.Sinh(6d * eta_);

            return new Point(
                k0A * eta + FalseEasting,
                k0A * xi + FalseNorthing);
        }

        public override Location MapToLocation(double x, double y)
        {
            var n = Flattening / (2d - Flattening);
            var n2 = n * n;
            var n3 = n * n2;
            var k0A = ScaleFactor * EquatorialRadius / (1d + n) * (1d + n2 / 4d + n2 * n2 / 64d);

            // β_j
            var beta1 = n / 2d - n2 * 2d / 3d + n3 * 37d / 96d;
            var beta2 = n2 / 48d + n3 / 15d;
            var beta3 = n3 * 17d / 480d;

            // δ_j
            var delta1 = n * 2d - n2 * 2d / 3d - n3 * 2d;
            var delta2 = n2 * 7d / 3d - n3 * 8d / 5d;
            var delta3 = n3 * 56d / 15d;

            // ξ
            var xi = (y - FalseNorthing) / k0A;

            // η
            var eta = (x - FalseEasting) / k0A;

            // ξ'
            var xi_ = xi
                - beta1 * Math.Sin(2d * xi) * Math.Cosh(2d * eta)
                - beta2 * Math.Sin(4d * xi) * Math.Cosh(4d * eta)
                - beta3 * Math.Sin(6d * xi) * Math.Cosh(6d * eta);

            // η'
            var eta_ = eta
                - beta1 * Math.Cos(2d * xi) * Math.Sinh(2d * eta)
                - beta2 * Math.Cos(4d * xi) * Math.Sinh(4d * eta)
                - beta3 * Math.Cos(6d * xi) * Math.Sinh(6d * eta);

            // χ
            var chi = Math.Asin(Math.Sin(xi_) / Math.Cosh(eta_));

            // φ
            var phi = chi
                + delta1 * Math.Sin(2d * chi)
                + delta2 * Math.Sin(4d * chi)
                + delta3 * Math.Sin(6d * chi);

            // λ
            var lambda = Math.Atan(Math.Sinh(eta_) / Math.Cos(xi_));

            return new Location(
                phi * 180d / Math.PI,
                lambda * 180d / Math.PI + CentralMeridian);
        }
    }
}
