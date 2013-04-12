// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;

namespace MapControl
{
    /// <summary>
    /// Container class for an item in a MapItemsControl.
    /// </summary>
    [TemplateVisualState(Name = "NotCurrent", GroupName = "CurrentStates")]
    [TemplateVisualState(Name = "Current", GroupName = "CurrentStates")]
    public class MapItem : ListBoxItem
    {
        public static readonly DependencyProperty IsCurrentProperty = MapItemsControl.IsCurrentProperty.AddOwner(
            typeof(MapItem), new PropertyMetadata((o, e) => ((MapItem)o).IsCurrentPropertyChanged((bool)e.NewValue)));

        static MapItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MapItem), new FrameworkPropertyMetadata(typeof(MapItem)));
        }

        /// <summary>
        /// Gets a value that indicates if the MapItem is the CurrentItem of the containing items collection.
        /// </summary>
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
