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
        public static readonly DependencyProperty AutoCollapseProperty = DependencyProperty.RegisterAttached(
            "AutoCollapse", typeof(bool), typeof(MapPanel), new PropertyMetadata(false));

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
        /// Gets the geodetic Location of an element.
        /// </summary>
        public static Location GetLocation(FrameworkElement element)
        {
            return (Location)element.GetValue(LocationProperty);
        }

        /// <summary>
        /// Sets the geodetic Location of an element.
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
        /// Gets the position of an element in view coordinates.
        /// </summary>
        public static Point? GetViewPosition(FrameworkElement element)
        {
            return (Point?)element.GetValue(ViewPositionProperty);
        }

        /// <summary>
        /// Returns the view position of a Location.
        /// </summary>
        public Point? GetViewPosition(Location location)
        {
            if (location == null)
            {
                return null;
            }

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
        public ViewRect GetViewRect(BoundingBox boundingBox)
        {
            return GetViewRect(parentMap.MapProjection.BoundingBoxToRect(boundingBox));
        }

        /// <summary>
        /// Returns the potentially rotated view rectangle of a map coordinate rectangle.
        /// </summary>
        public ViewRect GetViewRect(Rect rect)
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
                    var position = GetViewPosition(GetLocation(element));

                    SetViewPosition(element, position);

                    if (GetAutoCollapse(element))
                    {
                        if (position.HasValue &&
                           (position.Value.X < 0d ||
                            position.Value.Y < 0d ||
                            position.Value.X > parentMap.RenderSize.Width ||
                            position.Value.Y > parentMap.RenderSize.Height))
                        {
                            element.SetValue(VisibilityProperty, Visibility.Collapsed);
                        }
                        else
                        {
                            element.ClearValue(VisibilityProperty);
                        }
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
                            ArrangeElement(element, GetViewRect(boundingBox));
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

        private static void ParentMapPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is IMapElement mapElement)
            {
                mapElement.ParentMap = e.NewValue as MapBase;
            }
        }
    }
}
