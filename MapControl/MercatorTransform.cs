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
        private GeneralTransform inverse = new InverseMercatorTransform();

        public MercatorTransform()
        {
            Freeze();
        }

        public override GeneralTransform Inverse
        {
            get { return inverse; }
        }

        public override double MaxLatitude
        {
            get { return 85.0511; }
        }

        public override double RelativeScale(Point point)
        {
            if (point.Y <= -90d)
            {
                return double.NegativeInfinity;
            }

            if (point.Y >= 90d)
            {
                return double.PositiveInfinity;
            }

            return 1d / Math.Cos(point.Y * Math.PI / 180d);
        }

        public override bool TryTransform(Point point, out Point result)
        {
            result = point;

            if (point.Y <= -90d)
            {
                result.Y = double.NegativeInfinity;
            }
            else if (point.Y >= 90d)
            {
                result.Y = double.PositiveInfinity;
            }
            else
            {
                double lat = point.Y * Math.PI / 180d;
                result.Y = (Math.Log(Math.Tan(lat) + 1d / Math.Cos(lat))) / Math.PI * 180d;
            }

            return true;
        }

        public override Rect TransformBounds(Rect rect)
        {
            return new Rect(Transform(rect.TopLeft), Transform(rect.BottomRight));
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MercatorTransform();
        }
    }

    /// <summary>
    /// Transforms cartesian coordinates to latitude and longitude values in degrees
    /// according to the inverse Mercator transform.
    /// </summary>
    public class InverseMercatorTransform : GeneralTransform
    {
        public InverseMercatorTransform()
        {
            Freeze();
        }

        public override GeneralTransform Inverse
        {
            get { return new MercatorTransform(); }
        }

        public override bool TryTransform(Point point, out Point result)
        {
            result = point;
            result.Y = Math.Atan(Math.Sinh(point.Y * Math.PI / 180d)) / Math.PI * 180d;
            return true;
        }

        public override Rect TransformBounds(Rect rect)
        {
            return new Rect(Transform(rect.TopLeft), Transform(rect.BottomRight));
        }

        protected override Freezable CreateInstanceCore()
        {
            return new InverseMercatorTransform();
        }
    }
}
