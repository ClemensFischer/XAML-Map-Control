// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;

namespace MapControl.Projections
{
    public class GeoApiProjectionFactory : MapProjectionFactory
    {
        private readonly Dictionary<int, string> wkts = new Dictionary<int, string>();
        private readonly HttpClient httpClient = new HttpClient();

        public override MapProjection CreateProjection(string projectionDefinition)
        {
            var projection = base.CreateProjection(projectionDefinition);

            if (projection == null &&
                projectionDefinition.StartsWith("EPSG:") &&
                int.TryParse(projectionDefinition.Substring(5), out int epsgCode))
            {
                if (epsgCode >= 32601 && epsgCode <= 32660)
                {
                    projection = new UtmProjection(epsgCode - 32600, true);
                }
                else if (epsgCode == 32661)
                {
                    projection = new UpsNorthProjection();
                }
                else if (epsgCode >= 32701 && epsgCode <= 32760)
                {
                    projection = new UtmProjection(epsgCode - 32700, false);
                }
                else if (epsgCode == 32761)
                {
                    projection = new UpsSouthProjection();
                }
                else
                {
                    projection = new GeoApiProjection(GetWkt(epsgCode));
                }
            }

            return projection;
        }

        private string GetWkt(int epsgCode)
        {
            if (!wkts.TryGetValue(epsgCode, out string wkt))
            {
                var url = string.Format("https://epsg.io/{0}.wkt", epsgCode);

                try
                {
                    wkt = httpClient.GetStringAsync(url).Result; // potential deadlock?
                    wkts[epsgCode] = wkt;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"GeoApiProjectionFactory.GetWkt({epsgCode}): {url}: {ex.Message}");
                }
            }

            return wkt;
        }
    }
}
