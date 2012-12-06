// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINRT
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
#endif

namespace MapControl
{
    /// <summary>
    /// Displays a pushpin at a geographic location provided by the MapPanel.Location attached property.
    /// </summary>
    public class Pushpin : ContentControl
    {
        public Pushpin()
        {
            DefaultStyleKey = typeof(Pushpin);
            MapPanel.AddParentMapHandlers(this);
        }
    }
}
