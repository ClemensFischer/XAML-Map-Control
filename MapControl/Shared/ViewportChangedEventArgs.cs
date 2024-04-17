// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    public class ViewportChangedEventArgs : EventArgs
    {
        public ViewportChangedEventArgs(bool projectionChanged = false, bool transformCenterChanged = false)
        {
            ProjectionChanged = projectionChanged;
            TransformCenterChanged = transformCenterChanged;
        }

        /// <summary>
        /// Indicates that the map projection has changed. Used to control when
        /// a MapTileLayer or a MapImageLayer should be updated immediately,
        /// or MapPath Data in projected map coordinates should be recalculated.
        /// </summary>
        public bool ProjectionChanged { get; }

        /// <summary>
        /// Indicates that the view transform center has moved across 180° longitude.
        /// Used to control when a MapTileLayer should be updated immediately.
        /// </summary>
        public bool TransformCenterChanged { get; }
    }
}
