// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Controls;
using Avalonia.Media;

namespace MapControl
{
    public partial class MapPanel
    {
        public MapPanel()
        {
            if (this is MapBase mapBase)
            {
                SetValue(ParentMapProperty, mapBase);
            }
        }

        public static MapBase GetParentMap(Control element)
        {
            return (MapBase)element.GetValue(ParentMapProperty);
        }

        public static void SetRenderTransform(Control element, Transform transform, double originX = 0d, double originY = 0d)
        {
            element.RenderTransform = transform;
            element.RenderTransformOrigin = new RelativePoint(originX, originY, RelativeUnit.Relative);
        }

        private Controls ChildElements => Children;

        private static void SetVisible(Control element, bool visible)
        {
            element.IsVisible = visible;
        }
    }
}
