// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;

namespace MapControl
{
    public partial class MapPanel
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached(
            "Location", typeof(Location), typeof(MapPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static readonly DependencyProperty BoundingBoxProperty = DependencyProperty.RegisterAttached(
            "BoundingBox", typeof(BoundingBox), typeof(MapPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsParentArrange));

        private static readonly DependencyPropertyKey ParentMapPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "ParentMap", typeof(MapBase), typeof(MapPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, ParentMapPropertyChanged));

        private static readonly DependencyPropertyKey ViewPositionPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "ViewPosition", typeof(Point?), typeof(MapPanel), new PropertyMetadata());

        public static readonly DependencyProperty ParentMapProperty = ParentMapPropertyKey.DependencyProperty;
        public static readonly DependencyProperty ViewPositionProperty = ViewPositionPropertyKey.DependencyProperty;

        public MapPanel()
        {
            if (this is MapBase)
            {
                SetValue(ParentMapPropertyKey, this);
            }
        }

        public static MapBase GetParentMap(FrameworkElement element)
        {
            return (MapBase)element.GetValue(ParentMapProperty);
        }

        private static void SetViewPosition(FrameworkElement element, Point? viewPosition)
        {
            element.SetValue(ViewPositionPropertyKey, viewPosition);
        }
    }
}
