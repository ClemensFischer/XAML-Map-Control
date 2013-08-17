// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;

namespace MapControl
{
    /// <summary>
    /// Container class for an item in a MapItemsControl.
    /// </summary>
    public class MapItem : ListBoxItem
    {
        static MapItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MapItem), new FrameworkPropertyMetadata(typeof(MapItem)));
        }
    }
}
