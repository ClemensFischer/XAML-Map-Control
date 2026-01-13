using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;
#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl.Projections
{
    /// <summary>
    /// MapProjection based on ProjNET4GeoApi.
    /// </summary>
    public class GeoApiProjection : MapProjection
    {
        protected GeoApiProjection()
        {
        }

        public GeoApiProjection(string coordinateSystemWkt)
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
            protected set => CoordinateSystem = new CoordinateSystemFactory().CreateFromWkt(value) as IProjectedCoordinateSystem;
        }

        /// <summary>
        /// Gets or sets the ICoordinateSystem of the MapProjection.
        /// </summary>
        public IProjectedCoordinateSystem CoordinateSystem
        {
            get;
            protected set
            {
                field = value ?? throw new ArgumentNullException(nameof(value));

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

                if (CrsId == "EPSG:3857")
                {
                    Type = MapProjectionType.WebMercator;
                }
                else
                {
                    var projection = field.Projection ??
                        throw new ArgumentException("CoordinateSystem.Projection must not be null.", nameof(value));

                    var centralMeridian = projection.GetParameter("central_meridian") ?? projection.GetParameter("longitude_of_origin");
                    var centralParallel = projection.GetParameter("central_parallel") ?? projection.GetParameter("latitude_of_origin");
                    var falseEasting = projection.GetParameter("false_easting");
                    var falseNorthing = projection.GetParameter("false_northing");

                    if ((centralMeridian == null || centralMeridian.Value == 0d) &&
                        (centralParallel == null || centralParallel.Value == 0d) &&
                        (falseEasting == null || falseEasting.Value == 0d) &&
                        (falseNorthing == null || falseNorthing.Value == 0d))
                    {
                        Type = MapProjectionType.NormalCylindrical;
                    }
                    else if (projection.Name.StartsWith("UTM") || projection.Name.StartsWith("Transverse"))
                    {
                        Type = MapProjectionType.TransverseCylindrical;
                    }
                }
            }
        }

        public IMathTransform LocationToMapTransform { get; private set; }

        public IMathTransform MapToLocationTransform { get; private set; }

        public override Point RelativeScale(double latitude, double longitude)
        {
            var k = CoordinateSystem?.Projection?.GetParameter("scale_factor")?.Value ?? 1d;

            return new Point(k, k);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            if (LocationToMapTransform == null)
            {
                throw new InvalidOperationException("The CoordinateSystem property is not set.");
            }

            try
            {
                var coordinate = LocationToMapTransform.Transform(new Coordinate(longitude, latitude));
                return new Point(coordinate.X, coordinate.Y);
            }
            catch
            {
                return null;
            }
        }

        public override Location MapToLocation(double x, double y)
        {
            if (MapToLocationTransform == null)
            {
                throw new InvalidOperationException("The CoordinateSystem property is not set.");
            }

            try
            {
                var coordinate = MapToLocationTransform.Transform(new Coordinate(x, y));
                return new Location(coordinate.Y, coordinate.X);
            }
            catch
            {
                return null;
            }
        }
    }
}
