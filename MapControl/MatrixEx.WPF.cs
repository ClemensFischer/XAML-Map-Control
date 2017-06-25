// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Media;

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
            var matrix = new Matrix(1d, 0d, 0d, 1d, -translation1.X, -translation1.Y);
            matrix.Scale(scaleX, scaleY);
            matrix.Rotate(rotationAngle);
            matrix.Translate(translation2.X, translation2.Y);
            return matrix;
        }

        /// <summary>
        /// Used in TileLayer.
        /// </summary>
        public static Matrix TranslateScaleRotateTranslate(
            Point translation1, double scale, double rotationAngle, Point translation2)
        {
            var matrix = new Matrix(1d, 0d, 0d, 1d, -translation1.X, -translation1.Y);
            matrix.Scale(scale, scale);
            matrix.Rotate(rotationAngle);
            matrix.Translate(translation2.X, translation2.Y);
            return matrix;
        }
    }
}
