// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;

namespace MapControl
{
    /// <summary>
    /// A collection of Locations with support for string parsing
    /// and calculation of great circle and rhumb line locations.
    /// </summary>
#if !WINDOWS_UWP
    [System.ComponentModel.TypeConverter(typeof(LocationCollectionConverter))]
#endif
    public class LocationCollection : List<Location>
    {
        public LocationCollection()
        {
        }

        public LocationCollection(IEnumerable<Location> locations)
            : base(locations)
        {
        }

        public LocationCollection(params Location[] locations)
            : base(locations)
        {
        }

        public void Add(double latitude, double longitude)
        {
            if (Count > 0)
            {
                var deltaLon = longitude - this[Count - 1].Longitude;

                if (deltaLon < -180d)
                {
                    longitude += 360d;
                }
                else if (deltaLon > 180)
                {
                    longitude -= 360;
                }
            }

            Add(new Location(latitude, longitude));
        }

        public static LocationCollection Parse(string s)
        {
            var strings = s.Split(new char[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);

            return new LocationCollection(strings.Select(l => Location.Parse(l)));
        }

        /// <summary>
        /// Calculates a series of Locations on a great circle, or orthodrome, that connects the two specified Locations,
        /// with an optional angular resolution specified in degrees.
        ///
        /// See https://en.wikipedia.org/wiki/Great-circle_navigation
        /// </summary>
        public static LocationCollection OrthodromeLocations(Location location1, Location location2, double resolution = 1d)
        {
            if (resolution <= 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resolution), "The resolution argument must be greater than zero.");
            }

            var lat1 = location1.Latitude * Math.PI / 180d;
            var lon1 = location1.Longitude * Math.PI / 180d;
            var lat2 = location2.Latitude * Math.PI / 180d;
            var lon2 = location2.Longitude * Math.PI / 180d;

            var cosLat1 = Math.Cos(lat1);
            var sinLat1 = Math.Sin(lat1);
            var cosLat2 = Math.Cos(lat2);
            var sinLat2 = Math.Sin(lat2);
            var cosLon12 = Math.Cos(lon2 - lon1);
            var sinLon12 = Math.Sin(lon2 - lon1);

            var a = cosLat1 * sinLat2 - sinLat1 * cosLat2 * cosLon12;
            var b = cosLat2 * sinLon12;
            var s12 = Math.Atan2(Math.Sqrt(a * a + b * b), sinLat1 * sinLat2 + cosLat1 * cosLat2 * cosLon12);

            var n = (int)Math.Ceiling(s12 / resolution * 180d / Math.PI); // s12 in radians

            var locations = new LocationCollection(new Location(location1.Latitude, location1.Longitude));

            if (n > 1)
            {
                var az1 = Math.Atan2(sinLon12, cosLat1 * sinLat2 / cosLat2 - sinLat1 * cosLon12);
                var cosAz1 = Math.Cos(az1);
                var sinAz1 = Math.Sin(az1);

                var az0 = Math.Atan2(sinAz1 * cosLat1, Math.Sqrt(cosAz1 * cosAz1 + sinAz1 * sinAz1 * sinLat1 * sinLat1));
                var sinAz0 = Math.Sin(az0);
                var cosAz0 = Math.Cos(az0);

                var s01 = Math.Atan2(sinLat1, cosLat1 * cosAz1);
                var lon0 = lon1 - Math.Atan2(sinAz0 * Math.Sin(s01), Math.Cos(s01));

                for (var i = 1; i < n; i++)
                {
                    var s = s01 + i * s12 / n;
                    var sinS = Math.Sin(s);
                    var cosS = Math.Cos(s);
                    var lat = Math.Atan2(cosAz0 * sinS, Math.Sqrt(cosS * cosS + sinAz0 * sinAz0 * sinS * sinS));
                    var lon = Math.Atan2(sinAz0 * sinS, cosS) + lon0;

                    locations.Add(lat * 180d / Math.PI, lon * 180d / Math.PI);
                }
            }

            locations.Add(location2.Latitude, location2.Longitude);

            return locations;
        }

        /// <summary>
        /// Calculates a series of Locations on a rhumb line, or loxodrome, that connects the two specified Locations,
        /// with an optional angular resolution specified in degrees.
        ///
        /// See https://en.wikipedia.org/wiki/Rhumb_line
        /// </summary>
        public static LocationCollection LoxodromeLocations(Location location1, Location location2, double resolution = 1d)
        {
            if (resolution <= 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resolution), "The resolution argument must be greater than zero.");
            }

            var lat1 = location1.Latitude;
            var lon1 = location1.Longitude;
            var lat2 = location2.Latitude;
            var lon2 = location2.Longitude;

            var y1 = WebMercatorProjection.LatitudeToY(lat1);
            var y2 = WebMercatorProjection.LatitudeToY(lat2);

            if (double.IsInfinity(y1))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(location1), "The location1 argument must have an absolute latitude value of less than 90.");
            }

            if (double.IsInfinity(y2))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(location2), "The location2 argument must have an absolute latitude value of less than 90.");
            }

            var dlat = lat2 - lat1;
            var dlon = lon2 - lon1;
            var dy = y2 - y1;

            // beta = atan(dlon,dy)
            // sec(beta) = 1 / cos(atan(dlon,dy)) = sqrt(1 + (dlon/dy)^2)

            var sec = Math.Sqrt(1d + dlon * dlon / (dy * dy));

            const double secLimit = 1000d; // beta approximately +/-90°

            double s12;

            if (sec > secLimit)
            {
                var lat = (lat1 + lat2) * Math.PI / 360d; // mean latitude

                s12 = Math.Abs(dlon * Math.Cos(lat)); // distance in degrees along parallel of latitude
            }
            else
            {
                s12 = Math.Abs(dlat * sec); // distance in degrees along loxodrome
            }

            var n = (int)Math.Ceiling(s12 / resolution);

            var locations = new LocationCollection(new Location(lat1, lon1));

            if (sec > secLimit)
            {
                for (var i = 1; i < n; i++)
                {
                    var lon = lon1 + i * dlon / n;
                    var lat = WebMercatorProjection.YToLatitude(y1 + i * dy / n);
                    locations.Add(lat, lon);
                }
            }
            else
            {
                for (var i = 1; i < n; i++)
                {
                    var lat = lat1 + i * dlat / n;
                    var lon = lon1 + dlon * (WebMercatorProjection.LatitudeToY(lat) - y1) / dy;
                    locations.Add(lat, lon);
                }
            }

            locations.Add(lat2, lon2);

            return locations;
        }
    }
}
