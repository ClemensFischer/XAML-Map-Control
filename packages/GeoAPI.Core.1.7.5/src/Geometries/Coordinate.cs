using System;
using System.Globalization;

namespace GeoAPI.Geometries
{
    /// <summary>
    /// A lightweight class used to store coordinates on the 2-dimensional Cartesian plane.
    /// <para>
    /// It is distinct from <see cref="IPoint"/>, which is a subclass of <see cref="IGeometry"/>.
    /// Unlike objects of type <see cref="IPoint"/> (which contain additional
    /// information such as an envelope, a precision model, and spatial reference
    /// system information), a <other>Coordinate</other> only contains ordinate values
    /// and propertied.
    /// </para>
    /// <para>
    /// <other>Coordinate</other>s are two-dimensional points, with an additional Z-ordinate.    
    /// If an Z-ordinate value is not specified or not defined,
    /// constructed coordinates have a Z-ordinate of <code>NaN</code>
    /// (which is also the value of <see cref="NullOrdinate"/>).
    /// </para>
    /// </summary>
    /// <remarks>
    /// Apart from the basic accessor functions, NTS supports
    /// only specific operations involving the Z-ordinate.
    /// </remarks>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
#pragma warning disable 612,618
    public class Coordinate : ICoordinate, IComparable<Coordinate>
#pragma warning restore 612,618
    {
        ///<summary>
        /// The value used to indicate a null or missing ordinate value.
        /// In particular, used for the value of ordinates for dimensions
        /// greater than the defined dimension of a coordinate.
        ///</summary>
        public const double NullOrdinate = Double.NaN;

        /// <summary>
        /// X coordinate.
        /// </summary>
        public double X; // = Double.NaN;
        /// <summary>
        /// Y coordinate.
        /// </summary>
        public double Y; // = Double.NaN;
        /// <summary>
        /// Z coordinate.
        /// </summary>
        public double Z; // = Double.NaN;

        /// <summary>
        /// Constructs a <other>Coordinate</other> at (x,y,z).
        /// </summary>
        /// <param name="x">X value.</param>
        /// <param name="y">Y value.</param>
        /// <param name="z">Z value.</param>
        public Coordinate(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets or sets the ordinate value for the given index.
        /// The supported values for the index are 
        /// <see cref="Ordinate.X"/>, <see cref="Ordinate.Y"/> and <see cref="Ordinate.Z"/>.
        /// </summary>
        /// <param name="ordinateIndex">The ordinate index</param>
        /// <returns>The ordinate value</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="ordinateIndex"/> is not in the valid range.</exception>
        public double this[Ordinate ordinateIndex]
        {
            get
            {
                switch (ordinateIndex)
                {
                    case Ordinate.X:
                        return X;
                    case Ordinate.Y:
                        return Y;
                    case Ordinate.Z:
                        return Z;
                }
                throw new ArgumentOutOfRangeException("ordinateIndex");
            }
            set
            {
                switch (ordinateIndex)
                {
                    case Ordinate.X:
                        X = value;
                        return;
                    case Ordinate.Y:
                        Y = value;
                        return;
                    case Ordinate.Z:
                        Z = value;
                        return;
                }
                throw new ArgumentOutOfRangeException("ordinateIndex");
            }
        }

        /// <summary>
        ///  Constructs a <other>Coordinate</other> at (0,0,NaN).
        /// </summary>
        public Coordinate() : this(0.0, 0.0, NullOrdinate) { }

        /// <summary>
        /// Constructs a <other>Coordinate</other> having the same (x,y,z) values as
        /// <other>other</other>.
        /// </summary>
        /// <param name="c"><other>Coordinate</other> to copy.</param>
        [Obsolete]
        public Coordinate(ICoordinate c) : this(c.X, c.Y, c.Z) { }

        /// <summary>
        /// Constructs a <other>Coordinate</other> having the same (x,y,z) values as
        /// <other>other</other>.
        /// </summary>
        /// <param name="c"><other>Coordinate</other> to copy.</param>
        public Coordinate(Coordinate c) : this(c.X, c.Y, c.Z) { }

        /// <summary>
        /// Constructs a <other>Coordinate</other> at (x,y,NaN).
        /// </summary>
        /// <param name="x">X value.</param>
        /// <param name="y">Y value.</param>
        public Coordinate(double x, double y) : this(x, y, NullOrdinate) { }

        /// <summary>
        /// Gets/Sets <other>Coordinate</other>s (x,y,z) values.
        /// </summary>
        public Coordinate CoordinateValue
        {
            get { return this; }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        /// <summary>
        /// Returns whether the planar projections of the two <other>Coordinate</other>s are equal.
        ///</summary>
        /// <param name="other"><other>Coordinate</other> with which to do the 2D comparison.</param>
        /// <returns>
        /// <other>true</other> if the x- and y-coordinates are equal;
        /// the Z coordinates do not have to be equal.
        /// </returns>
        public bool Equals2D(Coordinate other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <summary>
        /// Tests if another coordinate has the same value for X and Y, within a tolerance.
        /// </summary>
        /// <param name="c">A <see cref="Coordinate"/>.</param>
        /// <param name="tolerance">The tolerance value.</param>
        /// <returns><c>true</c> if the X and Y ordinates are within the given tolerance.</returns>
        /// <remarks>The Z ordinate is ignored.</remarks>
        public bool Equals2D(Coordinate c, double tolerance)
        {
            if (!EqualsWithTolerance(X, c.X, tolerance))
                return false;
            if (!EqualsWithTolerance(Y, c.Y, tolerance))
                return false;
            return true;
        }

        private static bool EqualsWithTolerance(double x1, double x2, double tolerance)
        {
            return Math.Abs(x1 - x2) <= tolerance;
        }

        /// <summary>
        /// Returns <other>true</other> if <other>other</other> has the same values for the x and y ordinates.
        /// Since Coordinates are 2.5D, this routine ignores the z value when making the comparison.
        /// </summary>
        /// <param name="other"><other>Coordinate</other> with which to do the comparison.</param>
        /// <returns><other>true</other> if <other>other</other> is a <other>Coordinate</other> with the same values for the x and y ordinates.</returns>
        public override bool Equals(object other)
        {
            if (other == null)
                return false;
            var otherC = other as Coordinate;
            if (otherC != null)
                return Equals(otherC);
#pragma warning disable 612,618
            if (!(other is ICoordinate))
                return false;
            return ((ICoordinate)this).Equals((ICoordinate)other);
#pragma warning restore 612,618
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Boolean Equals(Coordinate other)
        {
            return Equals2D(other);
        }

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="obj1"></param>
        ///// <param name="obj2"></param>
        ///// <returns></returns>
        //public static bool operator ==(Coordinate obj1, ICoordinate obj2)
        //{
        //    return Equals(obj1, obj2);
        //}

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="obj1"></param>
        ///// <param name="obj2"></param>
        ///// <returns></returns>
        //public static bool operator !=(Coordinate obj1, ICoordinate obj2)
        //{
        //    return !(obj1 == obj2);
        //}

        /// <summary>
        /// Compares this object with the specified object for order.
        /// Since Coordinates are 2.5D, this routine ignores the z value when making the comparison.
        /// Returns
        ///   -1  : this.x lowerthan other.x || ((this.x == other.x) AND (this.y lowerthan other.y))
        ///    0  : this.x == other.x AND this.y = other.y
        ///    1  : this.x greaterthan other.x || ((this.x == other.x) AND (this.y greaterthan other.y))
        /// </summary>
        /// <param name="o"><other>Coordinate</other> with which this <other>Coordinate</other> is being compared.</param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <other>Coordinate</other>
        ///         is less than, equal to, or greater than the specified <other>Coordinate</other>.
        /// </returns>
        public int CompareTo(object o)
        {
            var other = (Coordinate)o;
            return CompareTo(other);
        }

        /// <summary>
        /// Compares this object with the specified object for order.
        /// Since Coordinates are 2.5D, this routine ignores the z value when making the comparison.
        /// Returns
        ///   -1  : this.x lowerthan other.x || ((this.x == other.x) AND (this.y lowerthan other.y))
        ///    0  : this.x == other.x AND this.y = other.y
        ///    1  : this.x greaterthan other.x || ((this.x == other.x) AND (this.y greaterthan other.y))
        /// </summary>
        /// <param name="other"><other>Coordinate</other> with which this <other>Coordinate</other> is being compared.</param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <other>Coordinate</other>
        ///         is less than, equal to, or greater than the specified <other>Coordinate</other>.
        /// </returns>
        public int CompareTo(Coordinate other)
        {
            if (X < other.X)
                return -1;
            if (X > other.X)
                return 1;
            if (Y < other.Y)
                return -1;
            return Y > other.Y ? 1 : 0;
        }

        /// <summary>
        /// Returns <c>true</c> if <paramref name="other"/> 
        /// has the same values for X, Y and Z.
        /// </summary>
        /// <param name="other">A <see cref="Coordinate"/> with which to do the 3D comparison.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="other"/> is a <see cref="Coordinate"/> 
        /// with the same values for X, Y and Z.
        /// </returns>
        public bool Equals3D(Coordinate other)
        {
            return (X == other.X) && (Y == other.Y) &&
                ((Z == other.Z) || (Double.IsNaN(Z) && Double.IsNaN(other.Z)));
        }

        /// <summary>
        /// Tests if another coordinate has the same value for Z, within a tolerance.
        /// </summary>
        /// <param name="c">A <see cref="Coordinate"/>.</param>
        /// <param name="tolerance">The tolerance value.</param>
        /// <returns><c>true</c> if the Z ordinates are within the given tolerance.</returns>
        public bool EqualInZ(Coordinate c, double tolerance)
        {
            return EqualsWithTolerance(this.Z, c.Z, tolerance);
        }

        /// <summary>
        /// Returns a <other>string</other> of the form <I>(x,y,z)</I> .
        /// </summary>
        /// <returns><other>string</other> of the form <I>(x,y,z)</I></returns>
        public override string ToString()
        {
            return "(" + X.ToString("R", NumberFormatInfo.InvariantInfo) + ", " +
                         Y.ToString("R", NumberFormatInfo.InvariantInfo) + ", " +
                         Z.ToString("R", NumberFormatInfo.InvariantInfo) + ")";
        }

        /// <summary>
        /// Create a new object as copy of this instance.
        /// </summary>
        /// <returns></returns>
        public virtual Coordinate Copy()
        {
            return new Coordinate(X, Y, Z);
        }

        /// <summary>
        /// Create a new object as copy of this instance.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use Copy")]
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Computes the 2-dimensional Euclidean distance to another location.
        /// </summary>
        /// <param name="c">A <see cref="Coordinate"/> with which to do the distance comparison.</param>
        /// <returns>the 2-dimensional Euclidean distance between the locations.</returns>
        /// <remarks>The Z-ordinate is ignored.</remarks>
        public double Distance(Coordinate c)
        {
            var dx = X - c.X;
            var dy = Y - c.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Computes the 3-dimensional Euclidean distance to another location.
        /// </summary>
        /// <param name="c">A <see cref="Coordinate"/> with which to do the distance comparison.</param>
        /// <returns>the 3-dimensional Euclidean distance between the locations.</returns>
        public double Distance3D(Coordinate c)
        {
            double dx = X - c.X;
            double dy = Y - c.Y;
            double dz = Z - c.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Gets a hashcode for this coordinate.
        /// </summary>
        /// <returns>A hashcode for this coordinate.</returns>
        public override int GetHashCode()
        {
            var result = 17;
            // ReSharper disable NonReadonlyFieldInGetHashCode
            result = 37 * result + GetHashCode(X);
            result = 37 * result + GetHashCode(Y);
            // ReSharper restore NonReadonlyFieldInGetHashCode
            return result;
        }

        /// <summary>
        /// Computes a hash code for a double value, using the algorithm from
        /// Joshua Bloch's book <i>Effective Java"</i>
        /// </summary>
        /// <param name="value">A hashcode for the double value</param>
        public static int GetHashCode(double value)
        {
            return value.GetHashCode();

            // This was implemented as follows, but that's actually equivalent:
            /*
            var f = BitConverter.DoubleToInt64Bits(value);
            return (int)(f ^ (f >> 32));
            */
        }

        #region ICoordinate

        /// <summary>
        /// X coordinate.
        /// </summary>
        [Obsolete]
        double ICoordinate.X
        {
            get { return X; }
            set { X = value; }
        }

        /// <summary>
        /// Y coordinate.
        /// </summary>
        [Obsolete]
        double ICoordinate.Y
        {
            get { return Y; }
            set { Y = value; }
        }

        /// <summary>
        /// Z coordinate.
        /// </summary>
        [Obsolete]
        double ICoordinate.Z
        {
            get { return Z; }
            set { Z = value; }
        }

        /// <summary>
        /// The measure value
        /// </summary>
        [Obsolete]
        double ICoordinate.M
        {
            get { return NullOrdinate; }
            set { }
        }

        /// <summary>
        /// Gets/Sets <other>Coordinate</other>s (x,y,z) values.
        /// </summary>
        [Obsolete]
        ICoordinate ICoordinate.CoordinateValue
        {
            get { return this; }
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        /// <summary>
        /// Gets/Sets the ordinate value for a given index
        /// </summary>
        /// <param name="index">The index of the ordinate</param>
        /// <returns>The ordinate value</returns>
        [Obsolete]
        Double ICoordinate.this[Ordinate index]
        {
            get
            {
                switch (index)
                {
                    case Ordinate.X:
                        return X;
                    case Ordinate.Y:
                        return Y;
                    case Ordinate.Z:
                        return Z;
                    default:
                        return NullOrdinate;
                }
            }
            set
            {
                switch (index)
                {
                    case Ordinate.X:
                        X = value;
                        break;
                    case Ordinate.Y:
                        Y = value;
                        break;
                    case Ordinate.Z:
                        Z = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Returns whether the planar projections of the two <other>Coordinate</other>s are equal.
        ///</summary>
        /// <param name="other"><other>Coordinate</other> with which to do the 2D comparison.</param>
        /// <returns>
        /// <other>true</other> if the x- and y-coordinates are equal;
        /// the Z coordinates do not have to be equal.
        /// </returns>
        [Obsolete]
        bool ICoordinate.Equals2D(ICoordinate other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <summary>
        /// Compares this object with the specified object for order.
        /// Since Coordinates are 2.5D, this routine ignores the z value when making the comparison.
        /// Returns
        ///   -1  : this.x lowerthan other.x || ((this.x == other.x) AND (this.y lowerthan other.y))
        ///    0  : this.x == other.x AND this.y = other.y
        ///    1  : this.x greaterthan other.x || ((this.x == other.x) AND (this.y greaterthan other.y))
        /// </summary>
        /// <param name="other"><other>Coordinate</other> with which this <other>Coordinate</other> is being compared.</param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <other>Coordinate</other>
        ///         is less than, equal to, or greater than the specified <other>Coordinate</other>.
        /// </returns>
        [Obsolete]
        int IComparable<ICoordinate>.CompareTo(ICoordinate other)
        {
            if (X < other.X)
                return -1;
            if (X > other.X)
                return 1;
            if (Y < other.Y)
                return -1;
            return Y > other.Y ? 1 : 0;
        }

        /// <summary>
        /// Compares this object with the specified object for order.
        /// Since Coordinates are 2.5D, this routine ignores the z value when making the comparison.
        /// Returns
        ///   -1  : this.x lowerthan other.x || ((this.x == other.x) AND (this.y lowerthan other.y))
        ///    0  : this.x == other.x AND this.y = other.y
        ///    1  : this.x greaterthan other.x || ((this.x == other.x) AND (this.y greaterthan other.y))
        /// </summary>
        /// <param name="o"><other>Coordinate</other> with which this <other>Coordinate</other> is being compared.</param>
        /// <returns>
        /// A negative integer, zero, or a positive integer as this <other>Coordinate</other>
        ///         is less than, equal to, or greater than the specified <other>Coordinate</other>.
        /// </returns>
        int IComparable.CompareTo(object o)
        {
            var other = (Coordinate)o;
            return CompareTo(other);
        }

        /// <summary>
        /// Returns <other>true</other> if <other>other</other> has the same values for x, y and z.
        /// </summary>
        /// <param name="other"><other>Coordinate</other> with which to do the 3D comparison.</param>
        /// <returns><other>true</other> if <other>other</other> is a <other>Coordinate</other> with the same values for x, y and z.</returns>
        [Obsolete]
        bool ICoordinate.Equals3D(ICoordinate other)
        {
            return (X == other.X) && (Y == other.Y) &&
                ((Z == other.Z) || (Double.IsNaN(Z) && Double.IsNaN(other.Z)));
        }

        /// <summary>
        /// Computes the 2-dimensional Euclidean distance to another location.
        /// The Z-ordinate is ignored.
        /// </summary>
        /// <param name="p"><other>Coordinate</other> with which to do the distance comparison.</param>
        /// <returns>the 2-dimensional Euclidean distance between the locations</returns>
        [Obsolete]
        double ICoordinate.Distance(ICoordinate p)
        {
            var dx = X - p.X;
            var dy = Y - p.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        #endregion ICoordinate

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        ///// <summary>
        ///// Overloaded + operator.
        ///// </summary>
        //public static Coordinate operator +(Coordinate coord1, Coordinate coord2)
        //{
        //    return new Coordinate(coord1.X + coord2.X, coord1.Y + coord2.Y, coord1.Z + coord2.Z);
        //}

        ///// <summary>
        ///// Overloaded + operator.
        ///// </summary>
        //public static Coordinate operator +(Coordinate coord1, double d)
        //{
        //    return new Coordinate(coord1.X + d, coord1.Y + d, coord1.Z + d);
        //}

        ///// <summary>
        ///// Overloaded + operator.
        ///// </summary>
        //public static Coordinate operator +(double d, Coordinate coord1)
        //{
        //    return coord1 + d;
        //}

        ///// <summary>
        ///// Overloaded * operator.
        ///// </summary>
        //public static Coordinate operator *(Coordinate coord1, Coordinate coord2)
        //{
        //    return new Coordinate(coord1.X * coord2.X, coord1.Y * coord2.Y, coord1.Z * coord2.Z);
        //}

        ///// <summary>
        ///// Overloaded * operator.
        ///// </summary>
        //public static Coordinate operator *(Coordinate coord1, double d)
        //{
        //    return new Coordinate(coord1.X * d, coord1.Y * d, coord1.Z * d);
        //}

        ///// <summary>
        ///// Overloaded * operator.
        ///// </summary>
        //public static Coordinate operator *(double d, Coordinate coord1)
        //{
        //    return coord1 * d;
        //}

        ///// <summary>
        ///// Overloaded - operator.
        ///// </summary>
        //public static Coordinate operator -(Coordinate coord1, Coordinate coord2)
        //{
        //    return new Coordinate(coord1.X - coord2.X, coord1.Y - coord2.Y, coord1.Z - coord2.Z);
        //}

        ///// <summary>
        ///// Overloaded - operator.
        ///// </summary>
        //public static Coordinate operator -(Coordinate coord1, double d)
        //{
        //    return new Coordinate(coord1.X - d, coord1.Y - d, coord1.Z - d);
        //}

        ///// <summary>
        ///// Overloaded - operator.
        ///// </summary>
        //public static Coordinate operator -(double d, Coordinate coord1)
        //{
        //    return coord1 - d;
        //}

        ///// <summary>
        ///// Overloaded / operator.
        ///// </summary>
        //public static Coordinate operator /(Coordinate coord1, Coordinate coord2)
        //{
        //    return new Coordinate(coord1.X / coord2.X, coord1.Y / coord2.Y, coord1.Z / coord2.Z);
        //}

        ///// <summary>
        ///// Overloaded / operator.
        ///// </summary>
        //public static Coordinate operator /(Coordinate coord1, double d)
        //{
        //    return new Coordinate(coord1.X / d, coord1.Y / d, coord1.Z / d);
        //}

        ///// <summary>
        ///// Overloaded / operator.
        ///// </summary>
        //public static Coordinate operator /(double d, Coordinate coord1)
        //{
        //    return coord1 / d;
        //}

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}