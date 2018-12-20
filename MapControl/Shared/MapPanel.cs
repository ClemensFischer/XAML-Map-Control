// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
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
        public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached(
            "Location", typeof(Location), typeof(MapPanel), new PropertyMetadata(null, LocationPropertyChanged));

        public static readonly DependencyProperty BoundingBoxProperty = DependencyProperty.RegisterAttached(
            "BoundingBox", typeof(BoundingBox), typeof(MapPanel), new PropertyMetadata(null, BoundingBoxPropertyChanged));

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

        public static Point? GetViewportPosition(FrameworkElement element)
        {
            return (Point?)element.GetValue(ViewportPositionProperty);
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
            foreach (var element in Children.OfType<FrameworkElement>())
            {
                Location location;
                BoundingBox boundingBox;
                Point? viewportPosition = null;

                if ((location = GetLocation(element)) != null)
                {
                    viewportPosition = ArrangeElementWithLocation(element, parentMap, location);
                }
                else if ((boundingBox = GetBoundingBox(element)) != null)
                {
                    ArrangeElementWithBoundingBox(element, parentMap, boundingBox);
                }
                else
                {
                    ArrangeElement(element, finalSize);
                }

                SetViewportPosition(element, viewportPosition);
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
            var element = (FrameworkElement)obj;
            var parentMap = GetParentMap(element);
            var location = (Location)e.NewValue;
            Point? viewportPosition = null;

            if (location != null)
            {
                viewportPosition = ArrangeElementWithLocation(element, parentMap, location);
            }
            else
            {
                ArrangeElement(element, parentMap?.RenderSize ?? new Size());
            }

            SetViewportPosition(element, viewportPosition);
        }

        private static void BoundingBoxPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)obj;
            var parentMap = GetParentMap(element);
            var boundingBox = (BoundingBox)e.NewValue;

            if (boundingBox != null)
            {
                ArrangeElementWithBoundingBox(element, parentMap, boundingBox);
            }
            else
            {
                ArrangeElement(element, parentMap?.RenderSize ?? new Size());
            }

            SetViewportPosition(element, null);
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

            if (element.UseLayoutRounding)
            {
                rect.X = Math.Round(rect.X);
                rect.Y = Math.Round(rect.Y);
            }

            element.Arrange(rect);
        }

        private static Point ArrangeElementWithLocation(FrameworkElement element, MapBase parentMap, Location location)
        {
            var pos = new Point();
            var rect = new Rect(pos, element.DesiredSize);

            if (parentMap != null)
            {
                var projection = parentMap.MapProjection;
                pos = projection.LocationToViewportPoint(location);

                if (projection.IsNormalCylindrical &&
                    (pos.X < 0d || pos.X > parentMap.RenderSize.Width ||
                     pos.Y < 0d || pos.Y > parentMap.RenderSize.Height))
                {
                    pos = projection.LocationToViewportPoint(new Location(
                        location.Latitude,
                        Location.NearestLongitude(location.Longitude, parentMap.Center.Longitude)));
                }

                rect.X = pos.X;
                rect.Y = pos.Y;
            }

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

        private static void ArrangeElementWithBoundingBox(FrameworkElement element, MapBase parentMap, BoundingBox boundingBox)
        {
            var rect = new Rect();
            var rotation = 0d;

            if (parentMap != null)
            {
                var projection = parentMap.MapProjection;
                rect = projection.BoundingBoxToRect(boundingBox);

                var center = new Point(rect.X + rect.Width / 2d, rect.Y + rect.Height / 2d);
                var pos = projection.ViewportTransform.Transform(center);

                if (projection.IsNormalCylindrical &&
                    (pos.X < 0d || pos.X > parentMap.RenderSize.Width ||
                     pos.Y < 0d || pos.Y > parentMap.RenderSize.Height))
                {
                    var location = projection.PointToLocation(center);
                    location.Longitude = Location.NearestLongitude(location.Longitude, parentMap.Center.Longitude);

                    pos = projection.LocationToViewportPoint(location);
                }

                rect.Width *= projection.ViewportScale;
                rect.Height *= projection.ViewportScale;
                rect.X = pos.X - rect.Width / 2d;
                rect.Y = pos.Y - rect.Height / 2d;

                if (element.UseLayoutRounding)
                {
                    rect.X = Math.Round(rect.X);
                    rect.Y = Math.Round(rect.Y);
                }

                rotation = parentMap.Heading;
            }

            element.Width = rect.Width;
            element.Height = rect.Height;
            element.Arrange(rect);

            var rotateTransform = element.RenderTransform as RotateTransform;

            if (rotateTransform != null)
            {
                rotateTransform.Angle = rotation;
            }
            else if (rotation != 0d)
            {
                rotateTransform = new RotateTransform { Angle = rotation };
                element.RenderTransform = rotateTransform;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }
    }
}
