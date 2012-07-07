// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Controls;

namespace MapControl
{
    /// <summary>
    /// Positions child elements on a Map. A child element's position is specified by the
    /// attached property Location, given as geographic location with latitude and longitude.
    /// The attached property ViewportPosition gets a child element's position in viewport
    /// coordinates. IsInsideMapBounds indicates if the viewport coordinates are located
    /// inside the visible part of the map.
    /// </summary>
    public class MapPanel : Panel, INotifyParentMapChanged
    {
        public static readonly DependencyProperty ParentMapProperty = DependencyProperty.RegisterAttached(
            "ParentMap", typeof(Map), typeof(MapPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, ParentMapPropertyChanged));

        public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached(
            "Location", typeof(Location), typeof(MapPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, LocationPropertyChanged));

        private static readonly DependencyPropertyKey ViewportPositionPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "ViewportPosition", typeof(Point), typeof(MapPanel), null);

        private static readonly DependencyPropertyKey IsInsideMapBoundsPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "IsInsideMapBounds", typeof(bool), typeof(MapPanel), null);

        public static readonly DependencyProperty ViewportPositionProperty = ViewportPositionPropertyKey.DependencyProperty;
        public static readonly DependencyProperty IsInsideMapBoundsProperty = IsInsideMapBoundsPropertyKey.DependencyProperty;

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

        public static bool HasViewportPosition(UIElement element)
        {
            return element.ReadLocalValue(ViewportPositionProperty) != DependencyProperty.UnsetValue;
        }

        public static Point GetViewportPosition(UIElement element)
        {
            return (Point)element.GetValue(ViewportPositionProperty);
        }

        public static bool GetIsInsideMapBounds(UIElement element)
        {
            return (bool)element.GetValue(IsInsideMapBoundsProperty);
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
                object viewportPosition = element.ReadLocalValue(ViewportPositionProperty);

                if (viewportPosition == DependencyProperty.UnsetValue ||
                    !ArrangeElement(element, (Point)viewportPosition))
                {
                    ArrangeElement(element, finalSize);
                }
            }

            return finalSize;
        }

        protected virtual void OnViewTransformChanged(Map parentMap)
        {
            foreach (UIElement element in InternalChildren)
            {
                Location location = GetLocation(element);

                if (location != null)
                {
                    SetViewportPosition(element, parentMap, location);
                }
            }
        }

        private void OnViewTransformChanged(object sender, EventArgs eventArgs)
        {
            OnViewTransformChanged((Map)sender);
        }

        void INotifyParentMapChanged.ParentMapChanged(Map oldParentMap, Map newParentMap)
        {
            if (oldParentMap != null && oldParentMap != this)
            {
                oldParentMap.ViewTransformChanged -= OnViewTransformChanged;
            }

            if (newParentMap != null && newParentMap != this)
            {
                newParentMap.ViewTransformChanged += OnViewTransformChanged;
            }
        }

        private static void ParentMapPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs)
        {
            INotifyParentMapChanged notifyChanged = obj as INotifyParentMapChanged;

            if (notifyChanged != null)
            {
                notifyChanged.ParentMapChanged(eventArgs.OldValue as Map, eventArgs.NewValue as Map);
            }
        }

        private static void LocationPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs)
        {
            UIElement element = (UIElement)obj;
            Location location = (Location)eventArgs.NewValue;
            Map parentMap;

            if (location != null && (parentMap = GetParentMap(element)) != null)
            {
                SetViewportPosition(element, parentMap, location);
            }
            else
            {
                element.ClearValue(ViewportPositionPropertyKey);
                element.ClearValue(IsInsideMapBoundsPropertyKey);
                element.Arrange(new Rect());
            }
        }

        private static void SetViewportPosition(UIElement element, Map parentMap, Location location)
        {
            Point viewportPosition = parentMap.LocationToViewportPoint(location);

            element.SetValue(ViewportPositionPropertyKey, viewportPosition);
            element.SetValue(IsInsideMapBoundsPropertyKey,
                viewportPosition.X >= 0d && viewportPosition.X <= parentMap.ActualWidth &&
                viewportPosition.Y >= 0d && viewportPosition.Y <= parentMap.ActualHeight);

            ArrangeElement(element, viewportPosition);
        }

        private static bool ArrangeElement(UIElement element, Point position)
        {
            Rect rect = new Rect(position, element.DesiredSize);
            FrameworkElement frameworkElement = element as FrameworkElement;

            if (frameworkElement != null)
            {
                if (frameworkElement.HorizontalAlignment == HorizontalAlignment.Stretch &&
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
