// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using Windows.UI.Xaml.Media;

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
            var matrix = new Matrix(
                scaleX, 0d, 0d, scaleY,
                scaleX * translation1X,
                scaleY * translation1Y);

            if (rotationAngle != 0d)
            {
                rotationAngle = (rotationAngle % 360d) / 180d * Math.PI;

                var cos = Math.Cos(rotationAngle);
                var sin = Math.Sin(rotationAngle);

                matrix = new Matrix(
                    matrix.M11 * cos - matrix.M12 * sin,
                    matrix.M11 * sin + matrix.M12 * cos,
                    matrix.M21 * cos - matrix.M22 * sin,
                    matrix.M21 * sin + matrix.M22 * cos,
                    cos * matrix.OffsetX - sin * matrix.OffsetY,
                    sin * matrix.OffsetX + cos * matrix.OffsetY);
            }

            matrix.OffsetX += translation2X;
            matrix.OffsetY += translation2Y;

            return matrix;
        }

        public static Matrix TranslateScaleRotateTranslate_(
            double translation1X, double translation1Y,
            double scaleX, double scaleY, double rotationAngle,
            double translation2X, double translation2Y)
        {
            var m11 = scaleX;
            var m12 = 0d;
            var m21 = 0d;
            var m22 = scaleY;
            var offsetX = scaleX * translation1X;
            var offsetY = scaleY * translation1Y;

            if (rotationAngle != 0d)
            {
                rotationAngle = (rotationAngle % 360d) / 180d * Math.PI;

                var cos = Math.Cos(rotationAngle);
                var sin = Math.Sin(rotationAngle);

                var _m11 = m11;
                var _m12 = m12;
                var _m21 = m21;
                var _m22 = m22;
                var _offsetX = offsetX;
                var _offsetY = offsetY;

                m11 = _m11 * cos - _m12 * sin;
                m12 = _m11 * sin + _m12 * cos;
                m21 = _m21 * cos - _m22 * sin;
                m22 = _m21 * sin + _m22 * cos;
                offsetX = cos * _offsetX - sin * _offsetY;
                offsetY = sin * _offsetX + cos * _offsetY;
            }

            offsetX += translation2X;
            offsetY += translation2Y;

            return new Matrix(m11, m12, m21, m22, offsetX, offsetY);
        }
    }
}
