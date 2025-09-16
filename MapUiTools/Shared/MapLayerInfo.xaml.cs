#if WPF
using System.Windows;
using System.Windows.Controls;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#else
using Avalonia.Controls;
using DependencyProperty = Avalonia.AvaloniaProperty;
using FrameworkElement = Avalonia.Controls.Control;
#endif

namespace MapControl.UiTools
{
    public partial class MapLayerInfo : UserControl
    {
        public static readonly DependencyProperty MapLayerProperty =
            DependencyPropertyHelper.Register<MapLayerInfo, FrameworkElement>(nameof(MapLayer), null);

        public FrameworkElement MapLayer
        {
            get => (FrameworkElement)GetValue(MapLayerProperty);
            set => SetValue(MapLayerProperty, value);
        }

        public MapLayerInfo()
        {
            InitializeComponent();
        }
    }
}
