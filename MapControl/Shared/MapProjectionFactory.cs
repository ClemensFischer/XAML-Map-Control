// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

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

                case UpsNorthProjection.DefaultCrsId:
                    projection = new UpsNorthProjection();
                    break;

                case UpsSouthProjection.DefaultCrsId:
                    projection = new UpsSouthProjection();
                    break;

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

        public virtual MapProjection GetProjection(int epsgCode)
        {
            switch (epsgCode)
            {
                case var c when c >= Etrs89UtmProjection.FirstZoneEpsgCode && c <= Etrs89UtmProjection.LastZoneEpsgCode:
                    return new Etrs89UtmProjection(epsgCode % 100);

                case var c when c >= Nad27UtmProjection.FirstZoneEpsgCode && c <= Nad27UtmProjection.LastZoneEpsgCode:
                    return new Nad27UtmProjection(epsgCode % 100);

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
