using System;

namespace GeoAPI.Geometries
{
    ///<summary>
    /// An interface for classes which process the coordinates in a <see cref="ICoordinateSequence"/>. 
    /// A filter can either record information about each coordinate,
    /// or change the value of the coordinate. 
    /// Filters can be
    /// used to implement operations such as coordinate transformations, centroid and
    /// envelope computation, and many other functions.
    /// <see cref="IGeometry"/> classes support the concept of applying a
    /// <c>CoordinateSequenceFilter</c> to each 
    /// <see cref="ICoordinateSequence"/>s they contain. 
    /// <para/>
    /// For maximum efficiency, the execution of filters can be short-circuited by using the <see cref="ICoordinateSequenceFilter.Done"/> property.
    ///</summary>
    ///<see cref="IGeometry.Apply(ICoordinateSequenceFilter)"/>
    ///<remarks>
    /// <c>CoordinateSequenceFilter</c> is an example of the Gang-of-Four Visitor pattern.
    /// <para><b>Note</b>: In general, it is preferable to treat Geometrys as immutable. 
    /// Mutation should be performed by creating a new Geometry object (see <see cref="T:NetTopologySuite.Geometries.Utilities.GeometryEditor"/> 
    /// and <see cref="T:NetTopologySuite.Geometries.Utilities.GeometryTransformer"/> for convenient ways to do this).
    /// An exception to this rule is when a new Geometry has been created via <see cref="ICoordinateSequence.Copy"/>.
    /// In this case mutating the Geometry will not cause aliasing issues, 
    /// and a filter is a convenient way to implement coordinate transformation.
    /// </para>
    ///</remarks>
    /// <author>Martin Davis</author>
    /// <seealso cref="IGeometry.Apply(ICoordinateFilter)"/>
    /// <seealso cref="T:NetTopologySuite.Geometries.Utilities.GeometryTransformer"/> 
    /// <see cref="T:NetTopologySuite.Geometries.Utilities.GeometryEditor"/> 
    public interface ICoordinateSequenceFilter
    {
        ///<summary>
        /// Performs an operation on a coordinate in a <see cref="ICoordinateSequence"/>.
        ///</summary>
        /// <param name="seq">the <c>CoordinateSequence</c> to which the filter is applied</param>
        /// <param name="i">i the index of the coordinate to apply the filter to</param>
        void Filter(ICoordinateSequence seq, int i);

        ///<summary>
        /// Reports whether the application of this filter can be terminated.
        ///</summary>
        ///<remarks>
        /// Once this method returns <c>false</c>, it should 
        /// continue to return <c>false</c> on every subsequent call.
        ///</remarks>
        Boolean Done { get; }

        ///<summary>
        /// Reports whether the execution of this filter has modified the coordinates of the geometry.
        /// If so, <see cref="IGeometry.GeometryChanged()"/> will be executed
        /// after this filter has finished being executed.
        /// </summary>
        /// <remarks>Most filters can simply return a constant value reflecting whether they are able to change the coordinates.</remarks>
        Boolean GeometryChanged { get; }
    }

}