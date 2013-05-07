// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
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
        MapBase ParentMap { get; set; }
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

        private MapBase parentMap;

        public MapBase ParentMap
        {
            get { return parentMap; }
            set
            {
                if (parentMap != null && parentMap != this)
                {
                    parentMap.ViewportChanged -= (o, e) => OnViewportChanged();
                }

                parentMap = value;

                if (parentMap != null && parentMap != this)
                {
                    parentMap.ViewportChanged += (o, e) => OnViewportChanged();
                    OnViewportChanged();
                }
            }
        }

        protected virtual void OnViewportChanged()
        {
            foreach (UIElement element in InternalChildren)
            {
                var location = GetLocation(element);

                if (location != null)
                {
                    SetViewportPosition(element, parentMap, location);
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement element in InternalChildren)
            {
                element.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement element in InternalChildren)
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

        private static void ParentMapPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var mapElement = obj as IMapElement;

            if (mapElement != null)
            {
                mapElement.ParentMap = e.NewValue as MapBase;
            }
        }

        private static void LocationPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var element = obj as UIElement;

            if (element != null)
            {
                var mapElement = element as IMapElement;
                var parentMap = mapElement != null ? mapElement.ParentMap : GetParentMap(element);

                SetViewportPosition(element, parentMap, (Location)e.NewValue);
            }
        }

        private static void SetViewportPosition(UIElement element, MapBase parentMap, Location location)
        {
            Point viewportPosition;

            if (parentMap != null && location != null)
            {
                viewportPosition = parentMap.LocationToViewportPoint(location);
                element.SetValue(ViewportPositionProperty, viewportPosition);
            }
            else
            {
                viewportPosition = new Point();
                element.ClearValue(ViewportPositionProperty);
            }

            var translateTransform = element.RenderTransform as TranslateTransform;

            if (translateTransform == null)
            {
                var transformGroup = element.RenderTransform as TransformGroup;

                if (transformGroup == null)
                {
                    translateTransform = new TranslateTransform();
                    element.RenderTransform = translateTransform;
                }
                else
                {
                    if (transformGroup.Children.Count > 0)
                    {
                        translateTransform = transformGroup.Children[transformGroup.Children.Count - 1] as TranslateTransform;
                    }

                    if (translateTransform == null)
                    {
                        translateTransform = new TranslateTransform();
                        transformGroup.Children.Add(translateTransform);
                    }
                }
            }

            translateTransform.X = viewportPosition.X;
            translateTransform.Y = viewportPosition.Y;
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
