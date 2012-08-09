// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Positions child elements on a Map. A child element's position is specified by the
    /// attached property Location, given as geographic location with latitude and longitude.
    /// The attached property ViewportPosition gets a child element's position in viewport
    /// coordinates and indicates if the coordinates are located inside the bounds of the ParentMap.
    /// </summary>
    public class MapPanel : Panel
    {
        public static readonly DependencyProperty ParentMapProperty = DependencyProperty.RegisterAttached(
            "ParentMap", typeof(Map), typeof(MapPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, ParentMapPropertyChanged));

        public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached(
            "Location", typeof(Location), typeof(MapPanel),
            new FrameworkPropertyMetadata(LocationPropertyChanged));

        private static readonly DependencyPropertyKey ViewportPositionPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "ViewportPosition", typeof(ViewportPosition), typeof(MapPanel),
            new FrameworkPropertyMetadata(ViewportPositionPropertyChanged));

        public static readonly DependencyProperty ViewportPositionProperty = ViewportPositionPropertyKey.DependencyProperty;

        public MapPanel()
        {
            ClipToBounds = true;
        }

        public Map ParentMap
        {
            get { return (Map)GetValue(ParentMapProperty); }
        }

        public static Map GetParentMap(UIElement element)
        {
            return (Map)element.GetValue(ParentMapProperty);
        }

        public static Location GetLocation(UIElement element)
        {
            return (Location)element.GetValue(LocationProperty);
        }

        public static void SetLocation(UIElement element, Location value)
        {
            element.SetValue(LocationProperty, value);
        }

        public static ViewportPosition GetViewportPosition(UIElement element)
        {
            return (ViewportPosition)element.GetValue(ViewportPositionProperty);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (UIElement element in InternalChildren)
            {
                element.Measure(infiniteSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement element in InternalChildren)
            {
                ViewportPosition viewportPosition = GetViewportPosition(element);

                if (viewportPosition == null || !ArrangeElement(element, viewportPosition))
                {
                    ArrangeElement(element, finalSize);
                }
            }

            return finalSize;
        }

        protected virtual Point GetArrangePosition(ViewportPosition viewportPosition)
        {
            return viewportPosition.Position;
        }

        protected virtual void OnViewportChanged()
        {
            Map parentMap = ParentMap;

            foreach (UIElement element in InternalChildren)
            {
                Location location = GetLocation(element);

                if (location != null)
                {
                    SetViewportPosition(element, parentMap, location);
                }
            }
        }

        private static void ParentMapPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs)
        {
            MapPanel mapPanel = obj as MapPanel;

            if (mapPanel != null)
            {
                Map oldParentMap = eventArgs.OldValue as Map;
                Map newParentMap = eventArgs.NewValue as Map;

                if (oldParentMap != null && oldParentMap != mapPanel)
                {
                    oldParentMap.ViewportChanged -= mapPanel.OnViewportChanged;
                }

                if (newParentMap != null && newParentMap != mapPanel)
                {
                    newParentMap.ViewportChanged += mapPanel.OnViewportChanged;
                }
            }
        }

        private static void LocationPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs)
        {
            UIElement element = obj as UIElement;

            if (element != null)
            {
                SetViewportPosition(element, GetParentMap(element), (Location)eventArgs.NewValue);
            }
        }

        private static void SetViewportPosition(UIElement element, Map parentMap, Location location)
        {
            ViewportPosition viewportPosition = null;

            if (parentMap != null && location != null)
            {
                Point position = parentMap.LocationToViewportPoint(location);

                viewportPosition = new ViewportPosition(position,
                    position.X >= 0d && position.X <= parentMap.ActualWidth &&
                    position.Y >= 0d && position.Y <= parentMap.ActualHeight);
            }

            element.SetValue(ViewportPositionPropertyKey, viewportPosition);
        }

        private static void ViewportPositionPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs)
        {
            UIElement element = obj as UIElement;

            if (element != null)
            {
                ViewportPosition position = (ViewportPosition)eventArgs.NewValue;

                if (position != null)
                {
                    ArrangeElement(element, position);
                }
                else
                {
                    element.Arrange(new Rect());
                }
            }
        }

        private static bool ArrangeElement(UIElement element, ViewportPosition viewportPosition)
        {
            MapPanel panel = VisualTreeHelper.GetParent(element) as MapPanel;
            Point position = panel != null ? panel.GetArrangePosition(viewportPosition) : viewportPosition.Position;
            Rect rect = new Rect(position, element.DesiredSize);
            FrameworkElement frameworkElement = element as FrameworkElement;

            if (frameworkElement != null)
            {
                if (frameworkElement.HorizontalAlignment == HorizontalAlignment.Stretch ||
                    frameworkElement.VerticalAlignment == VerticalAlignment.Stretch)
                {
                    return false; // do not arrange at position
                }

                switch (frameworkElement.HorizontalAlignment)
                {
                    case HorizontalAlignment.Center:
                        rect.X -= rect.Width / 2d;
                        break;
                    case HorizontalAlignment.Right:
                        rect.X -= rect.Width;
                        break;
                    default:
                        break;
                }

                switch (frameworkElement.VerticalAlignment)
                {
                    case VerticalAlignment.Center:
                        rect.Y -= rect.Height / 2d;
                        break;
                    case VerticalAlignment.Bottom:
                        rect.Y -= rect.Height;
                        break;
                    default:
                        break;
                }
            }

            element.Arrange(rect);
            return true;
        }

        private static void ArrangeElement(UIElement element, Size panelSize)
        {
            Rect rect = new Rect(element.DesiredSize);
            FrameworkElement frameworkElement = element as FrameworkElement;

            if (frameworkElement != null)
            {
                switch (frameworkElement.HorizontalAlignment)
                {
                    case HorizontalAlignment.Center:
                        rect.X = (panelSize.Width - rect.Width) / 2d;
                        break;
                    case HorizontalAlignment.Right:
                        rect.X = panelSize.Width - rect.Width;
                        break;
                    case HorizontalAlignment.Stretch:
                        rect.Width = panelSize.Width;
                        break;
                    default:
                        break;
                }

                switch (frameworkElement.VerticalAlignment)
                {
                    case VerticalAlignment.Center:
                        rect.Y = (panelSize.Height - rect.Height) / 2d;
                        break;
                    case VerticalAlignment.Bottom:
                        rect.Y = panelSize.Height - rect.Height;
                        break;
                    case VerticalAlignment.Stretch:
                        rect.Height = panelSize.Height;
                        break;
                    default:
                        break;
                }
            }

            element.Arrange(rect);
        }
    }
}
