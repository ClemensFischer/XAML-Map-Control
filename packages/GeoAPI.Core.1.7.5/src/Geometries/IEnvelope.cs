using System;

namespace GeoAPI.Geometries
{
#if HAS_SYSTEM_ICLONEABLE
    using ICloneable = System.ICloneable;
#else
    using ICloneable = GeoAPI.ICloneable;
#endif

    /// <summary>
    /// Defines a rectangular region of the 2D coordinate plane.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is often used to represent the bounding box of a <c>Geometry</c>,
    /// e.g. the minimum and maximum x and y values of the <c>Coordinate</c>s.
    /// </para>
    /// <para>
    /// Note that Envelopes support infinite or half-infinite regions, by using the values of
    /// <c>Double.PositiveInfinity</c> and <c>Double.NegativeInfinity</c>.
    /// </para>
    /// <para>
    /// When Envelope objects are created or initialized,
    /// the supplies extent values are automatically sorted into the correct order.    
    /// </para>
    /// </remarks>
    [Obsolete("Use Envelope class instead")]
    public interface IEnvelope : 
        ICloneable,
        IComparable, IComparable<IEnvelope>
    {
        /// <summary>
        /// Gets the area of the envelope
        /// </summary>
        double Area { get; }

        /// <summary>
        /// Gets the width of the envelope
        /// </summary>
        double Width { get; }

        /// <summary>
        /// Gets the height of the envelope
        /// </summary>
        double Height { get; }

        /// <summary>
        /// Gets the maximum x-ordinate of the envelope
        /// </summary>
        double MaxX { get; }

        /// <summary>
        /// Gets the maximum y-ordinate of the envelope
        /// </summary>
        double MaxY { get; }

        /// <summary>
        /// Gets the minimum x-ordinate of the envelope
        /// </summary>
        double MinX { get; }

        /// <summary>
        /// Gets the mimimum y-ordinate of the envelope
        /// </summary>
        double MinY { get; }

        /// <summary>
        /// Gets the <see cref="ICoordinate"/> or the center of the envelope
        /// </summary>
        ICoordinate Centre { get; }
        
        /// <summary>
        /// Returns if the point specified by <see paramref="x"/> and <see paramref="y"/> is contained by the envelope.
        /// </summary>
        /// <param name="x">The x-ordinate</param>
        /// <param name="y">The y-ordinate</param>
        /// <returns>True if the point is contained by the envlope</returns>
        bool Contains(double x, double y);

        /// <summary>
        /// Returns if the point specified by <see paramref="p"/> is contained by the envelope.
        /// </summary>
        /// <param name="p">The point</param>
        /// <returns>True if the point is contained by the envlope</returns>
        bool Contains(ICoordinate p);

        /// <summary>
        /// Returns if the envelope specified by <see paramref="other"/> is contained by this envelope.
        /// </summary>
        /// <param name="other">The envelope to test</param>
        /// <returns>True if the other envelope is contained by this envlope</returns>
        bool Contains(IEnvelope other);

        ///<summary>
        /// Tests if the given point lies in or on the envelope.
        ///</summary>
        /// <param name="x">the x-coordinate of the point which this <c>Envelope</c> is being checked for containing</param>
        /// <param name="y">the y-coordinate of the point which this <c>Envelope</c> is being checked for containing</param>
        /// <returns> <c>true</c> if <c>(x, y)</c> lies in the interior or on the boundary of this <c>Envelope</c>.</returns>
        bool Covers(double x, double y);

        ///<summary>
        /// Tests if the given point lies in or on the envelope.
        ///</summary>
        /// <param name="p">the point which this <c>Envelope</c> is being checked for containing</param>
        /// <returns><c>true</c> if the point lies in the interior or on the boundary of this <c>Envelope</c>.</returns>
        bool Covers(ICoordinate p);

        ///<summary>
        /// Tests if the <c>Envelope other</c> lies wholely inside this <c>Envelope</c> (inclusive of the boundary).
        ///</summary>
        /// <param name="other">the <c>Envelope</c> to check</param>
        /// <returns>true if this <c>Envelope</c> covers the <c>other</c></returns>
        bool Covers(IEnvelope other);

        /// <summary>
        /// Computes the distance between this and another
        /// <c>Envelope</c>.
        /// The distance between overlapping Envelopes is 0.  Otherwise, the
        /// distance is the Euclidean distance between the closest points.
        /// </summary>
        /// <returns>The distance between this and another <c>Envelope</c>.</returns>
        double Distance(IEnvelope env);

        /// <summary>
        /// Expands this envelope by a given distance in all directions.
        /// Both positive and negative distances are supported.
        /// </summary>
        /// <param name="distance">The distance to expand the envelope.</param>
        void ExpandBy(double distance);

        /// <summary>
        /// Expands this envelope by a given distance in all directions.
        /// Both positive and negative distances are supported.
        /// </summary>
        /// <param name="deltaX">The distance to expand the envelope along the the X axis.</param>
        /// <param name="deltaY">The distance to expand the envelope along the the Y axis.</param>
        void ExpandBy(double deltaX, double deltaY);

        /// <summary>
        /// Enlarges this <code>Envelope</code> so that it contains
        /// the given <see cref="Coordinate"/>.
        /// Has no effect if the point is already on or within the envelope.
        /// </summary>
        /// <param name="p">The Coordinate.</param>
        void ExpandToInclude(ICoordinate p);

        /// <summary>
        /// Enlarges this <c>Envelope</c> so that it contains
        /// the given <see cref="Coordinate"/>.
        /// </summary>
        /// <remarks>Has no effect if the point is already on or within the envelope.</remarks>
        /// <param name="x">The value to lower the minimum x to or to raise the maximum x to.</param>
        /// <param name="y">The value to lower the minimum y to or to raise the maximum y to.</param>
        void ExpandToInclude(double x, double y);

        /// <summary>
        /// Enlarges this <c>Envelope</c> so that it contains
        /// the <c>other</c> Envelope.
        /// Has no effect if <c>other</c> is wholly on or
        /// within the envelope.
        /// </summary>
        /// <param name="other">the <c>Envelope</c> to expand to include.</param>
        void ExpandToInclude(IEnvelope other);

        /// <summary>
        /// Method to initialize the envelope. Calling this function will result in <see cref="IsNull"/> returning <value>true</value>
        /// </summary>
        void Init();

        /// <summary>
        /// Method to initialize the envelope with a <see cref="T:GeoAPI.Geometries.ICoordinate"/>. Calling this function will result in an envelope having no extent but a location.
        /// </summary>
        /// <param name="p">The point</param>
        void Init(ICoordinate p);

        /// <summary>
        /// Method to initialize the envelope. Calling this function will result in an envelope having the same extent as <paramref name="env"/>.
        /// </summary>
        /// <param name="env">The envelope</param>
        void Init(IEnvelope env);

        /// <summary>
        /// Method to initialize the envelope with two <see cref="T:GeoAPI.Geometries.ICoordinate"/>s.
        /// </summary>
        /// <param name="p1">The first point</param>
        /// <param name="p2">The second point</param>
        void Init(ICoordinate p1, ICoordinate p2);

        /// <summary>
        /// Initialize an <c>Envelope</c> for a region defined by maximum and minimum values.
        /// </summary>
        /// <param name="x1">The first x-value.</param>
        /// <param name="x2">The second x-value.</param>
        /// <param name="y1">The first y-value.</param>
        /// <param name="y2">The second y-value.</param>
        void Init(double x1, double x2, double y1, double y2);

        /// <summary>
        /// Computes the intersection of two <see cref="Envelope"/>s.
        /// </summary>
        /// <param name="env">The envelope to intersect with</param>
        /// <returns>
        /// A new Envelope representing the intersection of the envelopes (this will be
        /// the null envelope if either argument is null, or they do not intersect
        /// </returns>
        IEnvelope Intersection(IEnvelope env);

        /// <summary>
        /// Translates this envelope by given amounts in the X and Y direction.
        /// </summary>
        /// <param name="transX">The amount to translate along the X axis.</param>
        /// <param name="transY">The amount to translate along the Y axis.</param>
        void Translate(double transX, double transY);

        /// <summary>
        /// Check if the point <c>p</c> overlaps (lies inside) the region of this <c>Envelope</c>.
        /// </summary>
        /// <param name="p"> the <c>Coordinate</c> to be tested.</param>
        /// <returns><c>true</c> if the point overlaps this <c>Envelope</c>.</returns>
        bool Intersects(ICoordinate p);

        /// <summary>
        /// Check if the point <c>(x, y)</c> overlaps (lies inside) the region of this <c>Envelope</c>.
        /// </summary>
        /// <param name="x"> the x-ordinate of the point.</param>
        /// <param name="y"> the y-ordinate of the point.</param>
        /// <returns><c>true</c> if the point overlaps this <c>Envelope</c>.</returns>
        bool Intersects(double x, double y);

        /// <summary>
        /// Check if the region defined by <c>other</c>
        /// overlaps (intersects) the region of this <c>Envelope</c>.
        /// </summary>
        /// <param name="other"> the <c>Envelope</c> which this <c>Envelope</c> is
        /// being checked for overlapping.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <c>Envelope</c>s overlap.
        /// </returns>
        bool Intersects(IEnvelope other);

        /// <summary>
        /// Returns <c>true</c> if this <c>Envelope</c> is a "null" envelope.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this <c>Envelope</c> is uninitialized
        /// or is the envelope of the empty point.
        /// </returns>
        bool IsNull { get; }

        /// <summary>
        /// Makes this <c>Envelope</c> a "null" envelope..
        /// </summary>
        void SetToNull();

        void Zoom(double perCent);
                
        bool Overlaps(IEnvelope other);

        bool Overlaps(ICoordinate p);
        
        bool Overlaps(double x, double y);
        
        void SetCentre(double width, double height);
        
        void SetCentre(IPoint centre, double width, double height);
        
        void SetCentre(ICoordinate centre);
        
        void SetCentre(IPoint centre);
        
        void SetCentre(ICoordinate centre, double width, double height);

        IEnvelope Union(IPoint point);

        IEnvelope Union(ICoordinate coord);

        IEnvelope Union(IEnvelope box);

    }
}
