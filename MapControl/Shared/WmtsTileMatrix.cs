#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    // See 07-057r7_Web_Map_Tile_Service_Standard.pdf, section 6.1.a, page 8:
    // "standardized rendering pixel size" is 0.28 mm
    //
    public class WmtsTileMatrix(
        string identifier, double scaleDenominator, Point topLeft,
        int tileWidth, int tileHeight, int matrixWidth, int matrixHeight)
    {
        public string Identifier { get; } = identifier;
        public double Scale { get; } = 1 / (scaleDenominator * 0.00028); // 0.28 mm
        public Point TopLeft { get; } = topLeft;
        public int TileWidth { get; } = tileWidth;
        public int TileHeight { get; } = tileHeight;
        public int MatrixWidth { get; } = matrixWidth;
        public int MatrixHeight { get; } = matrixHeight;
    }
}
