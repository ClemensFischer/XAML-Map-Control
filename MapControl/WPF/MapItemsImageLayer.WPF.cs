// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

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

        Drawing GetDrawing(IList<Point> positions, double scale, double rotation);
    }

    public class MapItemsImageLayer : MapImageLayer
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable<IMapDrawingItem>), typeof(MapItemsImageLayer));

        public IEnumerable<IMapDrawingItem> ItemsSource
        {
            get { return (IEnumerable<IMapDrawingItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        protected override async Task<ImageSource> GetImageAsync()
        {
            ImageSource image = null;
            var projection = ParentMap?.MapProjection;
            var items = ItemsSource;

            if (projection != null && items != null)
            {
                image = await Task.Run(() => GetImage(projection, items));
            }

            return image;
        }

        private DrawingImage GetImage(MapProjection projection, IEnumerable<IMapDrawingItem> items)
        {
            var scale = ParentMap.ViewTransform.Scale;
            var rotation = ParentMap.ViewTransform.Rotation;
            var mapRect = projection.BoundingBoxToRect(BoundingBox);
            var drawings = new DrawingGroup();

            foreach (var item in items)
            {
                var positions = item.Locations.Select(l => projection.LocationToMap(l)).ToList();

                if (positions.Any(p => mapRect.Contains(p)))
                {
                    for (int i = 0; i < positions.Count; i++)
                    {
                        positions[i] = new Point(
                            scale * (positions[i].X - mapRect.X),
                            scale * (mapRect.Height + mapRect.Y - positions[i].Y));
                    }

                    drawings.Children.Add(item.GetDrawing(positions, scale, rotation));
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
