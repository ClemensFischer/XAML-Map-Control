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
    /// method and applied to a child element's RenderTransform as an appropriate TranslateTransform.
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
                    parentMap.ViewportChanged -= OnViewportChanged;
                }

                parentMap = value;

                if (parentMap != null && parentMap != this)
                {
                    parentMap.ViewportChanged += OnViewportChanged;
                    OnViewportChanged();
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
                var location = GetLocation(element);

                ArrangeElement(element, finalSize, location != null);

                if (location != null)
                {
                    SetViewportPosition(element, parentMap, location);
                }
            }

            return finalSize;
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

        private void OnViewportChanged(object sender, EventArgs e)
        {
            OnViewportChanged();
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
                var location = e.NewValue as Location;

                if ((location != null) != (e.OldValue != null))
                {
                    ArrangeElement(element, null, location != null);
                }

                SetViewportPosition(element, parentMap, location);
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

        private static void ArrangeElement(UIElement element, Size? panelSize, bool hasLocation)
        {
            var rect = new Rect(0d, 0d, element.DesiredSize.Width, element.DesiredSize.Height);
            var frameworkElement = element as FrameworkElement;

            if (frameworkElement != null)
            {
                if (hasLocation)
                {
                    switch (frameworkElement.HorizontalAlignment)
                    {
                        case HorizontalAlignment.Center:
                            rect.X = -rect.Width / 2d;
                            break;

                        case HorizontalAlignment.Right:
                            rect.X = -rect.Width;
                            break;

                        default:
                            break;
                    }

                    switch (frameworkElement.VerticalAlignment)
                    {
                        case VerticalAlignment.Center:
                            rect.Y = -rect.Height / 2d;
                            break;

                        case VerticalAlignment.Bottom:
                            rect.Y = -rect.Height;
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    if (!panelSize.HasValue)
                    {
                        var panel = frameworkElement.Parent as Panel;
                        panelSize = panel != null ? panel.RenderSize : new Size();
                    }

                    switch (frameworkElement.HorizontalAlignment)
                    {
                        case HorizontalAlignment.Center:
                            rect.X = (panelSize.Value.Width - rect.Width) / 2d;
                            break;

                        case HorizontalAlignment.Right:
                            rect.X = panelSize.Value.Width - rect.Width;
                            break;

                        case HorizontalAlignment.Stretch:
                            rect.Width = panelSize.Value.Width;
                            break;

                        default:
                            break;
                    }

                    switch (frameworkElement.VerticalAlignment)
                    {
                        case VerticalAlignment.Center:
                            rect.Y = (panelSize.Value.Height - rect.Height) / 2d;
                            break;

                        case VerticalAlignment.Bottom:
                            rect.Y = panelSize.Value.Height - rect.Height;
                            break;

                        case VerticalAlignment.Stretch:
                            rect.Height = panelSize.Value.Height;
                            break;

                        default:
                            break;
                    }
                }
            }

            element.Arrange(rect);
        }
    }
}
