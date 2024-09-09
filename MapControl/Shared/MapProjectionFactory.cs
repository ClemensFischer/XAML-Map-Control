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
                    return null;
            }
        }
    }
}
