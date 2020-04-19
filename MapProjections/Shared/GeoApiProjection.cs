// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
#if WINDOWS_UWP
using Windows.Foundation;
#else
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
        private bool isNormalCylindrical;
        private bool isWebMercator;
        private string bboxFormat;

        public IMathTransform LocationToMapTransform { get; private set; }
        public IMathTransform MapToLocationTransform { get; private set; }

        /// <summary>
        /// Gets or sets the ICoordinateSystem of the MapProjection.
        /// </summary>
        public ICoordinateSystem CoordinateSystem
        {
            get { return coordinateSystem; }
            set
            {
                coordinateSystem = value ?? throw new ArgumentNullException("The property value must not be null.");

                var transformFactory = new CoordinateTransformationFactory();

                LocationToMapTransform = transformFactory
                    .CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, coordinateSystem)
                    .MathTransform;

                MapToLocationTransform = transformFactory
                    .CreateFromCoordinateSystems(coordinateSystem, GeographicCoordinateSystem.WGS84)
                    .MathTransform;

                CrsId = (!string.IsNullOrEmpty(coordinateSystem.Authority) && coordinateSystem.AuthorityCode > 0)
                    ? string.Format("{0}:{1}", coordinateSystem.Authority, coordinateSystem.AuthorityCode)
                    : null;

                var projection = (coordinateSystem as IProjectedCoordinateSystem)?.Projection;

                if (projection != null)
                {
                    var centralMeridian = projection.GetParameter("central_meridian") ?? projection.GetParameter("longitude_of_origin");
                    var centralParallel = projection.GetParameter("central_parallel") ?? projection.GetParameter("latitude_of_origin");
                    var falseEasting = projection.GetParameter("false_easting");
                    var falseNorthing = projection.GetParameter("false_northing");

                    isNormalCylindrical =
                        centralMeridian != null && centralMeridian.Value == 0d &&
                        centralParallel != null && centralParallel.Value == 0d &&
                        (falseEasting == null || falseEasting.Value == 0d) &&
                        (falseNorthing == null || falseNorthing.Value == 0d);
                    isWebMercator = CrsId == "EPSG:3857" || CrsId == "EPSG:900913";
                    bboxFormat = "{0},{1},{2},{3}";
                }
                else
                {
                    isNormalCylindrical = true;
                    isWebMercator = false;
                    bboxFormat = "{1},{0},{3},{2}";
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

        public override bool IsNormalCylindrical
        {
            get { return isNormalCylindrical; }
        }

        public override bool IsWebMercator
        {
            get { return isWebMercator; }
        }

        public override Point LocationToMap(Location location)
        {
            if (LocationToMapTransform == null)
            {
                throw new InvalidOperationException("The CoordinateSystem property is not set.");
            }

            var coordinate = LocationToMapTransform.Transform(new Coordinate(location.Longitude, location.Latitude));

            return new Point(coordinate.X, coordinate.Y);
        }

        public override Location MapToLocation(Point point)
        {
            if (MapToLocationTransform == null)
            {
                throw new InvalidOperationException("The CoordinateSystem property is not set.");
            }

            var coordinate = MapToLocationTransform.Transform(new Coordinate(point.X, point.Y));

            return new Location(coordinate.Y, coordinate.X);
        }

        public override string GetBboxValue(Rect rect)
        {
            return string.Format(CultureInfo.InvariantCulture,
                bboxFormat, rect.X, rect.Y, (rect.X + rect.Width), (rect.Y + rect.Height));
        }
    }
}
