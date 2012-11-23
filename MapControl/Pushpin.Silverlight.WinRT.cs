// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public partial class Pushpin
    {
        public Pushpin()
        {
            DefaultStyleKey = typeof(Pushpin);
            MapPanel.AddParentMapHandlers(this);
        }
    }
}
