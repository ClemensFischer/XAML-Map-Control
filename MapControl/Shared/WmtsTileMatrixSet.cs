using System;
using System.Collections.Generic;
using System.Linq;

namespace MapControl
{
    public class WmtsTileMatrixSet
    {
        public WmtsTileMatrixSet(string identifier, string supportedMapProjection, IEnumerable<WmtsTileMatrix> tileMatrixes)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException($"The {nameof(identifier)} argument must not be null or empty.", nameof(identifier));
            }

            if (string.IsNullOrEmpty(supportedMapProjection))
            {
                throw new ArgumentException($"The {nameof(supportedMapProjection)} argument must not be null or empty.", nameof(supportedMapProjection));
            }

            if (tileMatrixes == null || !tileMatrixes.Any())
            {
                throw new ArgumentException($"The {nameof(tileMatrixes)} argument must not be null or an empty collection.", nameof(tileMatrixes));
            }

            Identifier = identifier;
            SupportedMapProjection = supportedMapProjection;
            TileMatrixes = tileMatrixes.OrderBy(m => m.Scale).ToList();
        }

        public string Identifier { get; }
        public string SupportedMapProjection { get; }
        public IList<WmtsTileMatrix> TileMatrixes { get; }
    }
}
