using System;
using System.Collections.Generic;
using System.Linq;

namespace MapControl
{
    public class WmtsTileMatrixSet
    {
        public WmtsTileMatrixSet(string identifier, string supportedCrsId, IEnumerable<WmtsTileMatrix> tileMatrixes)
        {
            if (string.IsNullOrEmpty(supportedCrsId))
            {
                throw new ArgumentException($"The {nameof(supportedCrsId)} argument must not be null or empty.", nameof(supportedCrsId));
            }

            if (tileMatrixes == null || !tileMatrixes.Any())
            {
                throw new ArgumentException($"The {nameof(tileMatrixes)} argument must not be null or an empty collection.", nameof(tileMatrixes));
            }

            Identifier = identifier;
            SupportedCrsId = supportedCrsId;
            TileMatrixes = tileMatrixes.OrderBy(m => m.Scale).ToList();
        }

        public string Identifier { get; }
        public string SupportedCrsId { get; }
        public IList<WmtsTileMatrix> TileMatrixes { get; }
    }
}
