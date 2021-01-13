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
    }
}
