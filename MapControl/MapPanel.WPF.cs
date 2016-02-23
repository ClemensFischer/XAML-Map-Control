// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;

namespace MapControl
{
    public partial class MapPanel
    {
        private static readonly DependencyPropertyKey ParentMapPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "ParentMap", typeof(MapBase), typeof(MapPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, ParentMapPropertyChanged));

        public static readonly DependencyProperty ParentMapProperty = ParentMapPropertyKey.DependencyProperty;

        public MapPanel()
        {
            if (this is MapBase)
            {
                SetValue(ParentMapPropertyKey, this);
            }
        }

        public static MapBase GetParentMap(UIElement element)
        {
            return (MapBase)element.GetValue(ParentMapProperty);
        }
    }
}
