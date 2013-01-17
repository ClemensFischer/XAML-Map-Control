// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows.Media;

namespace MapControl
{
    internal partial class TileContainer : ContainerVisual
    {
        private Matrix GetTransformMatrix(Matrix transform, double scale)
        {
            transform.Scale(scale, scale);
            transform.Translate(offset.X, offset.Y);
            transform.RotateAt(rotation, origin.X, origin.Y);
            return transform;
        }

        private Matrix GetTileIndexMatrix(int numTiles)
        {
            var mapToTileScale = (double)numTiles / 360d;
            var transform = ViewportTransform.Matrix;
            transform.Invert(); // view to map coordinates
            transform.Translate(180d, -180d);
            transform.Scale(mapToTileScale, -mapToTileScale); // map coordinates to tile indices
            return transform;
        }
    }
}
