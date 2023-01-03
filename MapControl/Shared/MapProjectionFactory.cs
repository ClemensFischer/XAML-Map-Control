// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2023 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public class MapProjectionFactory
    {
        public virtual MapProjection GetProjection(int epsgCode)
        {
            MapProjection projection = null;

            switch (epsgCode)
            {
                case WorldMercatorProjection.DefaultEpsgCode:
                    projection = new WorldMercatorProjection();
                    break;

                case WebMercatorProjection.DefaultEpsgCode:
                    projection = new WebMercatorProjection();
                    break;

                case EquirectangularProjection.DefaultEpsgCode:
                    projection = new EquirectangularProjection();
                    break;

                case UpsNorthProjection.DefaultEpsgCode:
                    projection = new UpsNorthProjection();
                    break;

                case UpsSouthProjection.DefaultEpsgCode:
                    projection = new UpsSouthProjection();
                    break;

                case var c when c >= Etrs89UtmProjection.FirstZoneEpsgCode && c <= Etrs89UtmProjection.LastZoneEpsgCode:
                    projection = new Etrs89UtmProjection(epsgCode % 100);
                    break;

                case var c when c >= Nad83UtmProjection.FirstZoneEpsgCode && c <= Nad83UtmProjection.LastZoneEpsgCode:
                    projection = new Nad83UtmProjection(epsgCode % 100);
                    break;

                case var c when c >= Wgs84UtmProjection.FirstZoneNorthEpsgCode && c <= Wgs84UtmProjection.LastZoneNorthEpsgCode:
                    projection = new Wgs84UtmProjection(epsgCode % 100, true);
                    break;

                case var c when c >= Wgs84UtmProjection.FirstZoneSouthEpsgCode && c <= Wgs84UtmProjection.LastZoneSouthEpsgCode:
                    projection = new Wgs84UtmProjection(epsgCode % 100, false);
                    break;

                default:
                    break;
            }

            return projection;
        }

        public virtual MapProjection GetProjection(string crsId)
        {
            if (crsId.StartsWith("EPSG:") && int.TryParse(crsId.Substring(5), out int epsgCode))
            {
                return GetProjection(epsgCode);
            }

            MapProjection projection = null;

            switch (crsId)
            {
                case Wgs84AutoUtmProjection.DefaultCrsId:
                    projection = new Wgs84AutoUtmProjection();
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

                case "AUTO2:97003": // proprietary CRS ID
                    projection = new AzimuthalEquidistantProjection(crsId);
                    break;

                default:
                    break;
            }

            return projection;
        }
    }
}
