// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;

namespace MapControl
{
    [TemplateVisualState(Name = "NotCurrent", GroupName = "CurrentStates")]
    [TemplateVisualState(Name = "Current", GroupName = "CurrentStates")]
    public partial class MapItem
    {
        public static readonly DependencyProperty IsCurrentProperty = MapItemsControl.IsCurrentProperty.AddOwner(
            typeof(MapItem),
            new PropertyMetadata((o, e) => ((MapItem)o).IsCurrentPropertyChanged((bool)e.NewValue)));

        static MapItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MapItem), new FrameworkPropertyMetadata(typeof(MapItem)));
        }

        public bool IsCurrent
        {
            get { return (bool)GetValue(IsCurrentProperty); }
        }

        private void IsCurrentPropertyChanged(bool isCurrent)
        {
            var zIndex = Panel.GetZIndex(this);

            if (isCurrent)
            {
                Panel.SetZIndex(this, zIndex | 0x40000000);
                VisualStateManager.GoToState(this, "Current", true);
            }
            else
            {
                Panel.SetZIndex(this, zIndex & ~0x40000000);
                VisualStateManager.GoToState(this, "NotCurrent", true);
            }
        }
    }
}
