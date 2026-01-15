using System;

namespace MapControl
{
    public class MapProjectionFactory
    {
        public virtual MapProjection GetProjection(string crsId)
        {
            MapProjection projection = null;
            
            switch (crsId)
            {
                case WebMercatorProjection.DefaultCrsId:
                    projection = new WebMercatorProjection();
                    break;

                case WorldMercatorProjection.DefaultCrsId:
                    projection = new WorldMercatorProjection();
                    break;

                case EquirectangularProjection.DefaultCrsId:
                case "CRS:84":
                case "EPSG:4087":
                    projection = new EquirectangularProjection(crsId);
                    break;

                case Wgs84UpsNorthProjection.DefaultCrsId:
                    projection = new Wgs84UpsNorthProjection();
                    break;

                case Wgs84UpsSouthProjection.DefaultCrsId:
                    projection = new Wgs84UpsSouthProjection();
                    break;

                case Wgs84AutoUtmProjection.DefaultCrsId:
                    projection = new Wgs84AutoUtmProjection();
                    break;

                case Wgs84AutoTmProjection.DefaultCrsId:
                    projection = new Wgs84AutoTmProjection();
                    break;

                case OrthographicProjection.DefaultCrsId:
                    projection = new OrthographicProjection();
                    break;

                case AutoEquirectangularProjection.DefaultCrsId:
                    projection = new AutoEquirectangularProjection();
                    break;

                case GnomonicProjection.DefaultCrsId:
                    projection = new GnomonicProjection();
                    break;

                case StereographicProjection.DefaultCrsId:
                    projection = new StereographicProjection();
                    break;

                case AzimuthalEquidistantProjection.DefaultCrsId:
                    projection = new AzimuthalEquidistantProjection();
                    break;

                default:
                    if (crsId.StartsWith("EPSG:") && int.TryParse(crsId.Substring(5), out int epsgCode))
                    {
                        projection = GetProjection(epsgCode);
                    }
                    break;
            }

            return projection ?? throw new NotSupportedException($"MapProjection \"{crsId}\" is not supported.");
        }

        public virtual MapProjection GetProjection(int epsgCode) => epsgCode switch
        {
            var code when code >= Etrs89UtmProjection.FirstZoneEpsgCode && code <= Etrs89UtmProjection.LastZoneEpsgCode => new Etrs89UtmProjection(epsgCode % 100),
            var code when code >= Nad27UtmProjection.FirstZoneEpsgCode && code <= Nad27UtmProjection.LastZoneEpsgCode => new Nad27UtmProjection(epsgCode % 100),
            var code when code >= Nad83UtmProjection.FirstZoneEpsgCode && code <= Nad83UtmProjection.LastZoneEpsgCode => new Nad83UtmProjection(epsgCode % 100),
            var code when code >= Wgs84UtmProjection.FirstZoneNorthEpsgCode && code <= Wgs84UtmProjection.LastZoneNorthEpsgCode => new Wgs84UtmProjection(epsgCode % 100, Hemisphere.North),
            var code when code >= Wgs84UtmProjection.FirstZoneSouthEpsgCode && code <= Wgs84UtmProjection.LastZoneSouthEpsgCode => new Wgs84UtmProjection(epsgCode % 100, Hemisphere.South),
            _ => null
        };
    }
}
