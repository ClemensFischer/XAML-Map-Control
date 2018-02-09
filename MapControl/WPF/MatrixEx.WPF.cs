// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows.Media;

namespace MapControl
{
    internal static class MatrixEx
    {
        /// <summary>
        /// Used in MapProjection and MapTileLayer.
        /// </summary>
        public static Matrix TranslateScaleRotateTranslate(
            double translation1X, double translation1Y,
            double scaleX, double scaleY, double rotationAngle,
            double translation2X, double translation2Y)
        {
            var matrix = new Matrix(1d, 0d, 0d, 1d, translation1X, translation1Y);
            matrix.Scale(scaleX, scaleY);
            matrix.Rotate(rotationAngle);
            matrix.Translate(translation2X, translation2Y);
            return matrix;
        }
    }
}
