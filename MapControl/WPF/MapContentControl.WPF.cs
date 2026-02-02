using System.Windows;

namespace MapControl
{
    public partial class MapContentControl
    {
        static MapContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapContentControl), new FrameworkPropertyMetadata(typeof(MapContentControl)));
        }
    }

    public partial class Pushpin
    {
        static Pushpin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Pushpin), new FrameworkPropertyMetadata(typeof(Pushpin)));
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyPropertyHelper.Register<Pushpin, CornerRadius>(nameof(CornerRadius));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
