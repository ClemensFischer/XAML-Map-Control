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
    /// Arranges child elements on a Map at positions specified by the attached property Location,
    /// or in rectangles specified by the attached property BoundingBox.
    /// </summary>
    public partial class MapPanel : Panel, IMapElement
    {
        private MapBase parentMap;

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

        public MapBase ParentMap
        {
            get { return parentMap; }
            set { SetParentMap(value); }
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
                        var viewPosition = ArrangeElement(element, location);

                        SetViewPosition(element, viewPosition);
                    }
                    else
                    {
                        var boundingBox = GetBoundingBox(element);

                        if (boundingBox != null)
                        {
                            ArrangeElement(element, boundingBox);
                        }
                        else
                        {
                            ArrangeElement(element, finalSize);
                        }

                        SetViewPosition(element, null);
                    }
                }
            }

            return finalSize;
        }

        private Point ArrangeElement(FrameworkElement element, Location location)
        {
            var pos = parentMap.LocationToView(location);

            if (parentMap.MapProjection.IsNormalCylindrical &&
                (pos.X < 0d || pos.X > parentMap.RenderSize.Width ||
                 pos.Y < 0d || pos.Y > parentMap.RenderSize.Height))
            {
                pos = parentMap.LocationToView(new Location(
                    location.Latitude,
                    Location.NearestLongitude(location.Longitude, parentMap.Center.Longitude)));
            }

            var rect = new Rect(pos, element.DesiredSize);

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

            if (element.UseLayoutRounding)
            {
                rect.X = Math.Round(rect.X);
                rect.Y = Math.Round(rect.Y);
            }

            element.Arrange(rect);
            return pos;
        }

        private void ArrangeElement(FrameworkElement element, BoundingBox boundingBox)
        {
            var projection = parentMap.MapProjection;
            var rect = projection.BoundingBoxToRect(boundingBox);
            var center = new Point(rect.X + rect.Width / 2d, rect.Y + rect.Height / 2d);
            var pos = parentMap.ViewTransform.MapToView(center);

            if (projection.IsNormalCylindrical &&
                (pos.X < 0d || pos.X > parentMap.RenderSize.Width ||
                 pos.Y < 0d || pos.Y > parentMap.RenderSize.Height))
            {
                var location = projection.MapToLocation(center);
                location.Longitude = Location.NearestLongitude(location.Longitude, parentMap.Center.Longitude);

                pos = parentMap.LocationToView(location);
            }

            rect.Width *= parentMap.ViewTransform.Scale;
            rect.Height *= parentMap.ViewTransform.Scale;
            rect.X = pos.X - rect.Width / 2d;
            rect.Y = pos.Y - rect.Height / 2d;

            if (element.UseLayoutRounding)
            {
                rect.X = Math.Round(rect.X);
                rect.Y = Math.Round(rect.Y);
            }

            element.Width = rect.Width;
            element.Height = rect.Height;
            element.Arrange(rect);

            if (element.RenderTransform is RotateTransform rotateTransform)
            {
                rotateTransform.Angle = parentMap.ViewTransform.Rotation;
            }
            else if (parentMap.ViewTransform.Rotation != 0d)
            {
                rotateTransform = new RotateTransform { Angle = parentMap.ViewTransform.Rotation };
                element.RenderTransform = rotateTransform;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }

        private void ArrangeElement(FrameworkElement element, Size parentSize)
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

            if (element.UseLayoutRounding)
            {
                rect.X = Math.Round(rect.X);
                rect.Y = Math.Round(rect.Y);
            }

            element.Arrange(rect);
        }

        private static void ParentMapPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is IMapElement mapElement)
            {
                mapElement.ParentMap = e.NewValue as MapBase;
            }
        }
    }
}
