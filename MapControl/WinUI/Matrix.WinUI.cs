// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINUI
using XamlMedia = Microsoft.UI.Xaml.Media;
#else
using XamlMedia = Windows.UI.Xaml.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Replaces Windows.UI.Xaml.Media.Matrix to achieve necessary floating point precision.
    /// </summary>
    public struct Matrix
    {
        public double M11 { get; set; }
        public double M12 { get; set; }
        public double M21 { get; set; }
        public double M22 { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }

        public Matrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }

        public static implicit operator XamlMedia.Matrix(Matrix m)
        {
            return new XamlMedia.Matrix(m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY);
        }

        public Point Transform(Point p)
        {
            return new Point(M11 * p.X + M12 * p.Y + OffsetX, M21 * p.X + M22 * p.Y + OffsetY);
        }

        public void Translate(double x, double y)
        {
            OffsetX += x;
            OffsetY += y;
        }

        public void Rotate(double angle)
        {
            angle = (angle % 360d) / 180d * Math.PI;

            if (angle != 0d)
            {
                var cos = Math.Cos(angle);
                var sin = Math.Sin(angle);

                SetMatrix(
                    cos * M11 - sin * M12,
                    sin * M11 + cos * M12,
                    cos * M21 - sin * M22,
                    sin * M21 + cos * M22,
                    cos * OffsetX - sin * OffsetY,
                    sin * OffsetX + cos * OffsetY);
            }
        }

        public void Invert()
        {
            var invDet = 1d / (M11 * M22 - M12 * M21);

            if (double.IsInfinity(invDet))
            {
                throw new InvalidOperationException("Matrix is not invertible.");
            }

            SetMatrix(
                invDet * M22, invDet * -M12, invDet * -M21, invDet * M11,
                invDet * (M21 * OffsetY - M22 * OffsetX),
                invDet * (M12 * OffsetX - M11 * OffsetY));
        }

        private void SetMatrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }
}
