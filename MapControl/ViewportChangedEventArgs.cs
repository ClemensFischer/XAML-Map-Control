// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    public class ViewportChangedEventArgs : EventArgs
    {
        public ViewportChangedEventArgs(double originOffset)
        {
            OriginOffset = originOffset;
        }

        /// <summary>
        /// Offset of the X value of the map coordinate origin from the previous viewport.
        /// Used to detect if the map center has moved across 180° longitude.
        /// </summary>
        public double OriginOffset { get; }
    }
}
