// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    /// <summary>
    /// A BoundingBox with optional rotation. Used by GeoImage and GroundOverlay.
    /// </summary>
    public class LatLonBox : BoundingBox
    {
        public LatLonBox(double latitude1, double longitude1, double latitude2, double longitude2, double rotation)
            : base(latitude1, longitude1, latitude2, longitude2)
        {
            Rotation = rotation;
        }

        public LatLonBox(Location location1, Location location2, double rotation)
            : base(location1, location2)
        {
            Rotation = rotation;
        }

        /// <summary>
        /// Gets a counterclockwise rotation angle in degrees.
        /// </summary>
        public double Rotation { get; }
    }
}
