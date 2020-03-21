// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
using System.Xml.Linq;
#if !WINDOWS_UWP
using System.Windows;
#endif

namespace MapControl
{
    public class WmtsTileMatrix
    {
        public string Identifier { get; }
        public double Scale { get; }
        public Point TopLeft { get; }
        public int TileWidth { get; }
        public int TileHeight { get; }
        public int MatrixWidth { get; }
        public int MatrixHeight { get; }

        public WmtsTileMatrix(string identifier, double scaleDenominator, Point topLeft,
            int tileWidth, int tileHeight, int matrixWidth, int matrixHeight)
        {
            Identifier = identifier;
            Scale = 1 / (scaleDenominator * 0.00028);
            TopLeft = topLeft;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            MatrixWidth = matrixWidth;
            MatrixHeight = matrixHeight;
        }

        public static WmtsTileMatrix Create(XElement tileMatrixElement)
        {
            XNamespace ns = tileMatrixElement.Name.Namespace;
            XNamespace ows = "http://www.opengis.net/ows/1.1";

            var identifier = tileMatrixElement.Element(ows + "Identifier")?.Value;

            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("ows:Identifier element not found in TileMatrix.");
            }

            string[] topLeftCornerStrings;
            double scaleDenominator, top, left;
            int tileWidth, tileHeight, matrixWidth, matrixHeight;

            var valueString = tileMatrixElement.Element(ns + "ScaleDenominator")?.Value;

            if (string.IsNullOrEmpty(valueString) ||
                !double.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out scaleDenominator))
            {
                throw new ArgumentException("ScaleDenominator element not found in TileMatrix \"" + identifier + "\".");
            }

            valueString = tileMatrixElement.Element(ns + "TopLeftCorner")?.Value;

            if (string.IsNullOrEmpty(valueString) ||
                (topLeftCornerStrings = valueString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).Length < 2 ||
                !double.TryParse(topLeftCornerStrings[0], NumberStyles.Float, CultureInfo.InvariantCulture, out left) ||
                !double.TryParse(topLeftCornerStrings[1], NumberStyles.Float, CultureInfo.InvariantCulture, out top))
            {
                throw new ArgumentException("TopLeftCorner element not found in TileMatrix \"" + identifier + "\".");
            }

            valueString = tileMatrixElement.Element(ns + "TileWidth")?.Value;

            if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out tileWidth))
            {
                throw new ArgumentException("TileWidth element not found in TileMatrix \"" + identifier + "\".");
            }

            valueString = tileMatrixElement.Element(ns + "TileHeight")?.Value;

            if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out tileHeight))
            {
                throw new ArgumentException("TileHeight element not found in TileMatrix \"" + identifier + "\".");
            }

            valueString = tileMatrixElement.Element(ns + "MatrixWidth")?.Value;

            if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out matrixWidth))
            {
                throw new ArgumentException("MatrixWidth element not found in TileMatrix \"" + identifier + "\".");
            }

            valueString = tileMatrixElement.Element(ns + "MatrixHeight")?.Value;

            if (string.IsNullOrEmpty(valueString) || !int.TryParse(valueString, out matrixHeight))
            {
                throw new ArgumentException("MatrixHeight element not found in TileMatrix \"" + identifier + "\".");
            }

            return new WmtsTileMatrix(
                identifier, scaleDenominator, new Point(left, top), tileWidth, tileHeight, matrixWidth, matrixHeight);
        }
    }
}
