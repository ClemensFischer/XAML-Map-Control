// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;

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
                throw new ArgumentException("The identifier argument must not be null or empty.", nameof(identifier));
            }

            if (string.IsNullOrEmpty(supportedCrs))
            {
                throw new ArgumentException("The supportedCrs argument must not be null or empty.", nameof(supportedCrs));
            }

            if (tileMatrixes == null || !tileMatrixes.Any())
            {
                throw new ArgumentException("The tileMatrixes argument must not be null or an empty collection.", nameof(tileMatrixes));
            }

            Identifier = identifier;
            SupportedCrs = supportedCrs;
            TileMatrixes = tileMatrixes.OrderBy(m => m.Scale).ToList();
        }
    }
}
