#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    /// <summary>
    /// Base class for azimuthal map projections.
    /// </summary>
    public abstract class AzimuthalProjection : MapProjection
    {
        protected AzimuthalProjection()
        {
            Type = MapProjectionType.Azimuthal;
        }

        public override Rect? BoundingBoxToMap(BoundingBox boundingBox)
        {
            Rect? rect = null;
            var center = LocationToMap(boundingBox.Center);

            if (center.HasValue)
            {
                var width = boundingBox.Width * Wgs84MeterPerDegree;
                var height = boundingBox.Height * Wgs84MeterPerDegree;
                var x = center.Value.X - width / 2d;
                var y = center.Value.Y - height / 2d;

                rect = new Rect(x, y, width, height);
            }

            return rect;
        }

        public override BoundingBox MapToBoundingBox(Rect rect)
        {
            BoundingBox boundingBox = null;
            var rectCenter = new Point(rect.X + rect.Width / 2d, rect.Y + rect.Height / 2d);
            var center = MapToLocation(rectCenter);

            if (center != null)
            {
                boundingBox = new CenteredBoundingBox(center, rect.Width / Wgs84MeterPerDegree, rect.Height / Wgs84MeterPerDegree);
            }

            return boundingBox;
        }
    }
}
