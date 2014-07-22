// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public interface IMapElement
    {
        MapBase ParentMap { get; }

        void SetParentMap(MapBase parentMap);
    }
}
