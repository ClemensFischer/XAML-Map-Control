#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    public class WmtsTileMatrix(
        string identifier, double scale, Point topLeft,
        int tileWidth, int tileHeight, int matrixWidth, int matrixHeight)
    {
        public string Identifier { get; } = identifier;
        public double Scale { get; } = scale;
        public Point TopLeft { get; } = topLeft;
        public int TileWidth { get; } = tileWidth;
        public int TileHeight { get; } = tileHeight;
        public int MatrixWidth { get; } = matrixWidth;
        public int MatrixHeight { get; } = matrixHeight;
    }
}
