using Windows.Foundation;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
#endif

namespace MapControl
{
    [ContentProperty(Name = "Child")]
    public partial class PushpinBorder : UserControl
    {
        public static readonly DependencyProperty ArrowSizeProperty =
            DependencyPropertyHelper.Register<PushpinBorder, Size>(nameof(ArrowSize), new Size(10d, 20d),
                (border, oldValue, newValue) => border.SetBorderMargin());

        public static readonly DependencyProperty BorderWidthProperty =
            DependencyPropertyHelper.Register<PushpinBorder, double>(nameof(BorderWidth), 0d,
                (border, oldValue, newValue) => border.SetBorderMargin());

        private readonly Border border = new Border();

        public PushpinBorder()
        {
            var path = new Path
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Stretch = Stretch.None
            };

            path.SetBinding(Shape.FillProperty,
                new Binding { Source = this, Path = new PropertyPath(nameof(Background)) });

            path.SetBinding(Shape.StrokeProperty,
                new Binding { Source = this, Path = new PropertyPath(nameof(BorderBrush)) });

            path.SetBinding(Shape.StrokeThicknessProperty,
                new Binding { Source = this, Path = new PropertyPath(nameof(BorderWidth)) });

            border.SetBinding(PaddingProperty,
                new Binding { Source = this, Path = new PropertyPath(nameof(Padding)) });

            SetBorderMargin();

            var grid = new Grid();
            grid.Children.Add(path);
            grid.Children.Add(border);

            Content = grid;

            SizeChanged += (_, _) => path.Data = BuildGeometry();
        }

        public UIElement Child
        {
            get => border.Child;
            set => border.Child = value;
        }

        private void SetBorderMargin()
        {
            border.Margin = new Thickness(
                BorderWidth, BorderWidth, BorderWidth, BorderWidth + ArrowSize.Height);
        }
    }
}
