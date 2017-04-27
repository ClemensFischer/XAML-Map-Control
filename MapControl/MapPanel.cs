// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2017 Clemens Fischer
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
    public interface IMapElement
    {
        MapBase ParentMap { get; set; }
    }

    /// <summary>
    /// Arranges child elements on a Map at positions specified by the attached property Location,
    /// or in rectangles specified by the attached property BoundingBox.
    /// An element's viewport position is assigned as TranslateTransform to its RenderTransform property,
    /// either directly or as last child of a TransformGroup.
    /// </summary>
    public partial class MapPanel : Panel, IMapElement
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached(
            "Location", typeof(Location), typeof(MapPanel), new PropertyMetadata(null, LocationPropertyChanged));

        public static readonly DependencyProperty BoundingBoxProperty = DependencyProperty.RegisterAttached(
            "BoundingBox", typeof(BoundingBox), typeof(MapPanel), new PropertyMetadata(null, BoundingBoxPropertyChanged));

        public static Location GetLocation(UIElement element)
        {
            return (Location)element.GetValue(LocationProperty);
        }

        public static void SetLocation(UIElement element, Location value)
        {
            element.SetValue(LocationProperty, value);
        }

        public static BoundingBox GetBoundingBox(UIElement element)
        {
            return (BoundingBox)element.GetValue(BoundingBoxProperty);
        }

        public static void SetBoundingBox(UIElement element, BoundingBox value)
        {
            element.SetValue(BoundingBoxProperty, value);
        }

        private MapBase parentMap;

        public MapBase ParentMap
        {
            get { return parentMap; }
            set { SetParentMapOverride(value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (UIElement element in Children)
            {
                element.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement element in Children)
            {
                BoundingBox boundingBox;
                Location location;

                if ((boundingBox = GetBoundingBox(element)) != null)
                {
                    ArrangeElementWithBoundingBox(element);
                    SetBoundingBoxRect(element, parentMap, boundingBox);
                }
                else if ((location = GetLocation(element)) != null)
                {
                    ArrangeElementWithLocation(element);
                    SetViewportPosition(element, parentMap, location);
                }
                else
                {
                    ArrangeElement(element, finalSize);
                }
            }

            return finalSize;
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
                OnViewportChanged(new ViewportChangedEventArgs());
            }
        }

        protected virtual void OnViewportChanged(ViewportChangedEventArgs e)
        {
            foreach (UIElement element in Children)
            {
                BoundingBox boundingBox;
                Location location;

                if ((boundingBox = GetBoundingBox(element)) != null)
                {
                    SetBoundingBoxRect(element, parentMap, boundingBox);
                }
                else if ((location = GetLocation(element)) != null)
                {
                    SetViewportPosition(element, parentMap, location);
                }
            }
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            OnViewportChanged(e);
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
            var element = (UIElement)obj;
            var map = GetParentMap(element);
            var location = (Location)e.NewValue;

            if (location == null)
            {
                ArrangeElement(element, map?.RenderSize ?? new Size());
            }
            else if (e.OldValue == null)
            {
                ArrangeElementWithLocation(element); // arrange once when Location was null before
            }

            SetViewportPosition(element, map, location);
        }

        private static void BoundingBoxPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)obj;
            var map = GetParentMap(element);
            var boundingBox = (BoundingBox)e.NewValue;

            if (boundingBox == null)
            {
                ArrangeElement(element, map?.RenderSize ?? new Size());
            }
            else if (e.OldValue == null)
            {
                ArrangeElementWithBoundingBox(element); // arrange once when BoundingBox was null before
            }

            SetBoundingBoxRect(element, map, boundingBox);
        }

        private static void SetViewportPosition(UIElement element, MapBase parentMap, Location location)
        {
            var viewportPosition = new Point();

            if (parentMap != null && location != null)
            {
                viewportPosition = parentMap.MapProjection.LocationToViewportPoint(location);

                if (viewportPosition.X < 0d || viewportPosition.X > parentMap.RenderSize.Width ||
                    viewportPosition.Y < 0d || viewportPosition.Y > parentMap.RenderSize.Height)
                {
                    viewportPosition = parentMap.MapProjection.LocationToViewportPoint(new Location(
                        location.Latitude,
                        Location.NearestLongitude(location.Longitude, parentMap.Center.Longitude)));
                }

                if ((bool)element.GetValue(UseLayoutRoundingProperty))
                {
                    viewportPosition.X = Math.Round(viewportPosition.X);
                    viewportPosition.Y = Math.Round(viewportPosition.Y);
                }
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

        private static void SetBoundingBoxRect(UIElement element, MapBase parentMap, BoundingBox boundingBox)
        {
            var rotation = 0d;
            var viewportPosition = new Point();

            if (parentMap != null && boundingBox != null)
            {
                var projection = parentMap.MapProjection;
                var rect = projection.BoundingBoxToRect(boundingBox);
                var center = new Point(rect.X + rect.Width / 2d, rect.Y + rect.Height / 2d);

                rotation = parentMap.Heading;
                viewportPosition = projection.ViewportTransform.Transform(center);

                if (viewportPosition.X < 0d || viewportPosition.X > parentMap.RenderSize.Width ||
                    viewportPosition.Y < 0d || viewportPosition.Y > parentMap.RenderSize.Height)
                {
                    var location = projection.PointToLocation(center);
                    location.Longitude = Location.NearestLongitude(location.Longitude, parentMap.Center.Longitude);

                    viewportPosition = projection.LocationToViewportPoint(location);
                }

                var width = rect.Width * projection.ViewportScale;
                var height = rect.Height * projection.ViewportScale;

                var fElement = element as FrameworkElement;
                if (fElement != null)
                {
                    fElement.Width = width;
                    fElement.Height = height;
                }
                else
                {
                    element.Arrange(new Rect(-width / 2d, -height / 2d, width, height));
                }
            }

            var transformGroup = element.RenderTransform as TransformGroup;
            RotateTransform rotateTransform;
            TranslateTransform translateTransform;

            if (transformGroup == null ||
                transformGroup.Children.Count != 2 ||
                (rotateTransform = transformGroup.Children[0] as RotateTransform) == null ||
                (translateTransform = transformGroup.Children[1] as TranslateTransform) == null)
            {
                transformGroup = new TransformGroup();
                rotateTransform = new RotateTransform();
                translateTransform = new TranslateTransform();
                transformGroup.Children.Add(rotateTransform);
                transformGroup.Children.Add(translateTransform);

                element.RenderTransform = transformGroup;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            rotateTransform.Angle = rotation;
            translateTransform.X = viewportPosition.X;
            translateTransform.Y = viewportPosition.Y;
        }

        private static void ArrangeElementWithBoundingBox(UIElement element)
        {
            var size = element.DesiredSize;

            element.Arrange(new Rect(-size.Width / 2d, -size.Height / 2d, size.Width, size.Height));
        }

        private static void ArrangeElementWithLocation(UIElement element)
        {
            var rect = new Rect(new Point(), element.DesiredSize);
            var fElement = element as FrameworkElement;

            if (fElement != null)
            {
                switch (fElement.HorizontalAlignment)
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

                switch (fElement.VerticalAlignment)
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

        private static void ArrangeElement(UIElement element, Size parentSize)
        {
            var rect = new Rect(new Point(), element.DesiredSize);
            var fElement = element as FrameworkElement;

            if (fElement != null)
            {
                switch (fElement.HorizontalAlignment)
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

                switch (fElement.VerticalAlignment)
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
