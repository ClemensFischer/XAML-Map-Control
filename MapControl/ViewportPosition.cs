// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;

namespace MapControl
{
    /// <summary>
    /// Viewport position of a MapPanel child element.
    /// </summary>
    public class ViewportPosition
    {
        public ViewportPosition(Point position, bool isInside)
        {
            Position = position;
            IsInside = isInside;
        }

        public Point Position { get; private set; }
        public bool IsInside { get; private set; }
    }
}
