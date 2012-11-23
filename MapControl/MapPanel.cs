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
                var location = GetLocation(element);

                if (location != null)
                {
                    SetViewportPosition(element, parentMap, location);
                }

                var rect = new Rect(0d, 0d, element.DesiredSize.Width, element.DesiredSize.Height);
                var frameworkElement = element as FrameworkElement;

                if (frameworkElement != null)
                {
                    switch (frameworkElement.HorizontalAlignment)
                    {
                        case HorizontalAlignment.Center:
                            rect.X = ((location == null ? finalSize.Width : 0) - rect.Width) / 2d;
                            break;

                        case HorizontalAlignment.Right:
                            rect.X = (location == null ? finalSize.Width : 0) - rect.Width;
                            break;

                        case HorizontalAlignment.Stretch:
                            if (location == null)
                            {
                                rect.Width = finalSize.Width;
                            }
                            break;

                        default:
                            break;
                    }

                    switch (frameworkElement.VerticalAlignment)
                    {
                        case VerticalAlignment.Center:
                            rect.Y = ((location == null ? finalSize.Height : 0) - rect.Height) / 2d;
                            break;

                        case VerticalAlignment.Bottom:
                            rect.Y = (location == null ? finalSize.Height : 0) - rect.Height;
                            break;

                        case VerticalAlignment.Stretch:
                            if (location == null)
                            {
                                rect.Height = finalSize.Height;
                            }
                            break;

                        default:
                            break;
                    }
                }

                element.Arrange(rect);
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
            Transform transform = null;

            if (parentMap != null && location != null)
            {
                Point position = parentMap.LocationToViewportPoint(location);
                transform = new TranslateTransform { X = position.X, Y = position.Y };
            }

            var transformGroup = element.RenderTransform as TransformGroup;

            if (transformGroup != null)
            {
                if (transform == null)
                {
                    transform = new TranslateTransform();
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
            else if (transform != null)
            {
                element.RenderTransform = transform;
            }
            else
            {
                element.ClearValue(UIElement.RenderTransformProperty);
            }
        }
    }
}
