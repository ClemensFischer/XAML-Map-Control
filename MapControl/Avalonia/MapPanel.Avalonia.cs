// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace MapControl
{
    public class MapPanel : Panel
    {
        public static readonly AttachedProperty<MapBase> ParentMapProperty
            = AvaloniaProperty.RegisterAttached<MapPanel, AvaloniaObject, MapBase>("ParentMap", null, true);

        private static readonly AttachedProperty<Point?> ViewPositionProperty
            = AvaloniaProperty.RegisterAttached<MapPanel, AvaloniaObject, Point?>("ViewPosition");

        public static readonly AttachedProperty<Location> LocationProperty
            = AvaloniaProperty.RegisterAttached<MapPanel, AvaloniaObject, Location>("Location");

        public static readonly AttachedProperty<BoundingBox> BoundingBoxProperty
            = AvaloniaProperty.RegisterAttached<MapPanel, AvaloniaObject, BoundingBox>("BoundingBox");

        public static readonly AttachedProperty<bool> AutoCollapseProperty
            = AvaloniaProperty.RegisterAttached<MapPanel, AvaloniaObject, bool>("AutoCollapse");

        static MapPanel()
        {
            ParentMapProperty.Changed.AddClassHandler<MapPanel, MapBase>(
                (panel, args) => panel.OnParentMapPropertyChanged(args.NewValue.Value));
        }

        public MapPanel()
        {
            ClipToBounds = true;

            if (this is MapBase mapBase)
            {
                SetParentMap(this, mapBase);
            }
        }

        public MapBase ParentMap { get; private set; }

        public static MapBase GetParentMap(AvaloniaObject obj) => obj.GetValue(ParentMapProperty);

        private static void SetParentMap(AvaloniaObject obj, MapBase value) => obj.SetValue(ParentMapProperty, value);

        public static Point? GetViewPosition(AvaloniaObject obj) => obj.GetValue(ViewPositionProperty);

        private static void SetViewPosition(AvaloniaObject obj, Point? value) => obj.SetValue(ViewPositionProperty, value);

        public static Location GetLocation(AvaloniaObject obj) => obj.GetValue(LocationProperty);

        public static void SetLocation(AvaloniaObject obj, Location value) => obj.SetValue(LocationProperty, value);

        public static BoundingBox GetBoundingBox(AvaloniaObject obj) => obj.GetValue(BoundingBoxProperty);

        public static void SetBoundingBox(AvaloniaObject obj, BoundingBox value) => obj.SetValue(BoundingBoxProperty, value);

        public static bool GetAutoCollapse(AvaloniaObject obj) => obj.GetValue(AutoCollapseProperty);

        public static void SetAutoCollapse(AvaloniaObject obj, bool value) => obj.SetValue(AutoCollapseProperty, value);

        protected virtual void OnViewportChanged(ViewportChangedEventArgs e)
        {
            InvalidateArrange();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (var element in Children)
            {
                element.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (ParentMap != null)
            {
                foreach (var element in Children)
                {
                    var location = GetLocation(element);
                    var position = location != null ? GetViewPosition(location) : null;

                    SetViewPosition(element, position);

                    if (GetAutoCollapse(element))
                    {
                        element.IsVisible = !(position.HasValue && IsOutsideViewport(position.Value));
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
            var position = ParentMap.LocationToView(location);

            if (ParentMap.MapProjection.Type <= MapProjectionType.NormalCylindrical &&
                position.HasValue &&
                IsOutsideViewport(position.Value))
            {
                position = ParentMap.LocationToView(
                    new Location(location.Latitude, ParentMap.ConstrainedLongitude(location.Longitude)));
            }

            return position;
        }

        protected ViewRect? GetViewRect(BoundingBox boundingBox)
        {
            var rect = ParentMap.MapProjection.BoundingBoxToMap(boundingBox);

            if (!rect.HasValue)
            {
                return null;
            }

            return GetViewRect(rect.Value);
        }

        protected ViewRect GetViewRect(Rect mapRect)
        {
            var position = ParentMap.ViewTransform.MapToView(mapRect.Center);

            if (ParentMap.MapProjection.Type <= MapProjectionType.NormalCylindrical &&
                IsOutsideViewport(position))
            {
                var location = ParentMap.MapProjection.MapToLocation(mapRect.Center);

                if (location != null)
                {
                    var pos = ParentMap.LocationToView(
                        new Location(location.Latitude, ParentMap.ConstrainedLongitude(location.Longitude)));

                    if (pos.HasValue)
                    {
                        position = pos.Value;
                    }
                }
            }

            var width = mapRect.Width * ParentMap.ViewTransform.Scale;
            var height = mapRect.Height * ParentMap.ViewTransform.Scale;
            var x = position.X - width / 2d;
            var y = position.Y - height / 2d;

            return new ViewRect(x, y, width, height, ParentMap.ViewTransform.Rotation);
        }

        private bool IsOutsideViewport(Point point)
        {
            return point.X < 0d || point.X > ParentMap.Bounds.Width
                || point.Y < 0d || point.Y > ParentMap.Bounds.Height;
        }

        private static void ArrangeElement(Control element, Point position)
        {
            var size = GetDesiredSize(element);
            var x = position.X;
            var y = position.Y;

            switch (element.HorizontalAlignment)
            {
                case Avalonia.Layout.HorizontalAlignment.Center:
                    x -= size.Width / 2d;
                    break;

                case Avalonia.Layout.HorizontalAlignment.Right:
                    x -= size.Width;
                    break;

                default:
                    break;
            }

            switch (element.VerticalAlignment)
            {
                case Avalonia.Layout.VerticalAlignment.Center:
                    y -= size.Height / 2d;
                    break;

                case Avalonia.Layout.VerticalAlignment.Bottom:
                    y -= size.Height;
                    break;

                default:
                    break;
            }

            ArrangeElement(element, new Rect(x, y, size.Width, size.Height));
        }

        private static void ArrangeElement(Control element, Size parentSize)
        {
            var size = GetDesiredSize(element);
            var x = 0d;
            var y = 0d;
            var width = size.Width;
            var height = size.Height;

            switch (element.HorizontalAlignment)
            {
                case Avalonia.Layout.HorizontalAlignment.Center:
                    x = (parentSize.Width - size.Width) / 2d;
                    break;

                case Avalonia.Layout.HorizontalAlignment.Right:
                    x = parentSize.Width - size.Width;
                    break;

                case Avalonia.Layout.HorizontalAlignment.Stretch:
                    width = parentSize.Width;
                    break;

                default:
                    break;
            }

            switch (element.VerticalAlignment)
            {
                case Avalonia.Layout.VerticalAlignment.Center:
                    y = (parentSize.Height - size.Height) / 2d;
                    break;

                case Avalonia.Layout.VerticalAlignment.Bottom:
                    y = parentSize.Height - size.Height;
                    break;

                case Avalonia.Layout.VerticalAlignment.Stretch:
                    height = parentSize.Height;
                    break;

                default:
                    break;
            }

            ArrangeElement(element, new Rect(x, y, width, height));
        }

        private static void ArrangeElement(Control element, ViewRect rect)
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
                element.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            }
        }

        private static void ArrangeElement(Control element, Rect rect)
        {
            if (element.UseLayoutRounding)
            {
                rect = new Rect(Math.Round(rect.X), Math.Round(rect.Y), Math.Round(rect.Width), Math.Round(rect.Height));
            }

            element.Arrange(rect);
        }

        internal static Size GetDesiredSize(Control element)
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

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            OnViewportChanged(e);
        }

        private void OnParentMapPropertyChanged(MapBase parentMap)
        {
            if (ParentMap != null && ParentMap != this)
            {
                ParentMap.ViewportChanged -= OnViewportChanged;
            }

            ParentMap = parentMap;

            if (ParentMap != null && ParentMap != this)
            {
                ParentMap.ViewportChanged += OnViewportChanged;

                OnViewportChanged(new ViewportChangedEventArgs());
            }
        }
    }
}
