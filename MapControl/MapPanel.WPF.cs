// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;

namespace MapControl
{
    public partial class MapPanel
    {
        public static readonly DependencyProperty ParentMapProperty = DependencyProperty.RegisterAttached(
            "ParentMap", typeof(MapBase), typeof(MapPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, ParentMapPropertyChanged));

        public static MapBase GetParentMap(UIElement element)
        {
            return (MapBase)element.GetValue(ParentMapProperty);
        }
    }
}
