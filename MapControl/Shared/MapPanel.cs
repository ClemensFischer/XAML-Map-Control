// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Linq;
#if WINUI
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
#elif UWP
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
    /// <summary>
    /// Optional interface to hold the value of the attached property MapPanel.ParentMap.
    /// </summary>
    public interface IMapElement
    {
        MapBase ParentMap { get; set; }
    }

    /// <summary>
    /// Arranges child elements on a Map at positions specified by the attached property Location,
    /// or in rectangles specified by the attached property BoundingBox.
    /// </summary>
    public partial class MapPanel : Panel, IMapElement
    {
        private static void ParentMapPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is IMapElement mapElement)
            {
                mapElement.ParentMap = e.NewValue as MapBase;
            }
        }

        private MapBase parentMap;

        /// <summary>
        /// Implements IMapElement.ParentMap.
        /// </summary>
        public MapBase ParentMap
        {
            get => parentMap;
            set => SetParentMap(value);
        }

        public static readonly DependencyProperty AutoCollapseProperty = DependencyProperty.RegisterAttached(
            "AutoCollapse", typeof(bool), typeof(MapPanel), new PropertyMetadata(false));

        /// <summary>
        /// Gets a value that controls whether an element's Visibility is automatically
        /// set to Collapsed when it is located outside the visible viewport area.
        /// </summary>
        public static bool GetAutoCollapse(FrameworkElement element)
        {
            return (bool)element.GetValue(AutoCollapseProperty);
        }

        /// <summary>
        /// Sets the AutoCollapse property.
        /// </summary>
        public static void SetAutoCollapse(FrameworkElement element, bool value)
        {
            element.SetValue(AutoCollapseProperty, value);
        }

        /// <summary>
        /// Gets the Location of an element.
        /// </summary>
        public static Location GetLocation(FrameworkElement element)
        {
            return (Location)element.GetValue(LocationProperty);
        }

        /// <summary>
        /// Sets the Location of an element.
        /// </summary>
        public static void SetLocation(FrameworkElement element, Location value)
        {
            element.SetValue(LocationProperty, value);
        }

        /// <summary>
        /// Gets the BoundingBox of an element.
        /// </summary>
        public static BoundingBox GetBoundingBox(FrameworkElement element)
        {
            return (BoundingBox)element.GetValue(BoundingBoxProperty);
        }

        /// <summary>
        /// Sets the BoundingBox of an element.
        /// </summary>
        public static void SetBoundingBox(FrameworkElement element, BoundingBox value)
        {
            element.SetValue(BoundingBoxProperty, value);
        }

        /// <summary>
        /// Gets the view position of an element with Location.
        /// </summary>
        public static Point? GetViewPosition(FrameworkElement element)
        {
            return (Point?)element.GetValue(ViewPositionProperty);
        }

        protected virtual void SetParentMap(MapBase map)
        {
            if (parentMap != null && parentMap != this)
            {
                parentMap.ViewportChanged -= OnViewportChanged;
            }

            parentMap = map;

            if (parentMap != null && parentMap != this)
            {
                parentMap.ViewportChanged += OnViewportChanged;

                OnViewportChanged(new ViewportChangedEventArgs());
            }
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            OnViewportChanged(e);
        }

        protected virtual void OnViewportChanged(ViewportChangedEventArgs e)
        {
            InvalidateArrange();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (var element in Children.OfType<FrameworkElement>())
            {
                element.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (parentMap != null)
            {
                foreach (var element in Children.OfType<FrameworkElement>())
                {
                    var location = GetLocation(element);
                    var position = location != null ? GetViewPosition(location) : null;

                    SetViewPosition(element, ref position);

                    if (GetAutoCollapse(element))
                    {
                        element.Visibility = position.HasValue && IsOutsideViewport(position.Value)
                            ? Visibility.Collapsed : Visibility.Visible;
                    }

                    if (position.HasValue)
                    {
                        ArrangeElement(element, position.Value);
                    }
                    else
                    {
                        var boundingBox = GetBoundingBox(element);

                        if (boundingBox != null)
                        {
                            var viewRect = GetViewRect(boundingBox);

                            if (viewRect.HasValue)
                            {
                                ArrangeElement(element, viewRect.Value);
                            }
                        }
                        else
                        {
                            ArrangeElement(element, finalSize);
                        }
                    }
                }
            }

            return finalSize;
        }

        protected Point? GetViewPosition(Location location)
        {
            var position = parentMap.LocationToView(location);

            if (parentMap.MapProjection.Type <= MapProjectionType.NormalCylindrical &&
                position.HasValue &&
                IsOutsideViewport(position.Value))
            {
                position = parentMap.LocationToView(
                    new Location(location.Latitude, parentMap.ConstrainedLongitude(location.Longitude)));
            }

            return position;
        }

        protected ViewRect? GetViewRect(BoundingBox boundingBox)
        {
            var rect = parentMap.MapProjection.BoundingBoxToMap(boundingBox);

            if (!rect.HasValue)
            {
                return null;
            }

            return GetViewRect(rect.Value);
        }

        protected ViewRect GetViewRect(Rect rect)
        {
            var rectCenter = new Point(rect.X + rect.Width / 2d, rect.Y + rect.Height / 2d);
            var position = parentMap.ViewTransform.MapToView(rectCenter);

            if (parentMap.MapProjection.Type <= MapProjectionType.NormalCylindrical &&
                IsOutsideViewport(position))
            {
                var location = parentMap.MapProjection.MapToLocation(rectCenter);

                if (location != null)
                {
                    var pos = parentMap.LocationToView(
                        new Location(location.Latitude, parentMap.ConstrainedLongitude(location.Longitude)));

                    if (pos.HasValue)
                    {
                        position = pos.Value;
                    }
                }
            }

            var width = rect.Width * parentMap.ViewTransform.Scale;
            var height = rect.Height * parentMap.ViewTransform.Scale;
            var x = position.X - width / 2d;
            var y = position.Y - height / 2d;

            return new ViewRect(x, y, width, height, parentMap.ViewTransform.Rotation);
        }

        private bool IsOutsideViewport(Point point)
        {
            return point.X < 0d || point.X > parentMap.RenderSize.Width
                || point.Y < 0d || point.Y > parentMap.RenderSize.Height;
        }

        private static void ArrangeElement(FrameworkElement element, Point position)
        {
            var size = GetDesiredSize(element);
            var x = position.X;
            var y = position.Y;

            switch (element.HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    x -= size.Width / 2d;
                    break;

                case HorizontalAlignment.Right:
                    x -= size.Width;
                    break;

                default:
                    break;
            }

            switch (element.VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    y -= size.Height / 2d;
                    break;

                case VerticalAlignment.Bottom:
                    y -= size.Height;
                    break;

                default:
                    break;
            }

            ArrangeElement(element, new Rect(x, y, size.Width, size.Height));
        }

        private static void ArrangeElement(FrameworkElement element, Size parentSize)
        {
            var size = GetDesiredSize(element);
            var x = 0d;
            var y = 0d;
            var width = size.Width;
            var height = size.Height;

            switch (element.HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    x = (parentSize.Width - size.Width) / 2d;
                    break;

                case HorizontalAlignment.Right:
                    x = parentSize.Width - size.Width;
                    break;

                case HorizontalAlignment.Stretch:
                    width = parentSize.Width;
                    break;

                default:
                    break;
            }

            switch (element.VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    y = (parentSize.Height - size.Height) / 2d;
                    break;

                case VerticalAlignment.Bottom:
                    y = parentSize.Height - size.Height;
                    break;

                case VerticalAlignment.Stretch:
                    height = parentSize.Height;
                    break;

                default:
                    break;
            }

            ArrangeElement(element, new Rect(x, y, width, height));
        }

        private static void ArrangeElement(FrameworkElement element, ViewRect rect)
        {
            element.Width = rect.Rect.Width;
            element.Height = rect.Rect.Height;

            ArrangeElement(element, rect.Rect);

            if (element.RenderTransform is RotateTransform rotateTransform)
            {
                rotateTransform.Angle = rect.Rotation;
            }
            else if (rect.Rotation != 0d)
            {
                rotateTransform = new RotateTransform { Angle = rect.Rotation };
                element.RenderTransform = rotateTransform;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        private static void ArrangeElement(FrameworkElement element, Rect rect)
        {
            if (element.UseLayoutRounding)
            {
                rect = new Rect(Math.Round(rect.X), Math.Round(rect.Y), Math.Round(rect.Width), Math.Round(rect.Height));
            }

            element.Arrange(rect);
        }

        internal static Size GetDesiredSize(UIElement element)
        {
            var width = 0d;
            var height = 0d;

            if (element.DesiredSize.Width >= 0d &&
                element.DesiredSize.Width < double.PositiveInfinity)
            {
                width = element.DesiredSize.Width;
            }

            if (element.DesiredSize.Height >= 0d &&
                element.DesiredSize.Height < double.PositiveInfinity)
            {
                height = element.DesiredSize.Height;
            }

            return new Size(width, height);
        }
    }
}
