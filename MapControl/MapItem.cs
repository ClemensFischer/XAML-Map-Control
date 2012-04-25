using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MapControl
{
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "MouseOver")]
    [TemplateVisualState(GroupName = "SelectionStates", Name = "Unselected")]
    [TemplateVisualState(GroupName = "SelectionStates", Name = "Selected")]
    [TemplateVisualState(GroupName = "CurrentStates", Name = "NonCurrent")]
    [TemplateVisualState(GroupName = "CurrentStates", Name = "Current")]
    public class MapItem : ContentControl
    {
        public static readonly RoutedEvent SelectedEvent = ListBoxItem.SelectedEvent.AddOwner(typeof(MapItem));
        public static readonly RoutedEvent UnselectedEvent = ListBoxItem.UnselectedEvent.AddOwner(typeof(MapItem));

        public static readonly DependencyProperty LocationProperty = MapPanel.LocationProperty.AddOwner(typeof(MapItem));
        public static readonly DependencyProperty ViewPositionProperty = MapPanel.ViewPositionProperty.AddOwner(typeof(MapItem));
        public static readonly DependencyProperty ViewPositionTransformProperty = MapPanel.ViewPositionTransformProperty.AddOwner(typeof(MapItem));

        public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(
            typeof(MapItem), new FrameworkPropertyMetadata((o, e) => ((MapItem)o).IsSelectedChanged((bool)e.NewValue)));

        private static readonly DependencyPropertyKey IsCurrentPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsCurrent", typeof(bool), typeof(MapItem), null);

        public static readonly DependencyProperty IsCurrentProperty = IsCurrentPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsInsideMapBoundsPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsInsideMapBounds", typeof(bool), typeof(MapItem), null);

        public static readonly DependencyProperty IsInsideMapBoundsProperty = IsInsideMapBoundsPropertyKey.DependencyProperty;

        private object item;

        static MapItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MapItem),
                new FrameworkPropertyMetadata(typeof(MapItem)));

            UIElement.IsEnabledProperty.OverrideMetadata(typeof(MapItem),
                new FrameworkPropertyMetadata((o, e) => ((MapItem)o).CommonStateChanged()));

            MapPanel.ViewPositionPropertyKey.OverrideMetadata(typeof(MapItem),
                new FrameworkPropertyMetadata((o, e) => ((MapItem)o).ViewPositionChanged((Point)e.NewValue)));
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

        public Point? Location
        {
            get { return (Point?)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }

        public bool HasViewPosition
        {
            get { return ReadLocalValue(ViewPositionProperty) != DependencyProperty.UnsetValue; }
        }

        public Point ViewPosition
        {
            get { return (Point)GetValue(ViewPositionProperty); }
        }

        public Transform ViewPositionTransform
        {
            get { return (Transform)GetValue(ViewPositionTransformProperty); }
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
                    VisualStateManager.GoToState(this, value ? "Current" : "NonCurrent", true);
                }
            }
        }

        public bool IsInsideMapBounds
        {
            get { return (bool)GetValue(IsInsideMapBoundsProperty); }
        }

        public object Item
        {
            get { return item; }
            internal set
            {
                item = value;
                if (HasViewPosition)
                {
                    ViewPositionChanged(ViewPosition);
                }
            }
        }

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

        protected virtual void OnViewPositionChanged(Point viewPosition)
        {
        }

        private void ViewPositionChanged(Point viewPosition)
        {
            Map map = ParentMap;

            if (map != null)
            {
                SetValue(IsInsideMapBoundsPropertyKey,
                    viewPosition.X >= 0d && viewPosition.X <= map.ActualWidth &&
                    viewPosition.Y >= 0d && viewPosition.Y <= map.ActualHeight);

                OnViewPositionChanged(viewPosition);
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
    }
}
