// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    /// <summary>
    /// MapBase with input event handling.
    /// </summary>
    public partial class Map : MapBase
    {
        private const double MouseWheelDelta = 120;

        /// <summary>
        /// Gets or sets the amount by which the ZoomLevel property changes during a MouseWheel event.
        /// </summary>
        public double MouseWheelZoomChange { get; set; }
    }
}
