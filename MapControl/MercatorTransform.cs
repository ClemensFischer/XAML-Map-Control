// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Transforms latitude and longitude values in degrees to cartesian coordinates
    /// according to the Mercator transform.
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
            Point result = new Point(location.Longitude, 0d);

            if (location.Latitude <= -90d)
            {
                result.Y = double.NegativeInfinity;
            }
            else if (location.Latitude >= 90d)
            {
                result.Y = double.PositiveInfinity;
            }
            else
            {
                double lat = location.Latitude * Math.PI / 180d;
                result.Y = (Math.Log(Math.Tan(lat) + 1d / Math.Cos(lat))) / Math.PI * 180d;
            }

            return result;
        }

        public override Location TransformBack(Point point)
        {
            return new Location(Math.Atan(Math.Sinh(point.Y * Math.PI / 180d)) / Math.PI * 180d, point.X);
        }
    }
}
