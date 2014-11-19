// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;

namespace MapControl
{
    public partial class TileLayer
    {
        static TileLayer()
        {
            UIElement.IsHitTestVisibleProperty.OverrideMetadata(
                typeof(TileLayer), new FrameworkPropertyMetadata(false));
        }
    }
}
