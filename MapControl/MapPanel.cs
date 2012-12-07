// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINRT
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#endif

namespace MapControl
{
    internal interface IMapElement
    {
        void ParentMapChanged(MapBase oldParentMap, MapBase newParentMap);
    }

    /// <summary>
    /// Positions child elements on a Map, at a position specified by the attached property Location.
    /// The Location is transformed into a viewport position by the MapBase.LocationToViewportPoint
    /// method and then applied to the RenderTransform as an appropriate TranslateTransform.
    /// </summary>
    public partial class MapPanel : Panel, IMapElement
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached(
            "Location", typeof(Location), typeof(MapPanel), new PropertyMetadata(null, LocationPropertyChanged));

        public static readonly DependencyProperty ViewportPositionProperty = DependencyProperty.RegisterAttached(
            "ViewportPosition", typeof(Point?), typeof(MapPanel), null);

        public static Location GetLocation(UIElement element)
        {
            return (Location)element.GetValue(LocationProperty);
        }

        public static void SetLocation(UIElement element, Location value)
        {
            element.SetValue(LocationProperty, value);
        }

        public static Point? GetViewportPosition(UIElement element)
        {
            return (Point?)element.GetValue(ViewportPositionProperty);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement element in Children)
            {
                element.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var parentMap = GetParentMap(this);

            foreach (UIElement element in Children)
            {
                var rect = new Rect(0d, 0d, element.DesiredSize.Width, element.DesiredSize.Height);
                var location = GetLocation(element);

                if (element is FrameworkElement)
                {
                    if (location != null)
                    {
                        AlignElementWithLocation((FrameworkElement)element, ref rect);
                    }
                    else
                    {
                        AlignElementWithoutLocation((FrameworkElement)element, finalSize, ref rect);
                    }
                }

                element.Arrange(rect);

                if (location != null)
                {
                    SetViewportPosition(element, parentMap, location);
                }

            }

            return finalSize;
        }

        protected virtual void OnViewportChanged()
        {
            var parentMap = GetParentMap(this);

            foreach (UIElement element in Children)
            {
                var location = GetLocation(element);

                if (location != null)
                {
                    SetViewportPosition(element, parentMap, location);
                }
            }
        }

        private void OnViewportChanged(object sender, EventArgs e)
        {
            OnViewportChanged();
        }

        void IMapElement.ParentMapChanged(MapBase oldParentMap, MapBase newParentMap)
        {
            if (oldParentMap != null && oldParentMap != this)
            {
                oldParentMap.ViewportChanged -= OnViewportChanged;
            }

            if (newParentMap != null && newParentMap != this)
            {
                newParentMap.ViewportChanged += OnViewportChanged;
                OnViewportChanged();
            }
        }

        private static void ParentMapPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var element = obj as IMapElement;

            if (element != null)
            {
                element.ParentMapChanged(e.OldValue as MapBase, e.NewValue as MapBase);
            }
        }

        private static void LocationPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var element = obj as UIElement;

            if (element != null)
            {
                SetViewportPosition(element, GetParentMap(element), (Location)e.NewValue);
            }
        }

        private static void SetViewportPosition(UIElement element, MapBase parentMap, Location location)
        {
            Point? viewportPosition = null;

            if (parentMap != null && location != null)
            {
                viewportPosition = parentMap.LocationToViewportPoint(location);
                element.SetValue(ViewportPositionProperty, viewportPosition);
            }
            else
            {
                element.ClearValue(ViewportPositionProperty);
            }

            var transformGroup = element.RenderTransform as TransformGroup;

            if (transformGroup != null)
            {
                var transform = new TranslateTransform();

                if (viewportPosition.HasValue)
                {
                    transform.X = viewportPosition.Value.X;
                    transform.Y = viewportPosition.Value.Y;
                }

                var transformIndex = transformGroup.Children.Count - 1;

                if (transformIndex >= 0 &&
                    transformGroup.Children[transformIndex] is TranslateTransform)
                {
                    transformGroup.Children[transformIndex] = transform;
                }
                else
                {
                    transformGroup.Children.Add(transform);
                }
            }
            else if (viewportPosition.HasValue)
            {
                element.RenderTransform = new TranslateTransform
                {
                    X = viewportPosition.Value.X,
                    Y = viewportPosition.Value.Y
                };
            }
            else
            {
                element.ClearValue(UIElement.RenderTransformProperty);
            }
        }

        private static void AlignElementWithLocation(FrameworkElement element, ref Rect arrangeRect)
        {
            switch (element.HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    arrangeRect.X = -arrangeRect.Width / 2d;
                    break;

                case HorizontalAlignment.Right:
                    arrangeRect.X = -arrangeRect.Width;
                    break;

                default:
                    break;
            }

            switch (element.VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    arrangeRect.Y = -arrangeRect.Height / 2d;
                    break;

                case VerticalAlignment.Bottom:
                    arrangeRect.Y = -arrangeRect.Height;
                    break;

                default:
                    break;
            }
        }

        private static void AlignElementWithoutLocation(FrameworkElement element, Size panelSize, ref Rect arrangeRect)
        {
            switch (element.HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    arrangeRect.X = (panelSize.Width - arrangeRect.Width) / 2d;
                    break;

                case HorizontalAlignment.Right:
                    arrangeRect.X = panelSize.Width - arrangeRect.Width;
                    break;

                case HorizontalAlignment.Stretch:
                    arrangeRect.Width = panelSize.Width;
                    break;

                default:
                    break;
            }

            switch (element.VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    arrangeRect.Y = (panelSize.Height - arrangeRect.Height) / 2d;
                    break;

                case VerticalAlignment.Bottom:
                    arrangeRect.Y = panelSize.Height - arrangeRect.Height;
                    break;

                case VerticalAlignment.Stretch:
                    arrangeRect.Height = panelSize.Height;
                    break;

                default:
                    break;
            }
        }
    }
}
