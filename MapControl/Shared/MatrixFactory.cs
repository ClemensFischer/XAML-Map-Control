// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if !WINDOWS_UWP
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    public static class MatrixFactory
    {
        public static Matrix Create(Point translation1, double scaleX, double scaleY, double rotation, Point translation2)
        {
            var matrix = new Matrix(scaleX, 0d, 0d, scaleY, -scaleX * translation1.X, -scaleY * translation1.Y);
            matrix.Rotate(rotation);
            matrix.Translate(translation2.X, translation2.Y);
            return matrix;
        }

        public static Matrix Create(double scale, double rotation, Point translation)
        {
            var matrix = new Matrix(scale, 0d, 0d, scale, 0d, 0d);
            matrix.Rotate(rotation);
            matrix.Translate(translation.X, translation.Y);
            return matrix;
        }
    }
}
