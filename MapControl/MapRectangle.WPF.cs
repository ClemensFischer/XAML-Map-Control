// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapRectangle
    {
        static partial void ScaleRect(ref Rect rect, ref Transform transform)
        {
            // Scales the RectangleGeometry to compensate inaccurate hit testing in WPF.
            // See http://stackoverflow.com/a/19335624/1136211

            rect.Scale(1e6, 1e6);

            var scaleTransform = new ScaleTransform(1e-6, 1e-6); // reverts rect scaling
            scaleTransform.Freeze();

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(transform);

            transform = transformGroup;
        }
    }
}