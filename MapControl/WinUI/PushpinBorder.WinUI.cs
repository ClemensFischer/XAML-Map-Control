// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.Foundation;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#endif

namespace MapControl
{
    [ContentProperty(Name = "Child")]
    public partial class PushpinBorder : UserControl
    {
        public static readonly DependencyProperty ArrowSizeProperty = DependencyProperty.Register(
            nameof(ArrowSize), typeof(Size), typeof(PushpinBorder),
            new PropertyMetadata(new Size(10d, 20d), (o, e) => ((PushpinBorder)o).SetBorderMargin()));

        public static readonly DependencyProperty BorderWidthProperty = DependencyProperty.Register(
            nameof(BorderWidth), typeof(double), typeof(PushpinBorder),
            new PropertyMetadata(0d, (o, e) => ((PushpinBorder)o).SetBorderMargin()));

        private readonly Border border = new Border();

        public PushpinBorder()
        {
            var path = new Path
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Stretch = Stretch.None
            };
            
            path.SetBinding(Shape.FillProperty, new Binding
            {
                Path = new PropertyPath(nameof(Background)),
                Source = this
            });

            path.SetBinding(Shape.StrokeProperty, new Binding
            {
                Path = new PropertyPath(nameof(BorderBrush)),
                Source = this
            });

            path.SetBinding(Shape.StrokeThicknessProperty, new Binding
            {
                Path = new PropertyPath(nameof(BorderWidth)),
                Source = this
            });

            border.SetBinding(PaddingProperty, new Binding
            {
                Path = new PropertyPath(nameof(Padding)),
                Source = this
            });

            SetBorderMargin();

            var grid = new Grid();
            grid.Children.Add(path);
            grid.Children.Add(border);

            Content = grid;

            SizeChanged += (s, e) => path.Data = BuildGeometry();
        }

        public UIElement Child
        {
            get { return border.Child; }
            set { border.Child = value; }
        }

        private void SetBorderMargin()
        {
            border.Margin = new Thickness(
                BorderWidth, BorderWidth, BorderWidth, BorderWidth + ArrowSize.Height);
        }
    }
}
