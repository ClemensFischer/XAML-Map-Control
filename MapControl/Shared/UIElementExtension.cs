#if WPF
using System.Windows;
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#elif AVALONIA
global using UIElement = Avalonia.Controls.Control;
global using FrameworkElement = Avalonia.Controls.Control;
using Avalonia;
using Avalonia.Media;
#endif

namespace MapControl
{
    public static class UIElementExtension
    {
        public static void SetRenderTransform(this UIElement element, Transform transform, bool center = false)
        {
            element.RenderTransform = transform;
#if AVALONIA
            element.RenderTransformOrigin = center ? RelativePoint.Center : RelativePoint.TopLeft;
#else
            if (center)
            {
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }
#endif
        }

        public static void SetVisible(this UIElement element, bool visible)
        {
#if AVALONIA
            element.IsVisible = visible;
#else
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
#endif
        }
    }
}
