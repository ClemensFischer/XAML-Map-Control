// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    public partial class MapBase
    {
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(MapBase), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        partial void Initialize()
        {
            Background = new SolidColorBrush(Colors.Transparent);
            Clip = new RectangleGeometry();
            Children.Add(tileContainer);

            SizeChanged += OnRenderSizeChanged;
        }

        private void OnRenderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((RectangleGeometry)Clip).Rect = new Rect(0d, 0d, RenderSize.Width, RenderSize.Height);
            ResetTransformOrigin();
            UpdateTransform();
        }

        private void SetTransformMatrixes(double scale)
        {
            scaleTransform.Matrix = new Matrix(scale, 0d, 0d, scale, 0d, 0d);
            rotateTransform.Matrix = Matrix.Identity.Rotate(Heading);
            scaleRotateTransform.Matrix = scaleTransform.Matrix.Multiply(rotateTransform.Matrix);
        }
    }
}
