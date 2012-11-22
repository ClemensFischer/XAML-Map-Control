// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINRT
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// MapBase with input event handling.
    /// </summary>
    public partial class Map : MapBase
    {
        private double mouseWheelZoom = 1d;
        private Point? mousePosition;

        public Map()
        {
            Initialize();
        }

        partial void Initialize();

        public double MouseWheelZoom
        {
            get { return mouseWheelZoom; }
            set { mouseWheelZoom = value; }
        }
    }
}
