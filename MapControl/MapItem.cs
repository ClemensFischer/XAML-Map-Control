// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MapControl
{
    /// <summary>
    /// Container class for an item in a MapItemsControl.
    /// </summary>
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Unselected", GroupName = "SelectionStates")]
    [TemplateVisualState(Name = "Selected", GroupName = "SelectionStates")]
    [TemplateVisualState(Name = "NotCurrent", GroupName = "CurrentStates")]
    [TemplateVisualState(Name = "Current", GroupName = "CurrentStates")]
    public class MapItem : ContentControl
    {
        public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(MapItem));
        public static readonly RoutedEvent UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(MapItem));

        public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(
            typeof(MapItem),
            new FrameworkPropertyMetadata((o, e) => ((MapItem)o).IsSelectedPropertyChanged((bool)e.NewValue)));

        static MapItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MapItem), new FrameworkPropertyMetadata(typeof(MapItem)));

            MapItemsControl.IsCurrentPropertyKey.OverrideMetadata(
                typeof(MapItem),
                new FrameworkPropertyMetadata((o, e) => ((MapItem)o).IsCurrentPropertyChanged((bool)e.NewValue)));
        }

        public MapItem()
        {
            IsEnabledChanged += IsEnabledPropertyChanged; 
        }

        public Map ParentMap
        {
            get { return MapPanel.GetParentMap(this); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public bool IsCurrent
        {
            get { return MapItemsControl.GetIsCurrent(this); }
        }

        public event RoutedEventHandler Selected
        {
            add { AddHandler(SelectedEvent, value); }
            remove { RemoveHandler(SelectedEvent, value); }
        }

        public event RoutedEventHandler Unselected
        {
            add { AddHandler(UnselectedEvent, value); }
            remove { RemoveHandler(UnselectedEvent, value); }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (IsEnabled)
            {
                VisualStateManager.GoToState(this, "MouseOver", true);
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (IsEnabled)
            {
                VisualStateManager.GoToState(this, "Normal", true);
            }
        }

        private void IsEnabledPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                VisualStateManager.GoToState(this, "Disabled", true);
            }
            else if (IsMouseOver)
            {
                VisualStateManager.GoToState(this, "MouseOver", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", true);
            }
        }

        private void IsSelectedPropertyChanged(bool isSelected)
        {
            if (isSelected)
            {
                RaiseEvent(new RoutedEventArgs(SelectedEvent));
                VisualStateManager.GoToState(this, "Selected", true);
            }
            else
            {
                RaiseEvent(new RoutedEventArgs(UnselectedEvent));
                VisualStateManager.GoToState(this, "Unselected", true);
            }
        }

        private void IsCurrentPropertyChanged(bool isCurrent)
        {
            int zIndex = Panel.GetZIndex(this);

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
