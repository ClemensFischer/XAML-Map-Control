// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if NETFX_CORE
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Transforms latitude and longitude values in degrees to cartesian coordinates
    /// according to the Mercator projection.
    /// </summary>
    public class MercatorTransform : MapTransform
    {
        public override double MaxLatitude
        {
            get { return 85.0511; }
        }

        public override double RelativeScale(Location location)
        {
            if (location.Latitude <= -90d)
            {
                return double.NegativeInfinity;
            }

            if (location.Latitude >= 90d)
            {
                return double.PositiveInfinity;
            }

            return 1d / Math.Cos(location.Latitude * Math.PI / 180d);
        }

        public override Point Transform(Location location)
        {
            if (double.IsNaN(location.Y))
            {
                if (location.Latitude <= -90d)
                {
                    location.Y = double.NegativeInfinity;
                }
                else if (location.Latitude >= 90d)
                {
                    location.Y = double.PositiveInfinity;
                }
                else
                {
                    var lat = location.Latitude * Math.PI / 180d;
                    location.Y = (Math.Log(Math.Tan(lat) + 1d / Math.Cos(lat))) / Math.PI * 180d;
                }
            }

            return new Point(location.Longitude, location.Y);
        }

        public override Location Transform(Point point)
        {
            var location = new Location(Math.Atan(Math.Sinh(point.Y * Math.PI / 180d)) / Math.PI * 180d, point.X);
            location.Y = point.Y;
            return location;
        }
    }
}
