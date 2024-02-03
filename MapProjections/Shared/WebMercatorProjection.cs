// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using ProjNet.CoordinateSystems;
using System;

namespace MapControl.Projections
{
    /// <summary>
    /// Spherical Mercator Projection implemented by setting the CoordinateSystem property of a GeoApiProjection.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.41-44.
    /// </summary>
    public class WebMercatorProjection : GeoApiProjection
    {
        public const int DefaultEpsgCode = 3857;

        public WebMercatorProjection()
        {
            CoordinateSystem = ProjectedCoordinateSystem.WebMercator;
        }

        public override Scale GetRelativeScale(Location location)
        {
            var k = 1d / Math.Cos(location.Latitude * Math.PI / 180d); // p.44 (7-3)

            return new Scale(k, k);
        }
    }
}
