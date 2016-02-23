// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
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
        public static readonly double MaxLatitudeValue = YToLatitude(180d);

        public static double RelativeScale(double latitude)
        {
            if (latitude <= -90d)
            {
                return double.NegativeInfinity;
            }

            if (latitude >= 90d)
            {
                return double.PositiveInfinity;
            }

            return 1d / Math.Cos(latitude * Math.PI / 180d);
        }

        public static double LatitudeToY(double latitude)
        {
            if (latitude <= -90d)
            {
                return double.NegativeInfinity;
            }

            if (latitude >= 90d)
            {
                return double.PositiveInfinity;
            }

            return Math.Log(Math.Tan((latitude + 90d) * Math.PI / 360d)) / Math.PI * 180d;
        }

        public static double YToLatitude(double y)
        {
            return Math.Atan(Math.Exp(y * Math.PI / 180d)) / Math.PI * 360d - 90d;
        }

        public override double MaxLatitude
        {
            get { return MaxLatitudeValue; }
        }

        public override double RelativeScale(Location location)
        {
            return RelativeScale(location.Latitude);
        }

        public override Point Transform(Location location)
        {
            return new Point(location.Longitude, LatitudeToY(location.Latitude));
        }

        public override Location Transform(Point point)
        {
            return new Location(YToLatitude(point.Y), point.X);
        }
    }
}
