// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    public class ViewportChangedEventArgs : EventArgs
    {
        public ViewportChangedEventArgs(bool projectionChanged = false, double longitudeOffset = 0d)
        {
            ProjectionChanged = projectionChanged;
            LongitudeOffset = longitudeOffset;
        }

        /// <summary>
        /// Indicates if the map projection has changed, i.e. if a MapTileLayer or MapImageLayer should
        /// be updated immediately, or MapShape Data in cartesian map coordinates should be recalculated.
        /// </summary>
        public bool ProjectionChanged { get; }

        /// <summary>
        /// Offset of the map center longitude value from the previous viewport.
        /// Used to detect if the map center has moved across 180° longitude.
        /// </summary>
        public double LongitudeOffset { get; }
    }
}
