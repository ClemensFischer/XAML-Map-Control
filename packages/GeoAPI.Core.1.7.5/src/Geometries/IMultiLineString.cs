using System;

namespace GeoAPI.Geometries
{
    public interface IMultiLineString : IMultiCurve, ILineal
    {
        [Obsolete("Use IGeometry.Reverse()")]
        new IMultiLineString Reverse();
    }
}
