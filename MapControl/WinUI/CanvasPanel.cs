// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Linq;
using Windows.Foundation;
#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace MapControl
{
    /// <summary>
    /// Replacement for WinUI and UWP Canvas, which clips MapPath child elements.
    /// </summary>
    public class CanvasPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (var element in Children.OfType<UIElement>())
            {
                element.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var element in Children.OfType<UIElement>())
            {
                var x = Canvas.GetLeft(element);
                var y = Canvas.GetTop(element);
                var size = MapPanel.GetDesiredSize(element);

                element.Arrange(new Rect(x, y, size.Width, size.Height));
            }

            return finalSize;
        }
    }
}
