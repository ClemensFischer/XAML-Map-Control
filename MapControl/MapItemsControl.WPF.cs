// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.ComponentModel;
using System.Windows;

namespace MapControl
{
    public partial class MapItemsControl
    {
        public static readonly DependencyProperty IsCurrentProperty = DependencyProperty.RegisterAttached(
            "IsCurrent", typeof(bool), typeof(MapItemsControl), null);

        static MapItemsControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MapItemsControl), new FrameworkPropertyMetadata(typeof(MapItemsControl)));
        }

        partial void Initialize()
        {
            Items.CurrentChanging += OnCurrentItemChanging;
            Items.CurrentChanged += OnCurrentItemChanged;
        }

        public static bool GetIsCurrent(UIElement element)
        {
            return (bool)element.GetValue(IsCurrentProperty);
        }

        private static void SetIsCurrent(UIElement element, bool value)
        {
            element.SetValue(IsCurrentProperty, value);
        }

        private void OnCurrentItemChanging(object sender, CurrentChangingEventArgs e)
        {
            var container = ContainerFromItem(Items.CurrentItem);

            if (container != null)
            {
                SetIsCurrent(container, false);
            }
        }

        private void OnCurrentItemChanged(object sender, EventArgs e)
        {
            var container = ContainerFromItem(Items.CurrentItem);

            if (container != null)
            {
                SetIsCurrent(container, true);
            }
        }
    }
}