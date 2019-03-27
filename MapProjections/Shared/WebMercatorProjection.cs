// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINDOWS_UWP
using System.Windows;
#endif
using ProjNet.CoordinateSystems;

namespace MapControl.Projections
{
    /// <summary>
    /// Spherical Mercator Projection implemented by setting the CoordinateSystem property of a GeoApiProjection.
    /// See "Map Projections - A Working Manual" (https://pubs.usgs.gov/pp/1395/report.pdf), p.41-44.
    /// </summary>
    public class WebMercatorProjection : GeoApiProjection
    {
        public WebMercatorProjection()
        {
            IsWebMercator = true;
            IsNormalCylindrical = true;
            CoordinateSystem = ProjectedCoordinateSystem.WebMercator;
        }

        public override Vector GetMapScale(Location location)
        {
            var k = 1d / Math.Cos(location.Latitude * Math.PI / 180d); // p.44 (7-3)

            return new Vector(ViewportScale * k, ViewportScale * k);
        }
    }
}
