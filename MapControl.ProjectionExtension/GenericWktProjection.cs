using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using MapControl;
using ProjNet.Converters.WellKnownText;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace MapControl.ProjectionExtension
{
    /// <summary>
    /// Create all kind of specific projection by wkt. for example from epsg.io
    /// </summary>
    public class GenericWktProjection : MapProjection
    {
        private readonly ICoordinateTransformation _trans;
        private readonly IMathTransform _inversedTransform;

        public GenericWktProjection(string wktConfig, string crsId) : this(crsId)
        {
            if (String.IsNullOrEmpty(wktConfig))
                throw new ArgumentNullException(nameof(wktConfig));

            ICoordinateSystem CoorSystem = CoordinateSystemWktReader.Parse(wktConfig, Encoding.UTF8) as ICoordinateSystem;

            IGeographicCoordinateSystem WGSSystem = GeographicCoordinateSystem.WGS84;
            CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();
            _trans = ctfac.CreateFromCoordinateSystems(WGSSystem, CoorSystem);
            _inversedTransform = ctfac.CreateFromCoordinateSystems(CoorSystem, WGSSystem).MathTransform;
        }

        protected GenericWktProjection(string crsId)
        {
            if (String.IsNullOrEmpty(crsId))
                throw new ArgumentNullException(nameof(crsId));

            CrsId = crsId;
            IsWebMercator = false;
            this.IsContinuous = false;
        }

        public override Point LocationToPoint(Location location)
        {
            double[] fromPoint = new double[] { location.Longitude, location.Latitude };//because geolib changes axis
            double[] toPoint = _trans.MathTransform.Transform(fromPoint);
            return new Point(toPoint[0], toPoint[1]);
        }

        public override Location PointToLocation(Point point)
        {
            double[] ret = _inversedTransform.Transform(new double[] { point.X, point.Y });
            return new Location(ret[1], ret[0]);//because geolib changes axis
        }
        public override Vector GetMapScale(Location location)
        {
            return new Vector(1, 1);
        }
    }
}
