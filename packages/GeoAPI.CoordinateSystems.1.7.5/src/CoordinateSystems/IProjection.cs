// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

namespace GeoAPI.CoordinateSystems
{
	/// <summary>
	/// The IProjection interface defines the standard information stored with projection
	/// objects. A projection object implements a coordinate transformation from a geographic
	/// coordinate system to a projected coordinate system, given the ellipsoid for the
	/// geographic coordinate system. It is expected that each coordinate transformation of
	/// interest, e.g., Transverse Mercator, Lambert, will be implemented as a COM class of
	/// coType Projection, supporting the IProjection interface.
	/// </summary>
	public interface IProjection : IInfo
	{
		/// <summary>
		/// Gets number of parameters of the projection.
		/// </summary>
		int NumParameters { get; }

		/// <summary>
		/// Gets the projection classification name (e.g. 'Transverse_Mercator').
		/// </summary>
		string ClassName { get; }

		/// <summary>
		/// Gets an indexed parameter of the projection.
		/// </summary>
		/// <param name="index">Index of parameter</param>
		/// <returns>n'th parameter</returns>
		ProjectionParameter GetParameter(int index);

		/// <summary>
		/// Gets an named parameter of the projection.
		/// </summary>
		/// <remarks>The parameter name is case insensitive</remarks>
		/// <param name="name">Name of parameter</param>
		/// <returns>parameter or null if not found</returns>
		ProjectionParameter GetParameter(string name);
	}
}
