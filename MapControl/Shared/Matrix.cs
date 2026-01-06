using System;
#if UWP
using FrameworkMatrix = Windows.UI.Xaml.Media.Matrix;
#elif WINUI
using FrameworkMatrix = Microsoft.UI.Xaml.Media.Matrix;
#elif AVALONIA
using Avalonia;
using FrameworkMatrix = Avalonia.Matrix;
#endif

namespace MapControl
{
    /// <summary>
    /// Replaces Windows.UI.Xaml.Media.Matrix, Microsoft.UI.Xaml.Media.Matrix and Avalonia.Matrix
    /// to expose Translate, Rotate and Invert methods.
    /// </summary>
    public struct Matrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
    {
        public double M11 { get; private set; } = m11;
        public double M12 { get; private set; } = m12;
        public double M21 { get; private set; } = m21;
        public double M22 { get; private set; } = m22;
        public double OffsetX { get; private set; } = offsetX;
        public double OffsetY { get; private set; } = offsetY;

        public static implicit operator Matrix(FrameworkMatrix m)
        {
#if AVALONIA
            return new Matrix(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32);
#else
            return new Matrix(m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY);
#endif
        }

        public static implicit operator FrameworkMatrix(Matrix m)
        {
            return new FrameworkMatrix(m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY);
        }

        public readonly Point Transform(Point p)
        {
            return new Point(
                M11 * p.X + M21 * p.Y + OffsetX,
                M12 * p.X + M22 * p.Y + OffsetY);
        }

        public void Translate(double x, double y)
        {
            OffsetX += x;
            OffsetY += y;
        }

        public void Rotate(double angle)
        {
            if (angle != 0d)
            {
                var cos = Math.Cos(angle * Math.PI / 180d);
                var sin = Math.Sin(angle * Math.PI / 180d);

                // equivalent to Multiply(new Matrix(cos, sin, -sin, cos, 0d, 0d));
                //
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

        public void Multiply(Matrix m)
        {
            SetMatrix(
                M11 * m.M11 + M12 * m.M21,
                M11 * m.M12 + M12 * m.M22,
                M21 * m.M11 + M22 * m.M21,
                M21 * m.M12 + M22 * m.M22,
                OffsetX * m.M11 + OffsetY * m.M21 + m.OffsetX,
                OffsetX * m.M12 + OffsetY * m.M22 + m.OffsetY);
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
