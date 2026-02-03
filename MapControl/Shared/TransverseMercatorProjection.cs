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
    /// Transverse Mercator Projection. See
    /// https://en.wikipedia.org/wiki/Transverse_Mercator_projection,
    /// https://en.wikipedia.org/wiki/Universal_Transverse_Mercator_coordinate_system,
    /// https://en.wikipedia.org/wiki/Transverse_Mercator_projection#Convergence.
    /// </summary>
    public class TransverseMercatorProjection : MapProjection
    {
        private readonly double n;
        private readonly double a1; // α1
        private readonly double a2; // α2
        private readonly double a3; // α3
        private readonly double b1; // β1
        private readonly double b2; // β2
        private readonly double b3; // β3
        private readonly double d1; // δ1
        private readonly double d2; // δ2
        private readonly double d3; // δ3
        private readonly double f1; // A/a
        private readonly double f2; // 2*sqrt(n)/(1+n)

        public TransverseMercatorProjection(
            double equatorialRadius, double flattening,
            double scaleFactor, double centralMeridian,
            double falseEasting, double falseNorthing = 0d)
        {
            EquatorialRadius = equatorialRadius;
            Flattening = flattening;
            ScaleFactor = scaleFactor;
            CentralMeridian = centralMeridian;
            FalseEasting = falseEasting;
            FalseNorthing = falseNorthing;

            n = flattening / (2d - flattening);
            var n2 = n * n;
            var n3 = n * n2;
            a1 = n / 2d - n2 * 2d / 3d + n3 * 5d / 16d;
            a2 = n2 * 13d / 48d - n3 * 3d / 5d;
            a3 = n3 * 61d / 240d;
            b1 = n / 2d - n2 * 2d / 3d + n3 * 37d / 96d;
            b2 = n2 / 48d + n3 / 15d;
            b3 = n3 * 17d / 480d;
            d1 = n * 2d - n2 * 2d / 3d - n3 * 2d;
            d2 = n2 * 7d / 3d - n3 * 8d / 5d;
            d3 = n3 * 56d / 15d;
            f1 = (1d + n2 / 4d + n2 * n2 / 64d) / (1d + n);
            f2 = 2d * Math.Sqrt(n) / (1d + n);
        }

        public TransverseMercatorProjection(
            double equatorialRadius, double flattening,
            double scaleFactor, int utmZone, bool north = true)
            : this(equatorialRadius, flattening,
                  scaleFactor, utmZone * 6d - 183d, 5e5, north ? 0d : 1e7)
        {
        }

        public override double GridConvergence(double latitude, double longitude)
        {
            // φ
            var phi = latitude * Math.PI / 180d;
            // λ - λ0
            var lambda = (longitude - CentralMeridian) * Math.PI / 180d;

            // γ calculation for the sphere is sufficiently accurate
            //
            return Math.Atan(Math.Tan(lambda) * Math.Sin(phi)) * 180d / Math.PI;
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            // φ
            var phi = latitude * Math.PI / 180d;
            var sinPhi = Math.Sin(phi);
            // λ - λ0
            var lambda = (longitude - CentralMeridian) * Math.PI / 180d;
            var cosLambda = Math.Cos(lambda);
            var tanLambda = Math.Tan(lambda);
            // t
            var t = Math.Sinh(Atanh(sinPhi) - f2 * Atanh(f2 * sinPhi));
            var u = Math.Sqrt(1d + t * t);
            // ξ'
            var xi_ = Math.Atan2(t, cosLambda);
            // η'
            var eta_ = Atanh(Math.Sin(lambda) / u);
            // σ
            var sigma = 1 +
                2d * a1 * Math.Cos(2d * xi_) * Math.Cosh(2d * eta_) +
                4d * a2 * Math.Cos(4d * xi_) * Math.Cosh(4d * eta_) +
                6d * a3 * Math.Cos(6d * xi_) * Math.Cosh(6d * eta_);
            // τ
            var tau =
                2d * a1 * Math.Sin(2d * xi_) * Math.Sinh(2d * eta_) +
                4d * a2 * Math.Sin(4d * xi_) * Math.Sinh(4d * eta_) +
                6d * a3 * Math.Sin(6d * xi_) * Math.Sinh(6d * eta_);

            var m = (1d - n) / (1d + n) * Math.Tan(phi);
            var k = ScaleFactor * f1 * Math.Sqrt((1d + m * m) * (sigma * sigma + tau * tau) / (t * t + cosLambda * cosLambda));

            // γ, grid convergence
            var gamma = Math.Atan2(tau * u + sigma * t * tanLambda, sigma * u - tau * t * tanLambda);

            var transform = new Matrix(k, 0d, 0d, k, 0d, 0d);
            transform.Rotate(-gamma * 180d / Math.PI);

            return transform;
        }

        public override Point LocationToMap(double latitude, double longitude)
        {
            // φ
            var phi = latitude * Math.PI / 180d;
            var sinPhi = Math.Sin(phi);
            // t
            var t = Math.Sinh(Atanh(sinPhi) - f2 * Atanh(f2 * sinPhi));
            // λ - λ0
            var lambda = (longitude - CentralMeridian) * Math.PI / 180d;
            // ξ'
            var xi_ = Math.Atan2(t, Math.Cos(lambda));
            // η'
            var eta_ = Atanh(Math.Sin(lambda) / Math.Sqrt(1d + t * t));
            // k0 * A
            var k0A = ScaleFactor * EquatorialRadius * f1;

            var x = FalseEasting + k0A * (eta_ +
                a1 * Math.Cos(2d * xi_) * Math.Sinh(2d * eta_) +
                a2 * Math.Cos(4d * xi_) * Math.Sinh(4d * eta_) +
                a3 * Math.Cos(6d * xi_) * Math.Sinh(6d * eta_));

            var y = FalseNorthing + k0A * (xi_ +
                a1 * Math.Sin(2d * xi_) * Math.Cosh(2d * eta_) +
                a2 * Math.Sin(4d * xi_) * Math.Cosh(4d * eta_) +
                a3 * Math.Sin(6d * xi_) * Math.Cosh(6d * eta_));

            return new Point(x, y);
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
            var xi_ = xi -
                b1 * Math.Sin(2d * xi) * Math.Cosh(2d * eta) -
                b2 * Math.Sin(4d * xi) * Math.Cosh(4d * eta) -
                b3 * Math.Sin(6d * xi) * Math.Cosh(6d * eta);
            // η'
            var eta_ = eta -
                b1 * Math.Cos(2d * xi) * Math.Sinh(2d * eta) -
                b2 * Math.Cos(4d * xi) * Math.Sinh(4d * eta) -
                b3 * Math.Cos(6d * xi) * Math.Sinh(6d * eta);
            // χ
            var chi = Math.Asin(Math.Sin(xi_) / Math.Cosh(eta_));
            // φ
            var phi = chi +
                d1 * Math.Sin(2d * chi) +
                d2 * Math.Sin(4d * chi) +
                d3 * Math.Sin(6d * chi);
            // λ - λ0
            var lambda = Math.Atan2(Math.Sinh(eta_), Math.Cos(xi_));

            return new Location(
                phi * 180d / Math.PI,
                lambda * 180d / Math.PI + CentralMeridian);
        }

#if NETFRAMEWORK
        private static double Atanh(double x) => Math.Log((1d + x) / (1d - x)) / 2d;
#else
        private static double Atanh(double x) => Math.Atanh(x);
#endif
    }
}
