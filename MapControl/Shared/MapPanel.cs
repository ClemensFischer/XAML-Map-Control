// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Linq;
#if WINDOWS_UWP
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
    /// Optional interface to hold the value of the MapPanel.ParentMap attached property.
    /// </summary>
    public interface IMapElement
    {
        MapBase ParentMap { get; set; }
    }

    /// <summary>
    /// Rotated rectangle used to arrange and rotate an element with a BoundingBox.
    /// </summary>
    public struct ViewRect
    {
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }
        public double Rotation { get; }

        public ViewRect(double x, double y, double width, double height, double rotation)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Rotation = rotation;
        }
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

        public MapBase ParentMap
        {
            get { return parentMap; }
            set { SetParentMap(value); }
        }

        public MapPanel()
        {
            InitMapElement(this);
        }

        public static Location GetLocation(FrameworkElement element)
        {
            return (Location)element.GetValue(LocationProperty);
        }

        public static void SetLocation(FrameworkElement element, Location value)
        {
            element.SetValue(LocationProperty, value);
        }

        public static BoundingBox GetBoundingBox(FrameworkElement element)
        {
            return (BoundingBox)element.GetValue(BoundingBoxProperty);
        }

        public static void SetBoundingBox(FrameworkElement element, BoundingBox value)
        {
            element.SetValue(BoundingBoxProperty, value);
        }

        public static Point? GetViewPosition(FrameworkElement element)
        {
            return (Point?)element.GetValue(ViewPositionProperty);
        }

        /// <summary>
        /// Returns the view position of a Location.
        /// </summary>
        public Point GetViewPosition(Location location)
        {
            var pos = parentMap.LocationToView(location);

            if (parentMap.MapProjection.IsNormalCylindrical &&
                (pos.X < 0d || pos.X > parentMap.RenderSize.Width ||
                 pos.Y < 0d || pos.Y > parentMap.RenderSize.Height))
            {
                location = new Location(location.Latitude, parentMap.ConstrainedLongitude(location.Longitude));

                pos = parentMap.LocationToView(location);
            }

            return pos;
        }

        /// <summary>
        /// Returns the potentially rotated view rectangle of a BoundingBox.
        /// </summary>
        public ViewRect GetViewRectangle(BoundingBox boundingBox)
        {
            return GetViewRectangle(parentMap.MapProjection.BoundingBoxToRect(boundingBox));
        }

        /// <summary>
        /// Returns the potentially rotated view rectangle of a map coordinate rectangle.
        /// </summary>
        public ViewRect GetViewRectangle(Rect rect)
        {
            var center = new Point(rect.X + rect.Width / 2d, rect.Y + rect.Height / 2d);
            var pos = parentMap.ViewTransform.MapToView(center);

            if (parentMap.MapProjection.IsNormalCylindrical &&
                (pos.X < 0d || pos.X > parentMap.RenderSize.Width ||
                 pos.Y < 0d || pos.Y > parentMap.RenderSize.Height))
            {
                var location = parentMap.MapProjection.MapToLocation(center);
                location.Longitude = parentMap.ConstrainedLongitude(location.Longitude);

                pos = parentMap.LocationToView(location);
            }

            var width = rect.Width * parentMap.ViewTransform.Scale;
            var height = rect.Height * parentMap.ViewTransform.Scale;
            var x = pos.X - width / 2d;
            var y = pos.Y - height / 2d;

            return new ViewRect(x, y, width, height, parentMap.ViewTransform.Rotation);
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

                    if (location != null)
                    {
                        var position = GetViewPosition(location);

                        SetViewPosition(element, position);
                        ArrangeElement(element, position);
                    }
                    else
                    {
                        SetViewPosition(element, null);

                        var boundingBox = GetBoundingBox(element);

                        if (boundingBox != null)
                        {
                            ArrangeElement(element, GetViewRectangle(boundingBox));
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

        private static void ArrangeElement(FrameworkElement element, ViewRect rect)
        {
            element.Width = rect.Width;
            element.Height = rect.Height;

            ArrangeElement(element, new Rect(rect.X, rect.Y, rect.Width, rect.Height));

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

        private static void ArrangeElement(FrameworkElement element, Point position)
        {
            var rect = new Rect(position, element.DesiredSize);

            switch (element.HorizontalAlignment)
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

            switch (element.VerticalAlignment)
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

            ArrangeElement(element, rect);
        }

        private static void ArrangeElement(FrameworkElement element, Size parentSize)
        {
            var rect = new Rect(new Point(), element.DesiredSize);

            switch (element.HorizontalAlignment)
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

            switch (element.VerticalAlignment)
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

            ArrangeElement(element, rect);
        }

        private static void ArrangeElement(FrameworkElement element, Rect rect)
        {
            if (element.UseLayoutRounding)
            {
                rect.X = Math.Round(rect.X);
                rect.Y = Math.Round(rect.Y);
                rect.Width = Math.Round(rect.Width);
                rect.Height = Math.Round(rect.Height);
            }

            element.Arrange(rect);
        }
    }
}
