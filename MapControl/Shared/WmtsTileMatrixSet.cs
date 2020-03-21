// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MapControl
{
    public class WmtsTileMatrixSet
    {
        public string Identifier { get; }
        public string SupportedCrs { get; }
        public IList<WmtsTileMatrix> TileMatrixes { get; }

        public WmtsTileMatrixSet(string identifier, string supportedCrs, IEnumerable<WmtsTileMatrix> tileMatrixes)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The parameter identifier must not be null or empty.");
            }

            if (string.IsNullOrEmpty(supportedCrs))
            {
                throw new ArgumentException("The parameter supportedCrs must not be null or empty.");
            }

            if (tileMatrixes == null || tileMatrixes.Count() <= 0)
            {
                throw new ArgumentException("The parameter tileMatrixes must not be null or an empty collection.");
            }

            Identifier = identifier;
            SupportedCrs = supportedCrs;
            TileMatrixes = tileMatrixes.OrderBy(m => m.Scale).ToList();
        }

        public static WmtsTileMatrixSet Create(XElement tileMatrixSetElement)
        {
            XNamespace ns = tileMatrixSetElement.Name.Namespace;
            XNamespace ows = "http://www.opengis.net/ows/1.1";

            var identifier = tileMatrixSetElement.Element(ows + "Identifier")?.Value;

            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("ows:Identifier element not found in TileMatrixSet.");
            }

            var supportedCrs = tileMatrixSetElement.Element(ows + "SupportedCRS")?.Value;

            if (string.IsNullOrEmpty(supportedCrs))
            {
                throw new ArgumentException("ows:SupportedCRS element not found in TileMatrixSet \"" + identifier + "\".");
            }

            var tileMatrixes = new List<WmtsTileMatrix>();

            foreach (var tileMatrixElement in tileMatrixSetElement.Descendants(ns + "TileMatrix"))
            {
                tileMatrixes.Add(WmtsTileMatrix.Create(tileMatrixElement));
            }

            if (tileMatrixes.Count <= 0)
            {
                throw new ArgumentException("No TileMatrix elements found in TileMatrixSet \"" + identifier + "\".");
            }

            return new WmtsTileMatrixSet(identifier, supportedCrs, tileMatrixes);
        }
    }
}
