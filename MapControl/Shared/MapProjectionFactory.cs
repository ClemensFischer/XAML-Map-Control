// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public class MapProjectionFactory
    {
        private static MapProjectionFactory instance;

        public static MapProjectionFactory Instance
        {
            get => instance ?? (instance = new MapProjectionFactory());
            set => instance = value;
        }

        public virtual MapProjection GetProjection(string crsId)
        {
            switch (crsId)
            {
                case WebMercatorProjection.DefaultCrsId:
                    return new WebMercatorProjection();

                case WorldMercatorProjection.DefaultCrsId:
                    return new WorldMercatorProjection();

                case EquirectangularProjection.DefaultCrsId:
                case "CRS:84":
                case "EPSG:4087":
                    return new EquirectangularProjection(crsId);

                case UpsNorthProjection.DefaultCrsId:
                    return new UpsNorthProjection();

                case UpsSouthProjection.DefaultCrsId:
                    return new UpsSouthProjection();

                case Wgs84AutoUtmProjection.DefaultCrsId:
                    return new Wgs84AutoUtmProjection();

                case OrthographicProjection.DefaultCrsId:
                    return new OrthographicProjection();

                case AutoEquirectangularProjection.DefaultCrsId:
                    return new AutoEquirectangularProjection();

                case GnomonicProjection.DefaultCrsId:
                    return new GnomonicProjection();

                case StereographicProjection.DefaultCrsId:
                    return new StereographicProjection();

                case AzimuthalEquidistantProjection.DefaultCrsId:
                    return new AzimuthalEquidistantProjection();

                default:
                    return crsId.StartsWith("EPSG:") && int.TryParse(crsId.Substring(5), out int epsgCode)
                        ? GetProjection(epsgCode)
                        : null;
            }
        }

        public virtual MapProjection GetProjection(int epsgCode)
        {
            switch (epsgCode)
            {
                case var c when c >= Etrs89UtmProjection.FirstZoneEpsgCode && c <= Etrs89UtmProjection.LastZoneEpsgCode:
                    return new Etrs89UtmProjection(epsgCode % 100);

                case var c when c >= Nad83UtmProjection.FirstZoneEpsgCode && c <= Nad83UtmProjection.LastZoneEpsgCode:
                    return new Nad83UtmProjection(epsgCode % 100);

                case var c when c >= Wgs84UtmProjection.FirstZoneNorthEpsgCode && c <= Wgs84UtmProjection.LastZoneNorthEpsgCode:
                    return new Wgs84UtmProjection(epsgCode % 100, true);

                case var c when c >= Wgs84UtmProjection.FirstZoneSouthEpsgCode && c <= Wgs84UtmProjection.LastZoneSouthEpsgCode:
                    return new Wgs84UtmProjection(epsgCode % 100, false);

                default:
                    return null;
            }
        }
    }
}
