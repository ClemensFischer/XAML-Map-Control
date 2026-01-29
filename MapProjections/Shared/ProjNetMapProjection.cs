using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;
#if WPF
using System.Windows;
using System.Windows.Media;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl.Projections
{
    /// <summary>
    /// MapProjection based on ProjNet.
    /// </summary>
    public class ProjNetMapProjection : MapProjection
    {
        protected MapProjection FallbackProjection { get; private set; }

        protected ProjNetMapProjection(MapProjection fallbackProjection)
        {
            FallbackProjection = fallbackProjection;
        }

        public ProjNetMapProjection(string coordinateSystemWkt)
        {
            CoordinateSystemWkt = coordinateSystemWkt;
        }

        /// <summary>
        /// Gets or sets an OGC Well-known text representation of a coordinate system,
        /// i.e. a PROJCS[...] or GEOGCS[...] string as used by https://epsg.io or http://spatialreference.org.
        /// Setting this property updates the CoordinateSystem property with an ICoordinateSystem created from the WKT string.
        /// </summary>
        public string CoordinateSystemWkt
        {
            get => CoordinateSystem?.WKT;
            protected set => CoordinateSystem = new CoordinateSystemFactory().CreateFromWkt(value) as ProjectedCoordinateSystem;
        }

        /// <summary>
        /// Gets or sets the ICoordinateSystem of the MapProjection.
        /// </summary>
        public ProjectedCoordinateSystem CoordinateSystem
        {
            get;
            protected set
            {
                field = value ??
                    throw new ArgumentNullException(nameof(value));

                var projection = field.Projection ??
                    throw new ArgumentException("CoordinateSystem.Projection must not be null.", nameof(value));

                IsNormalCylindrical = projection.Name.StartsWith("Mercator") || projection.Name.Contains("Pseudo-Mercator");

                var transformFactory = new CoordinateTransformationFactory();

                LocationToMapTransform = transformFactory
                    .CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, field)
                    .MathTransform;

                MapToLocationTransform = transformFactory
                    .CreateFromCoordinateSystems(field, GeographicCoordinateSystem.WGS84)
                    .MathTransform;

                CrsId = !string.IsNullOrEmpty(field.Authority) && field.AuthorityCode > 0
                    ? $"{field.Authority}:{field.AuthorityCode}"
                    : string.Empty;

                var ellipsoid = field.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid;

                if (projection.Name.Contains("Pseudo-Mercator"))
                {
                    FallbackProjection = new MapControl.WebMercatorProjection
                    {
                        EquatorialRadius = ellipsoid.SemiMajorAxis
                    };
                }
                else if (projection.Name.StartsWith("Mercator"))
                {
                    FallbackProjection = new MapControl.WorldMercatorProjection
                    {
                        EquatorialRadius = ellipsoid.SemiMajorAxis,
                        Flattening = 1d / ellipsoid.InverseFlattening
                    };
                }
                else if (projection.Name.StartsWith("Transverse_Mercator"))
                {
                    FallbackProjection = new TransverseMercatorProjection
                    {
                        EquatorialRadius = ellipsoid.SemiMajorAxis,
                        Flattening = 1d / ellipsoid.InverseFlattening,
                        CentralMeridian = projection.GetParameter("central_meridian").Value,
                        ScaleFactor = projection.GetParameter("scale_factor").Value,
                        FalseEasting = projection.GetParameter("false_easting").Value,
                        FalseNorthing = projection.GetParameter("false_northing").Value
                    };
                }
                else if (projection.Name.StartsWith("Polar_Stereographic"))
                {
                    FallbackProjection = new PolarStereographicProjection
                    {
                        EquatorialRadius = ellipsoid.SemiMajorAxis,
                        Flattening = 1d / ellipsoid.InverseFlattening,
                        ScaleFactor = projection.GetParameter("scale_factor").Value,
                        FalseEasting = projection.GetParameter("false_easting").Value,
                        FalseNorthing = projection.GetParameter("false_northing").Value,
                        Hemisphere = projection.GetParameter("latitude_of_origin").Value >= 0 ? Hemisphere.North : Hemisphere.South
                    };
                }
            }
        }

        public MathTransform LocationToMapTransform { get; private set; }

        public MathTransform MapToLocationTransform { get; private set; }

        public override Point LocationToMap(double latitude, double longitude)
        {
            if (LocationToMapTransform == null)
            {
                throw new InvalidOperationException("The CoordinateSystem property is not set.");
            }

            var coordinate = LocationToMapTransform.Transform([longitude, latitude]);

            return new Point(coordinate[0], coordinate[1]);
        }

        public override Location MapToLocation(double x, double y)
        {
            if (MapToLocationTransform == null)
            {
                throw new InvalidOperationException("The CoordinateSystem property is not set.");
            }

            var coordinate = MapToLocationTransform.Transform([x, y]);

            return new Location(coordinate[1], coordinate[0]);
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            return FallbackProjection != null
                ? FallbackProjection.RelativeTransform(latitude, longitude)
                : new Matrix(1d, 0d, 0d, 1d, 0d, 0d);
        }

        public override double GridConvergence(double x, double y)
        {
            return FallbackProjection != null
                ? FallbackProjection.GridConvergence(x, y)
                : 0d;
        }
    }
}
