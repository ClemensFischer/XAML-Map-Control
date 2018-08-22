using System;

namespace GeoAPI.Geometries
{
#if HAS_SYSTEM_ICLONEABLE
    using ICloneable = System.ICloneable;
#else
    using ICloneable = GeoAPI.ICloneable;
#endif

    /// <summary>
    /// Interface for lightweight classes used to store coordinates on the 2-dimensional Cartesian plane.
    /// </summary>
    [Obsolete("Use Coordinate class instead")]
    public interface ICoordinate : 
        ICloneable,
        IComparable, IComparable<ICoordinate>
    {
        /// <summary>
        /// The x-ordinate value
        /// </summary>
        double X { get; set; }

        /// <summary>
        /// The y-ordinate value
        /// </summary>
        double Y { get; set; }
        
        /// <summary>
        /// The z-ordinate value
        /// </summary>
        double Z { get; set; }

        /// <summary>
        /// The measure value
        /// </summary>
        double M { get; set; }

        /// <summary>
        /// Gets or sets all ordinate values
        /// </summary>
        ICoordinate CoordinateValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Ordinate"/> value of this <see cref="ICoordinate"/>
        /// </summary>
        /// <param name="index">The <see cref="Ordinate"/> index</param>
        double this[Ordinate index] { get; set; }

        /// <summary>
        /// Computes the 2-dimensional distance to the <paramref name="other"/> coordiante.
        /// </summary>
        /// <param name="other">The other coordinate</param>
        /// <returns>The 2-dimensional distance to other</returns>
        double Distance(ICoordinate other);
        
        /// <summary>
        /// Compares equality for x- and y-ordinates
        /// </summary>
        /// <param name="other">The other coordinate</param>
        /// <returns><c>true</c> if x- and y-ordinates of this coordinate and <see paramref="other"/> coordiante are equal.</returns>
        bool Equals2D(ICoordinate other);

        /// <summary>
        /// Compares equality for x-, y- and z-ordinates
        /// </summary>
        /// <param name="other">The other coordinate</param>
        /// <returns><c>true</c> if x-, y- and z-ordinates of this coordinate and <see paramref="other"/> coordiante are equal.</returns>
        bool Equals3D(ICoordinate other);        
    }
}
