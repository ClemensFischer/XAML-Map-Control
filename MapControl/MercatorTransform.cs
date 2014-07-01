// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINDOWS_RUNTIME
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
        private static readonly double maxLatitude = Math.Atan(Math.Sinh(Math.PI)) / Math.PI * 180d;

        public override double MaxLatitude
        {
            get { return maxLatitude; }
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
            double latitude;

            if (location.Latitude <= -90d)
            {
                latitude = double.NegativeInfinity;
            }
            else if (location.Latitude >= 90d)
            {
                latitude = double.PositiveInfinity;
            }
            else
            {
                latitude = location.Latitude * Math.PI / 180d;
                latitude = Math.Log(Math.Tan(latitude) + 1d / Math.Cos(latitude)) / Math.PI * 180d;
            }

            return new Point(location.Longitude, latitude);
        }

        public override Location Transform(Point point)
        {
            var latitude = Math.Atan(Math.Sinh(point.Y * Math.PI / 180d)) / Math.PI * 180d;

            return new Location(latitude, point.X);
        }
    }
}
