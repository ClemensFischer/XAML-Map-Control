// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2019 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if !WINDOWS_UWP
using System.Windows;
#endif
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace MapControl.Projections
{
    /// <summary>
    /// MapProjection based on ProjNET4GeoApi.
    /// </summary>
    public class GeoApiProjection : MapProjection
    {
        private ICoordinateSystem coordinateSystem;

        public IMathTransform LocationToPointTransform { get; private set; }
        public IMathTransform PointToLocationTransform { get; private set; }

        /// <summary>
        /// Gets or sets the ICoordinateSystem of the MapProjection.
        /// </summary>
        public ICoordinateSystem CoordinateSystem
        {
            get { return coordinateSystem; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("The property value must not be null.");
                }

                coordinateSystem = value;

                var transformFactory = new CoordinateTransformationFactory();

                LocationToPointTransform = transformFactory
                    .CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, coordinateSystem)
                    .MathTransform;

                PointToLocationTransform = transformFactory
                    .CreateFromCoordinateSystems(coordinateSystem, GeographicCoordinateSystem.WGS84)
                    .MathTransform;

                CrsId = (!string.IsNullOrEmpty(coordinateSystem.Authority) && coordinateSystem.AuthorityCode > 0)
                    ? string.Format("{0}:{1}", coordinateSystem.Authority, coordinateSystem.AuthorityCode)
                    : null;

                IsWebMercator = CrsId == "EPSG:3857" || CrsId == "EPSG:900913";

                var projection = (coordinateSystem as IProjectedCoordinateSystem)?.Projection;

                if (projection != null)
                {
                    var centralMeridian = projection.GetParameter("central_meridian") ?? projection.GetParameter("longitude_of_origin");
                    var centralParallel = projection.GetParameter("latitude_of_origin") ?? projection.GetParameter("central_parallel");
                    var falseEasting = projection.GetParameter("false_easting");
                    var falseNorthing = projection.GetParameter("false_northing");
                    var scaleFactor = projection.GetParameter("scale_factor");

                    IsNormalCylindrical =
                        centralMeridian != null && centralMeridian.Value == 0d &&
                        centralParallel != null && centralParallel.Value == 0d &&
                        (falseEasting == null || falseEasting.Value == 0d) &&
                        (falseNorthing == null || falseNorthing.Value == 0d);
                    TrueScale = (scaleFactor != null ? scaleFactor.Value : 1d) * Wgs84MetersPerDegree;
                }
                else
                {
                    IsNormalCylindrical = true;
                    TrueScale = 1d;
                }
            }
        }

        /// <summary>
        /// Gets or sets an OGC Well-known text representation of a coordinate system,
        /// i.e. a PROJCS[...] or GEOGCS[...] string as used by https://epsg.io or http://spatialreference.org.
        /// Setting this property updates the CoordinateSystem property with an ICoordinateSystem created from the WKT string.
        /// </summary>
        public string WKT
        {
            get { return CoordinateSystem?.WKT; }
            set { CoordinateSystem = new CoordinateSystemFactory().CreateFromWkt(value); }
        }

        public override Point LocationToPoint(Location location)
        {
            if (LocationToPointTransform == null)
            {
                throw new InvalidOperationException("The CoordinateSystem property is not set.");
            }

            var coordinate = LocationToPointTransform.Transform(new Coordinate(location.Longitude, location.Latitude));

            return new Point(coordinate.X, coordinate.Y);
        }

        public override Location PointToLocation(Point point)
        {
            if (PointToLocationTransform == null)
            {
                throw new InvalidOperationException("The CoordinateSystem property is not set.");
            }

            var coordinate = PointToLocationTransform.Transform(new Coordinate(point.X, point.Y));

            return new Location(coordinate.Y, coordinate.X);
        }
    }
}
