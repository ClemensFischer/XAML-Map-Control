// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.Foundation;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    internal static class MatrixEx
    {
        /// <summary>
        /// Used in MapProjection.
        /// </summary>
        public static Matrix TranslateScaleRotateTranslate(
            Point translation1, double scaleX, double scaleY, double rotationAngle, Point translation2)
        {
            return new Matrix(1d, 0d, 0d, 1d, -translation1.X, -translation1.Y)
                .Scale(scaleX, scaleY)
                .Rotate(rotationAngle)
                .Translate(translation2.X, translation2.Y);
        }

        /// <summary>
        /// Used in TileLayer.
        /// </summary>
        public static Matrix TranslateScaleRotateTranslate(
            Point translation1, double scale, double rotationAngle, Point translation2)
        {
            return new Matrix(1d, 0d, 0d, 1d, -translation1.X, -translation1.Y)
                .Scale(scale, scale)
                .Rotate(rotationAngle)
                .Translate(translation2.X, translation2.Y);
        }

        private static Matrix Translate(this Matrix matrix, double offsetX, double offsetY)
        {
            matrix.OffsetX += offsetX;
            matrix.OffsetY += offsetY;
            return matrix;
        }

        private static Matrix Scale(this Matrix matrix, double scaleX, double scaleY)
        {
            return Multiply(matrix, new Matrix(scaleX, 0d, 0d, scaleY, 0d, 0d));
        }

        private static Matrix Rotate(this Matrix matrix, double angle)
        {
            if (angle == 0d)
            {
                return matrix;
            }

            angle = (angle % 360d) / 180d * Math.PI;
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            return Multiply(matrix, new Matrix(cos, sin, -sin, cos, 0d, 0d));
        }

        private static Matrix Invert(this Matrix matrix)
        {
            var determinant = matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21;

            return new Matrix(
                matrix.M22 / determinant,
                -matrix.M12 / determinant,
                -matrix.M21 / determinant,
                matrix.M11 / determinant,
                (matrix.M21 * matrix.OffsetY - matrix.M22 * matrix.OffsetX) / determinant,
                (matrix.M12 * matrix.OffsetX - matrix.M11 * matrix.OffsetY) / determinant);
        }

        private static Matrix Multiply(this Matrix matrix1, Matrix matrix2)
        {
            return new Matrix(
                matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21,
                matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22,
                matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21,
                matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22,
                (matrix2.M11 * matrix1.OffsetX + matrix2.M21 * matrix1.OffsetY) + matrix2.OffsetX,
                (matrix2.M12 * matrix1.OffsetX + matrix2.M22 * matrix1.OffsetY) + matrix2.OffsetY);
        }
    }
}
