// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_RUNTIME
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;

#endif

namespace MapControl
{
    /// <summary>
    /// Common base class for MapPanel, TileLayer and TileContainer.
    /// </summary>
    public class PanelBase : Panel
    {
#if WINDOWS_RUNTIME || SILVERLIGHT
        protected internal UIElementCollection InternalChildren
        {
            get { return Children; }
        }
#endif
        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (UIElement element in InternalChildren)
            {
                element.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                child.Arrange(new Rect(new Point(), finalSize));
            }

            return finalSize;
        }
    }
}
