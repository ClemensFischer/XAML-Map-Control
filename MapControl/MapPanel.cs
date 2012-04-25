using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapControl
{
    public class MapPanel : Panel, INotifyParentMapChanged
    {
        public static readonly DependencyProperty ParentMapProperty = DependencyProperty.RegisterAttached(
            "ParentMap", typeof(Map), typeof(MapPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, ParentMapPropertyChanged));

        public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached(
            "Location", typeof(Point?), typeof(MapPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, LocationPropertyChanged));

        internal static readonly DependencyPropertyKey ViewPositionPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "ViewPosition", typeof(Point), typeof(MapPanel), null);

        private static readonly DependencyPropertyKey ViewPositionTransformPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "ViewPositionTransform", typeof(Transform), typeof(MapPanel), null);

        public static readonly DependencyProperty ViewPositionProperty = ViewPositionPropertyKey.DependencyProperty;
        public static readonly DependencyProperty ViewPositionTransformProperty = ViewPositionTransformPropertyKey.DependencyProperty;

        public MapPanel()
        {
            ClipToBounds = true;
        }

        public Map ParentMap
        {
            get { return (Map)GetValue(ParentMapProperty); }
        }

        public static Map GetParentMap(UIElement element)
        {
            return (Map)element.GetValue(ParentMapProperty);
        }

        public static Point? GetLocation(UIElement element)
        {
            return (Point?)element.GetValue(LocationProperty);
        }

        public static void SetLocation(UIElement element, Point? value)
        {
            element.SetValue(LocationProperty, value);
        }

        public static Point GetViewPosition(UIElement element)
        {
            return (Point)element.GetValue(ViewPositionProperty);
        }

        public static Transform GetViewPositionTransform(UIElement element)
        {
            return (Transform)element.GetValue(ViewPositionTransformProperty);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (UIElement element in InternalChildren)
            {
                element.Measure(infiniteSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement element in InternalChildren)
            {
                object viewPosition = element.ReadLocalValue(ViewPositionProperty);

                if (viewPosition == DependencyProperty.UnsetValue ||
                    !ArrangeElement(element, (Point)viewPosition))
                {
                    ArrangeElement(element, finalSize);
                }
            }

            return finalSize;
        }

        protected virtual void OnViewTransformChanged(Map parentMap)
        {
            foreach (UIElement element in InternalChildren)
            {
                Point? location = GetLocation(element);

                if (location.HasValue)
                {
                    SetViewPosition(element, parentMap, location);
                }
            }
        }

        private void OnViewTransformChanged(object sender, EventArgs eventArgs)
        {
            OnViewTransformChanged((Map)sender);
        }

        void INotifyParentMapChanged.ParentMapChanged(Map oldParentMap, Map newParentMap)
        {
            if (oldParentMap != null && oldParentMap != this)
            {
                oldParentMap.ViewTransformChanged -= OnViewTransformChanged;
            }

            if (newParentMap != null && newParentMap != this)
            {
                newParentMap.ViewTransformChanged += OnViewTransformChanged;
            }
        }

        private static void ParentMapPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs)
        {
            INotifyParentMapChanged notifyChanged = obj as INotifyParentMapChanged;

            if (notifyChanged != null)
            {
                notifyChanged.ParentMapChanged(eventArgs.OldValue as Map, eventArgs.NewValue as Map);
            }
        }

        private static void LocationPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs)
        {
            UIElement element = (UIElement)obj;
            Point? location = (Point?)eventArgs.NewValue;
            Map parentMap;

            if (location.HasValue && (parentMap = Map.GetParentMap(element)) != null)
            {
                SetViewPosition(element, parentMap, location);
            }
            else
            {
                element.ClearValue(ViewPositionPropertyKey);
                element.ClearValue(ViewPositionTransformPropertyKey);
                element.Arrange(new Rect());
            }
        }

        private static void SetViewPosition(UIElement element, Map parentMap, Point? location)
        {
            Point viewPosition = parentMap.MapViewTransform.Transform(location.Value);

            element.SetValue(ViewPositionPropertyKey, viewPosition);

            Matrix matrix = new Matrix(1d, 0d, 0d, 1d, viewPosition.X, viewPosition.Y);
            MatrixTransform viewTransform = element.GetValue(ViewPositionTransformProperty) as MatrixTransform;

            if (viewTransform != null)
            {
                viewTransform.Matrix = matrix;
            }
            else
            {
                element.SetValue(ViewPositionTransformPropertyKey, new MatrixTransform(matrix));
            }

            ArrangeElement(element, viewPosition);
        }

        private static bool ArrangeElement(UIElement element, Point position)
        {
            Rect rect = new Rect(position, element.DesiredSize);
            FrameworkElement frameworkElement = element as FrameworkElement;

            if (frameworkElement != null)
            {
                if (frameworkElement.HorizontalAlignment == HorizontalAlignment.Stretch &&
                    frameworkElement.VerticalAlignment == VerticalAlignment.Stretch)
                {
                    return false; // do not arrange at position
                }

                switch (frameworkElement.HorizontalAlignment)
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

                switch (frameworkElement.VerticalAlignment)
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
            }

            element.Arrange(rect);
            return true;
        }

        private static void ArrangeElement(UIElement element, Size panelSize)
        {
            Rect rect = new Rect(element.DesiredSize);
            FrameworkElement frameworkElement = element as FrameworkElement;

            if (frameworkElement != null)
            {
                switch (frameworkElement.HorizontalAlignment)
                {
                    case HorizontalAlignment.Center:
                        rect.X = (panelSize.Width - rect.Width) / 2d;
                        break;
                    case HorizontalAlignment.Right:
                        rect.X = panelSize.Width - rect.Width;
                        break;
                    case HorizontalAlignment.Stretch:
                        rect.Width = panelSize.Width;
                        break;
                    default:
                        break;
                }

                switch (frameworkElement.VerticalAlignment)
                {
                    case VerticalAlignment.Center:
                        rect.Y = (panelSize.Height - rect.Height) / 2d;
                        break;
                    case VerticalAlignment.Bottom:
                        rect.Y = panelSize.Height - rect.Height;
                        break;
                    case VerticalAlignment.Stretch:
                        rect.Height = panelSize.Height;
                        break;
                    default:
                        break;
                }
            }

            element.Arrange(rect);
        }
    }
}
