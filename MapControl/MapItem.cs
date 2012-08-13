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
        public static readonly RoutedEvent SelectedEvent = ListBoxItem.SelectedEvent.AddOwner(typeof(MapItem));
        public static readonly RoutedEvent UnselectedEvent = ListBoxItem.UnselectedEvent.AddOwner(typeof(MapItem));

        public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(
            typeof(MapItem), new FrameworkPropertyMetadata((o, e) => ((MapItem)o).IsSelectedChanged((bool)e.NewValue)));

        private static readonly DependencyPropertyKey IsCurrentPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsCurrent", typeof(bool), typeof(MapItem), null);

        public static readonly DependencyProperty IsCurrentProperty = IsCurrentPropertyKey.DependencyProperty;

        static MapItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MapItem), new FrameworkPropertyMetadata(typeof(MapItem)));

            UIElement.IsEnabledProperty.OverrideMetadata(
                typeof(MapItem), new FrameworkPropertyMetadata((o, e) => ((MapItem)o).CommonStateChanged()));
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
            get { return (bool)GetValue(IsCurrentProperty); }
            internal set
            {
                if (IsCurrent != value)
                {
                    SetValue(IsCurrentPropertyKey, value);
                    int zIndex = Panel.GetZIndex(this);
                    Panel.SetZIndex(this, value ? (zIndex | 0x40000000) : (zIndex & ~0x40000000));
                    VisualStateManager.GoToState(this, value ? "Current" : "NotCurrent", true);
                }
            }
        }

        public object Item { get; internal set; }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            CommonStateChanged();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            CommonStateChanged();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs eventArgs)
        {
            base.OnMouseLeftButtonDown(eventArgs);
            eventArgs.Handled = true;
            IsSelected = !IsSelected;
        }

        protected override void OnTouchDown(TouchEventArgs eventArgs)
        {
            base.OnTouchDown(eventArgs);
            eventArgs.Handled = true; // get TouchUp event
        }

        protected override void OnTouchUp(TouchEventArgs eventArgs)
        {
            base.OnTouchUp(eventArgs);
            eventArgs.Handled = true;
            IsSelected = !IsSelected;
        }

        private void IsSelectedChanged(bool isSelected)
        {
            if (isSelected)
            {
                VisualStateManager.GoToState(this, "Selected", true);
                RaiseEvent(new RoutedEventArgs(SelectedEvent));
            }
            else
            {
                VisualStateManager.GoToState(this, "Unselected", true);
                RaiseEvent(new RoutedEventArgs(UnselectedEvent));
            }
        }

        private void CommonStateChanged()
        {
            if (!IsEnabled)
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
    }
}
