// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapControl
{
    public partial class MapItemsControl
    {
        static MapItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapItemsControl), new FrameworkPropertyMetadata(typeof(MapItemsControl)));
        }

        public FrameworkElement ContainerFromItem(object item)
        {
            return (FrameworkElement)ItemContainerGenerator.ContainerFromItem(item);
        }

        public object ItemFromContainer(FrameworkElement container)
        {
            return ItemContainerGenerator.ItemFromContainer(container);
        }

        public void SelectItemsInGeometry(Geometry geometry)
        {
            SelectItemsByPosition(p => geometry.FillContains(p));
        }
    }
}
