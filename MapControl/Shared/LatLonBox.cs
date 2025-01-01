// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    /// <summary>
    /// A BoundingBox with optional rotation. Used by GeoImage and GroundOverlay.
    /// </summary>
    public class LatLonBox : BoundingBox
    {
        public LatLonBox(double latitude1, double longitude1, double latitude2, double longitude2, double rotation = 0d)
            : base(latitude1, longitude1, latitude2, longitude2)
        {
            Rotation = rotation;
        }

        public LatLonBox(Location location1, Location location2, double rotation = 0d)
            : base(location1, location2)
        {
            Rotation = rotation;
        }

        public LatLonBox(BoundingBox boundingBox, double rotation = 0d)
            : base(boundingBox.South, boundingBox.West, boundingBox.North, boundingBox.East)
        {
            Rotation = rotation;
        }

        /// <summary>
        /// Gets a counterclockwise rotation angle in degrees.
        /// </summary>
        public double Rotation { get; }
    }
}
