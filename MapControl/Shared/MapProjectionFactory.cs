// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public class MapProjectionFactory
    {
        public virtual MapProjection GetProjection(string crsId)
        {
            MapProjection projection = null;

            switch (crsId)
            {
                case WorldMercatorProjection.DefaultCrsId:
                    projection = new WorldMercatorProjection();
                    break;

                case WebMercatorProjection.DefaultCrsId:
                    projection = new WebMercatorProjection();
                    break;

                case EquirectangularProjection.DefaultCrsId:
                    projection = new EquirectangularProjection();
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

                case "EPSG:97003": // proprietary CRS ID
                    projection = new AzimuthalEquidistantProjection(crsId);
                    break;

                default:
                    break;
            }

            return projection;
        }
    }
}
