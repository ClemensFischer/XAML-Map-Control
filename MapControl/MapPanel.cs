// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINDOWS_RUNTIME
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Positions child elements on a Map, at a position specified by the attached property Location.
    /// The Location is transformed to a viewport position by ParentMap.MapTransform and ParentMap.ViewportTransform
    /// and applied to a child element's RenderTransform as an appropriate TranslateTransform.
    /// </summary>
    public partial class MapPanel : PanelBase, IMapElement
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached(
            "Location", typeof(Location), typeof(MapPanel), new PropertyMetadata(null, LocationPropertyChanged));

        public static Location GetLocation(UIElement element)
        {
            return (Location)element.GetValue(LocationProperty);
        }

        public static void SetLocation(UIElement element, Location value)
        {
            element.SetValue(LocationProperty, value);
        }

        private MapBase parentMap;

        public MapBase ParentMap
        {
            get { return parentMap; }
            set { SetParentMapOverride(value); }
        }

        protected virtual void SetParentMapOverride(MapBase map)
        {
            if (parentMap != null && parentMap != this)
            {
                parentMap.ViewportChanged -= OnViewportChanged;
            }

            parentMap = map;

            if (parentMap != null && parentMap != this)
            {
                parentMap.ViewportChanged += OnViewportChanged;
                OnViewportChanged();
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement element in Children)
            {
                var location = GetLocation(element);

                if (location != null)
                {
                    ArrangeElementWithLocation(element);
                    SetViewportPosition(element, parentMap, location);
                }
                else
                {
                    ArrangeElementWithoutLocation(element, finalSize);
                }
            }

            return finalSize;
        }

        protected virtual void OnViewportChanged()
        {
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

                if (location == null)
                {
                    ArrangeElementWithoutLocation(element, Size.Empty);
                }
                else if (e.OldValue == null)
                {
                    ArrangeElementWithLocation(element); // arrange element when Location was null before
                }

                SetViewportPosition(element, parentMap, location);
            }
        }

        private static void SetViewportPosition(UIElement element, MapBase parentMap, Location location)
        {
            Point viewportPosition;

            if (parentMap != null && location != null)
            {
                var mapPosition = parentMap.MapTransform.Transform(location, parentMap.Center.Longitude); // nearest to center longitude
                viewportPosition = parentMap.ViewportTransform.Transform(mapPosition);
            }
            else
            {
                viewportPosition = new Point();
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

        private static void ArrangeElementWithLocation(UIElement element)
        {
            var rect = new Rect(new Point(), element.DesiredSize);
            var frameworkElement = element as FrameworkElement;

            if (frameworkElement != null)
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

            element.Arrange(rect);
        }

        private static void ArrangeElementWithoutLocation(UIElement element, Size parentSize)
        {
            var rect = new Rect(new Point(), element.DesiredSize);
            var frameworkElement = element as FrameworkElement;

            if (frameworkElement != null)
            {
                if (parentSize.IsEmpty)
                {
                    var parent = frameworkElement.Parent as UIElement;
                    parentSize = parent != null ? parent.RenderSize : new Size();
                }

                switch (frameworkElement.HorizontalAlignment)
                {
                    case HorizontalAlignment.Center:
                        rect.X = (parentSize.Width - rect.Width) / 2d;
                        break;

                    case HorizontalAlignment.Right:
                        rect.X = parentSize.Width - rect.Width;
                        break;

                    case HorizontalAlignment.Stretch:
                        rect.Width = parentSize.Width;
                        break;

                    default:
                        break;
                }

                switch (frameworkElement.VerticalAlignment)
                {
                    case VerticalAlignment.Center:
                        rect.Y = (parentSize.Height - rect.Height) / 2d;
                        break;

                    case VerticalAlignment.Bottom:
                        rect.Y = parentSize.Height - rect.Height;
                        break;

                    case VerticalAlignment.Stretch:
                        rect.Height = parentSize.Height;
                        break;

                    default:
                        break;
                }
            }

            element.Arrange(rect);
        }
    }
}
