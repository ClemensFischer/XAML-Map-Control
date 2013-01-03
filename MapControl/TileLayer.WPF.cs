// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public partial class TileLayer : DrawingVisual
    {
        partial void Initialize()
        {
            VisualTransform = transform;
            VisualEdgeMode = EdgeMode.Aliased;
        }

        protected ContainerVisual TileContainer
        {
            get { return Parent as ContainerVisual; }
        }

        protected void RenderTiles()
        {
            //System.Diagnostics.Trace.TraceInformation("{0} Tiles: {1}", tiles.Count, string.Join(", ", tiles.Select(t => t.ZoomLevel.ToString())));

            using (var drawingContext = RenderOpen())
            {
                foreach (var tile in tiles)
                {
                    var tileSize = 256 << (zoomLevel - tile.ZoomLevel);
                    var tileRect = new Rect(tileSize * tile.X - 256 * grid.X, tileSize * tile.Y - 256 * grid.Y, tileSize, tileSize);

                    drawingContext.DrawRectangle(tile.Brush, null, tileRect);

                    //if (tile.ZoomLevel == zoomLevel)
                    //    drawingContext.DrawText(new FormattedText(string.Format("{0}-{1}-{2}", tile.ZoomLevel, tile.X, tile.Y),
                    //        System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 14, Brushes.Black), tileRect.TopLeft);
                }
            }
        }
    }
}
