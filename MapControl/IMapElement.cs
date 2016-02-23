// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public interface IMapElement
    {
        MapBase ParentMap { get; set; }
    }
}
