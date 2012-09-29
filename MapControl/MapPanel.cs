// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;

namespace MapControl
{
    /// <summary>
    /// Positions child elements on a Map. A child element's position is specified by the
    /// attached property Location, given as geographic location with latitude and longitude.
    /// The attached property ViewportPosition gets a child element's position in viewport coordinates.
    /// </summary>
    public class MapPanel : Panel
    {
        internal static readonly DependencyPropertyKey ParentMapPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "ParentMap", typeof(Map), typeof(MapPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, ParentMapPropertyChanged));

        public static readonly DependencyProperty ParentMapProperty = ParentMapPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ViewportPositionPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "ViewportPosition", typeof(Point?), typeof(MapPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, ViewportPositionPropertyChanged));

        public static readonly DependencyProperty ViewportPositionProperty = ViewportPositionPropertyKey.DependencyProperty;

        public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached(
            "Location", typeof(Location), typeof(MapPanel),
            new FrameworkPropertyMetadata(LocationPropertyChanged));

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

        public static Point? GetViewportPosition(UIElement element)
        {
            return (Point?)element.GetValue(ViewportPositionProperty);
        }

        public static Location GetLocation(UIElement element)
        {
            return (Location)element.GetValue(LocationProperty);
        }

        public static void SetLocation(UIElement element, Location value)
        {
            element.SetValue(LocationProperty, value);
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
                Point? viewportPosition = GetViewportPosition(element);

                if (viewportPosition.HasValue)
                {
                    ArrangeElement(element, viewportPosition.Value);
                }
                else
                {
                    element.Arrange(new Rect(finalSize));
                }
            }

            return finalSize;
        }

        protected virtual void OnViewportChanged()
        {
            foreach (UIElement element in InternalChildren)
            {
                Location location = GetLocation(element);

                if (location != null)
                {
                    SetViewportPosition(element, ParentMap, location);
                }
            }
        }

        private static void ParentMapPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MapPanel mapPanel = obj as MapPanel;

            if (mapPanel != null)
            {
                Map oldParentMap = e.OldValue as Map;
                Map newParentMap = e.NewValue as Map;

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

        private static void ViewportPositionPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = obj as UIElement;

            if (element != null)
            {
                Point? viewportPosition = (Point?)e.NewValue;

                if (viewportPosition.HasValue)
                {
                    ArrangeElement(element, viewportPosition.Value);
                }
                else
                {
                    element.Arrange(new Rect());
                }
            }
        }

        private static void LocationPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = obj as UIElement;

            if (element != null)
            {
                SetViewportPosition(element, GetParentMap(element), (Location)e.NewValue);
            }
        }

        private static void SetViewportPosition(UIElement element, Map parentMap, Location location)
        {
            Point? viewportPosition = null;

            if (parentMap != null && location != null)
            {
                viewportPosition = parentMap.LocationToViewportPoint(location);
            }

            element.SetValue(ViewportPositionPropertyKey, viewportPosition);
        }

        private static void ArrangeElement(UIElement element, Point position)
        {
            Rect rect = new Rect(position, element.DesiredSize);

            if (element is FrameworkElement)
            {
                switch (((FrameworkElement)element).HorizontalAlignment)
                {
                    case HorizontalAlignment.Center:
                        rect.X -= rect.Width / 2d;
                        break;
                    case HorizontalAlignment.Right:
                        rect.X -= rect.Width;
                        break;
                    case HorizontalAlignment.Stretch:
                        rect.X = 0d;
                        break;
                    default:
                        break;
                }

                switch (((FrameworkElement)element).VerticalAlignment)
                {
                    case VerticalAlignment.Center:
                        rect.Y -= rect.Height / 2d;
                        break;
                    case VerticalAlignment.Bottom:
                        rect.Y -= rect.Height;
                        break;
                    case VerticalAlignment.Stretch:
                        rect.Y = 0d;
                        break;
                    default:
                        break;
                }
            }

            element.Arrange(rect);
        }
    }
}
