using System;
// Copyright 2013-2015 - Felix Obermaier (www.ivv-aachen.de)
// Copyright 2015      - Spartaco Giubbolini
//
// This file is part of GeoAPI.
// GeoAPI is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// GeoAPI is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;

namespace GeoAPI
{
    /// <summary>
    /// Interface for classes that provide access to coordinate system and tranformation facilities.
    /// </summary>
    public interface ICoordinateSystemServices
    {
        /// <summary>
        /// Returns the coordinate system by <paramref name="srid"/> identifier
        /// </summary>
        /// <param name="srid">The initialization for the coordinate system</param>
        /// <returns>The coordinate system.</returns>
        ICoordinateSystem GetCoordinateSystem(int srid);

        /// <summary>
        /// Returns the coordinate system by <paramref name="authority"/> and <paramref name="code"/>.
        /// </summary>
        /// <param name="authority">The authority for the coordinate system</param>
        /// <param name="code">The code assigned to the coordinate system by <paramref name="authority"/>.</param>
        /// <returns>The coordinate system.</returns>
        ICoordinateSystem GetCoordinateSystem(string authority, long code);

        /// <summary>
        /// Method to get the identifier, by which this coordinate system can be accessed.
        /// </summary>
        /// <param name="authority">The authority name</param>
        /// <param name="authorityCode">The code assigned by <paramref name="authority"/></param>
        /// <returns>The identifier or <value>null</value></returns>
        int? GetSRID(string authority, long authorityCode);

        /// <summary>
        /// Method to create a coordinate tranformation between two spatial reference systems, defined by their identifiers
        /// </summary>
        /// <remarks>This is a convenience function for <see cref="CreateTransformation(ICoordinateSystem,ICoordinateSystem)"/>.</remarks>
        /// <param name="sourceSrid">The identifier for the source spatial reference system.</param>
        /// <param name="targetSrid">The identifier for the target spatial reference system.</param>
        /// <returns>A coordinate transformation, <value>null</value> if no transformation could be created.</returns>
        ICoordinateTransformation CreateTransformation(int sourceSrid, int targetSrid);

        /// <summary>
        /// Method to create a coordinate tranformation between two spatial reference systems
        /// </summary>
        /// <param name="source">The source spatial reference system.</param>
        /// <param name="target">The target spatial reference system.</param>
        /// <returns>A coordinate transformation, <value>null</value> if no transformation could be created.</returns>
        ICoordinateTransformation CreateTransformation(ICoordinateSystem source, ICoordinateSystem target);
    
    }
}