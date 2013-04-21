// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
#endif

namespace MapControl
{
    public partial class MapBase
    {
        // Set FillBehavior.HoldEnd to prevent animation from returning
        // to local value before invoking the Completed handler
        private const FillBehavior AnimationFillBehavior = FillBehavior.HoldEnd;

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(MapBase), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        partial void Initialize()
        {
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
