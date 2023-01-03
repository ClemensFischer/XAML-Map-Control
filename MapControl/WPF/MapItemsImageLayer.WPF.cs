// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public interface IMapDrawingItem
    {
        IEnumerable<Location> Locations { get; }

        Drawing GetDrawing(IList<Point> points, double scale, double rotation);
    }

    public class MapItemsImageLayer : MapImageLayer
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable<IMapDrawingItem>), typeof(MapItemsImageLayer));

        public IEnumerable<IMapDrawingItem> ItemsSource
        {
            get => (IEnumerable<IMapDrawingItem>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        protected override async Task<ImageSource> GetImageAsync(BoundingBox boundingBox, IProgress<double> progress)
        {
            ImageSource image = null;
            var projection = ParentMap?.MapProjection;
            var items = ItemsSource;

            if (projection != null && items != null)
            {
                var mapRect = projection.BoundingBoxToMapRect(boundingBox);

                if (mapRect != null)
                {
                    image = await Task.Run(() => GetImage(projection, mapRect, items));
                }
            }

            return image;
        }

        private DrawingImage GetImage(MapProjection projection, MapRect mapRect, IEnumerable<IMapDrawingItem> items)
        {
            var scale = ParentMap.ViewTransform.Scale;
            var rotation = ParentMap.ViewTransform.Rotation;
            var drawings = new DrawingGroup();

            foreach (var item in items)
            {
                var points = item.Locations
                    .Select(location => projection.LocationToMap(location))
                    .Where(point => point.HasValue)
                    .Select(point => point.Value)
                    .ToList();

                if (points.Any(point => mapRect.Contains(point)))
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        points[i] = new Point(
                            scale * (points[i].X - mapRect.XMin),
                            scale * (mapRect.YMax - points[i].Y));
                    }

                    drawings.Children.Add(item.GetDrawing(points, scale, rotation));
                }
            }

            var drawingBrush = new DrawingBrush
            {
                Drawing = drawings,
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, scale * mapRect.Width, scale * mapRect.Height),
            };

            var drawing = new GeometryDrawing(
                drawingBrush, null, new RectangleGeometry(drawingBrush.Viewbox));

            var image = new DrawingImage(drawing);
            image.Freeze();

            return image;
        }
    }
}
