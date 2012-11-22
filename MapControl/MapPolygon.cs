// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public class MapPolygon : MapPolyline
    {
        protected override bool IsClosed
        {
            get { return true; }
        }
    }
}
