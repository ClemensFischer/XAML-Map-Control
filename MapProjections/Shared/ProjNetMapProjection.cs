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
        protected ProjNetMapProjection()
        {
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

                var name = field.Projection?.Name ??
                    throw new ArgumentException("CoordinateSystem.Projection must not be null.", nameof(value));

                IsNormalCylindrical = name.StartsWith("Mercator") || name.Contains("Pseudo-Mercator");

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
            }
        }

        public MathTransform LocationToMapTransform { get; private set; }

        public MathTransform MapToLocationTransform { get; private set; }

        public override Matrix RelativeTransform(double latitude, double longitude)
        {
            return new Matrix(1d, 0d, 0d, 1d, 0d, 0d);
        }

        public override Point? LocationToMap(double latitude, double longitude)
        {
            if (LocationToMapTransform == null)
            {
                throw new InvalidOperationException("The CoordinateSystem property is not set.");
            }

            try
            {
                var coordinate = LocationToMapTransform.Transform([longitude, latitude]);
                return new Point(coordinate[0], coordinate[1]);
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
                var coordinate = MapToLocationTransform.Transform([x, y]);
                return new Location(coordinate[1], coordinate[0]);
            }
            catch
            {
                return null;
            }
        }
    }
}
