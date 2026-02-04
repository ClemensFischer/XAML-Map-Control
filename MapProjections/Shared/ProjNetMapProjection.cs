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
        public ProjNetMapProjection(ProjectedCoordinateSystem coordinateSystem)
        {
            CoordinateSystem = coordinateSystem;
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

                IsNormalCylindrical = projection.Name.Contains("Pseudo-Mercator") ||
                                      projection.Name.StartsWith("Mercator");

                var transformFactory = new CoordinateTransformationFactory();

                CrsId = !string.IsNullOrEmpty(field.Authority) && field.AuthorityCode > 0
                    ? $"{field.Authority}:{field.AuthorityCode}"
                    : string.Empty;

                LocationToMapTransform = transformFactory
                    .CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, field)
                    .MathTransform;

                MapToLocationTransform = transformFactory
                    .CreateFromCoordinateSystems(field, GeographicCoordinateSystem.WGS84)
                    .MathTransform;

                var ellipsoid = field.HorizontalDatum.Ellipsoid;
                EquatorialRadius = ellipsoid.SemiMajorAxis;
                Flattening = 1d / ellipsoid.InverseFlattening;

                var parameter = projection.GetParameter("scale_factor");
                ScaleFactor = parameter != null ? parameter.Value : 1d;

                parameter = projection.GetParameter("central_meridian");
                CentralMeridian = parameter != null ? parameter.Value : 0d;

                parameter = projection.GetParameter("latitude_of_origin");
                LatitudeOfOrigin = parameter != null ? parameter.Value : 0d;

                parameter = projection.GetParameter("false_easting");
                FalseEasting = parameter != null ? parameter.Value : 0d;

                parameter = projection.GetParameter("false_northing");
                FalseNorthing = parameter != null ? parameter.Value : 0d;
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

            double x, y;

            try
            {

                (x, y) = LocationToMapTransform.Transform(longitude, latitude);
            }
            catch (ArgumentException)
            {
                x = 0d;
                y = latitude >= 0d ? double.PositiveInfinity : double.NegativeInfinity;
            }

            return new Point(x, y);
        }

        public override Location MapToLocation(double x, double y)
        {
            if (MapToLocationTransform == null)
            {
                throw new InvalidOperationException("The CoordinateSystem property is not set.");
            }

            (var lon, var lat) = MapToLocationTransform.Transform(x, y);

            return new Location(lat, lon);
        }

        public override double GridConvergence(double latitude, double longitude)
        {
            var projection = CoordinateSystem.Projection.Name;

            if (projection.StartsWith("Transverse_Mercator"))
            {
                return TransverseMercatorGridConvergence(latitude, longitude);
            }

            if (projection.StartsWith("Polar_Stereographic"))
            {
                return PolarStereographicGridConvergence(longitude);
            }

            return base.GridConvergence(latitude, longitude);
        }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            var projection = CoordinateSystem.Projection.Name;

            if (projection.Contains("Pseudo-Mercator"))
            {
                return WebMercatorRelativeTransform(latitude);
            }

            if (projection.StartsWith("Mercator"))
            {
                return WorldMercatorRelativeTransform(latitude);
            }

            if (projection.StartsWith("Polar_Stereographic"))
            {
                return PolarStereographicRelativeTransform(latitude, longitude);
            }

            return base.RelativeTransform(latitude, longitude);
        }

        protected static Matrix WebMercatorRelativeTransform(double latitude)
        {
            var k = 1d / Math.Cos(latitude * Math.PI / 180d); // p.44 (7-3)

            return new Matrix(k, 0d, 0d, k, 0d, 0d);
        }

        protected Matrix WorldMercatorRelativeTransform(double latitude)
        {
            var e2 = (2d - Flattening) * Flattening;
            var phi = latitude * Math.PI / 180d;
            var sinPhi = Math.Sin(phi);
            var k = Math.Sqrt(1d - e2 * sinPhi * sinPhi) / Math.Cos(phi); // p.44 (7-8)

            return new Matrix(k, 0d, 0d, k, 0d, 0d);
        }

        protected double TransverseMercatorGridConvergence(double latitude, double longitude)
        {
            return 180d / Math.PI * Math.Atan(
                Math.Tan((longitude - CentralMeridian) * Math.PI / 180d) *
                Math.Sin(latitude * Math.PI / 180d));
        }

        protected double PolarStereographicGridConvergence(double longitude)
        {
            return Math.Sign(LatitudeOfOrigin) * (longitude - CentralMeridian);
        }

        protected Matrix PolarStereographicRelativeTransform(double latitude, double longitude)
        {
            var sign = Math.Sign(LatitudeOfOrigin);
            var phi = sign * latitude * Math.PI / 180d;
            var e = Math.Sqrt((2d - Flattening) * Flattening);
            var eSinPhi = e * Math.Sin(phi);
            var t = Math.Tan(Math.PI / 4d - phi / 2d)
                  / Math.Pow((1d - eSinPhi) / (1d + eSinPhi), e / 2d); // p.161 (15-9)
            // r == ρ/a
            var r = 2d * ScaleFactor * t / Math.Sqrt(Math.Pow(1d + e, 1d + e) * Math.Pow(1d - e, 1d - e)); // p.161 (21-33)
            var m = Math.Cos(phi) / Math.Sqrt(1d - eSinPhi * eSinPhi); // p.160 (14-15)
            var k = r / m; // p.161 (21-32)

            var transform = new Matrix(k, 0d, 0d, k, 0d, 0d);
            transform.Rotate(-sign * (longitude - CentralMeridian));

            return transform;
        }
    }
}
