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
        private IProjectedCoordinateSystem coordinateSystem;

        public IMathTransform MathTransform { get; private set; }
        public IMathTransform InverseTransform { get; private set; }

        /// <summary>
        /// Gets or sets the IProjectedCoordinateSystem of the MapProjection.
        /// </summary>
        public IProjectedCoordinateSystem CoordinateSystem
        {
            get { return coordinateSystem; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("The property value must not be null.");
                }

                coordinateSystem = value;

                var coordinateTransform = new CoordinateTransformationFactory()
                    .CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, coordinateSystem);

                MathTransform = coordinateTransform.MathTransform;
                InverseTransform = MathTransform.Inverse();

                CrsId = (!string.IsNullOrEmpty(coordinateSystem.Authority) && coordinateSystem.AuthorityCode > 0)
                    ? string.Format("{0}:{1}", coordinateSystem.Authority, coordinateSystem.AuthorityCode)
                    : null;

                if (!IsWebMercator)
                {
                    IsWebMercator = CrsId == "EPSG:3857" || CrsId == "EPSG:900913";
                }

                var projection = coordinateSystem.Projection;
                var scaleFactor = projection.GetParameter("scale_factor");

                if (scaleFactor != null)
                {
                    TrueScale = scaleFactor.Value * MetersPerDegree;
                }

                if (!IsNormalCylindrical)
                {
                    var centralMeridian = projection.GetParameter("central_meridian") ?? projection.GetParameter("longitude_of_origin");
                    var centralParallel = projection.GetParameter("latitude_of_origin") ?? projection.GetParameter("central_parallel");
                    var falseEasting = projection.GetParameter("false_easting");
                    var falseNorthing = projection.GetParameter("false_northing");

                    if (centralMeridian != null && centralMeridian.Value == 0d &&
                        centralParallel != null && centralParallel.Value == 0d &&
                        (falseEasting == null || falseEasting.Value == 0d) &&
                        (falseNorthing == null || falseNorthing.Value == 0d))
                    {
                        IsNormalCylindrical = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets an OGC Well-known text representation of a projected coordinate system,
        /// i.e. a PROJCS[...] string as used by https://epsg.io or http://spatialreference.org.
        /// Setting this property updates the CoordinateSystem property with an IProjectedCoordinateSystem created from the WKT string.
        /// </summary>
        public string WKT
        {
            get { return CoordinateSystem?.WKT; }
            set { CoordinateSystem = (IProjectedCoordinateSystem)new CoordinateSystemFactory().CreateFromWkt(value); }
        }

        public override Point LocationToPoint(Location location)
        {
            if (MathTransform == null)
            {
                throw new InvalidOperationException("The CoordinateSystem property is not set.");
            }

            var coordinate = MathTransform.Transform(new Coordinate(location.Longitude, location.Latitude));

            return new Point(coordinate.X, coordinate.Y);
        }

        public override Location PointToLocation(Point point)
        {
            if (InverseTransform == null)
            {
                throw new InvalidOperationException("The CoordinateSystem property is not set.");
            }

            var coordinate = InverseTransform.Transform(new Coordinate(point.X, point.Y));

            return new Location(coordinate.Y, coordinate.X);
        }
    }
}
