// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.UI.Xaml.Shapes;

namespace MapControl
{
    public partial class MapPolyline : Path
    {
        partial void Initialize()
        {
            Data = Geometry;
        }
    }
}
