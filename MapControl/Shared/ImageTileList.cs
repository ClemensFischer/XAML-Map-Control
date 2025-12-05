using System.Collections.Generic;
using System.Linq;

namespace MapControl
{
    public class ImageTileList : List<ImageTile>
    {
        public ImageTileList()
        {
        }

        public ImageTileList(IEnumerable<ImageTile> source, TileMatrix tileMatrix, int columnCount)
            : base(tileMatrix.Width * tileMatrix.Height)
        {
            FillMatrix(source, tileMatrix.ZoomLevel, tileMatrix.XMin, tileMatrix.YMin, tileMatrix.XMax, tileMatrix.YMax, columnCount);
        }

        /// <summary>
        /// Adds existing ImageTile from the source collection or newly created ImageTile to fill the specified tile matrix.
        /// </summary>
        public void FillMatrix(IEnumerable<ImageTile> source, int zoomLevel, int xMin, int yMin, int xMax, int yMax, int columnCount)
        {
            for (var y = yMin; y <= yMax; y++)
            {
                for (var x = xMin; x <= xMax; x++)
                {
                    var tile = source.FirstOrDefault(t => t.ZoomLevel == zoomLevel && t.X == x && t.Y == y);

                    if (tile == null)
                    {
                        tile = new ImageTile(zoomLevel, x, y, columnCount);

                        var equivalentTile = source.FirstOrDefault(
                            t => t.Image.Source != null && t.ZoomLevel == tile.ZoomLevel && t.Column == tile.Column && t.Row == tile.Row);

                        if (equivalentTile != null)
                        {
                            tile.IsPending = false;
                            tile.Image.Source = equivalentTile.Image.Source; // no Opacity animation
                        }
                    }

                    Add(tile);
                }
            }
        }
    }
}
