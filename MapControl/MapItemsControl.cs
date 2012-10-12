// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>
    /// Manages a collection of selectable items on a Map. Uses MapItem as container for items
    /// and updates the IsCurrent attached property when the Items.CurrentItem property changes.
    /// </summary>
    public class MapItemsControl : MultiSelector
    {
        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(
            "SelectionMode", typeof(SelectionMode), typeof(MapItemsControl),
            new FrameworkPropertyMetadata((o, e) => ((MapItemsControl)o).CanSelectMultipleItems = (SelectionMode)e.NewValue != SelectionMode.Single));

        public static readonly DependencyProperty SelectionGeometryProperty = DependencyProperty.Register(
            "SelectionGeometry", typeof(Geometry), typeof(MapItemsControl),
            new FrameworkPropertyMetadata((o, e) => ((MapItemsControl)o).SelectionGeometryPropertyChanged((Geometry)e.NewValue)));

        internal static readonly DependencyPropertyKey IsCurrentPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "IsCurrent", typeof(bool), typeof(MapItemsControl), null);

        public static readonly DependencyProperty IsCurrentProperty = IsCurrentPropertyKey.DependencyProperty;

        public MapItemsControl()
        {
            Style = (Style)FindResource(typeof(MapItemsControl));
            CanSelectMultipleItems = SelectionMode != SelectionMode.Single;
            Items.CurrentChanging += OnCurrentItemChanging;
            Items.CurrentChanged += OnCurrentItemChanged;
        }

        public MapBase ParentMap
        {
            get { return MapPanel.GetParentMap(this); }
        }

        public SelectionMode SelectionMode
        {
            get { return (SelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        public Geometry SelectionGeometry
        {
            get { return (Geometry)GetValue(SelectionGeometryProperty); }
            set { SetValue(SelectionGeometryProperty, value); }
        }

        public static bool GetIsCurrent(UIElement element)
        {
            return (bool)element.GetValue(IsCurrentProperty);
        }

        public static void SetIsCurrent(UIElement element, bool value)
        {
            element.SetValue(IsCurrentPropertyKey, value);
        }

        public UIElement ContainerFromItem(object item)
        {
            return item != null ? ItemContainerGenerator.ContainerFromItem(item) as UIElement : null;
        }

        public object ItemFromContainer(DependencyObject container)
        {
            return container != null ? ItemContainerGenerator.ItemFromContainer(container) : null;
        }

        public IList GetItemsInGeometry(Geometry geometry)
        {
            return GetItemsInGeometry(geometry, new ArrayList());
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is UIElement;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MapItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            UIElement container = (UIElement)element;
            container.MouseLeftButtonDown += ContainerMouseLeftButtonDown;
            container.TouchDown += ContainerTouchDown;
            container.TouchUp += ContainerTouchUp;
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            UIElement container = (UIElement)element;
            container.MouseLeftButtonDown -= ContainerMouseLeftButtonDown;
            container.TouchDown -= ContainerTouchDown;
            container.TouchUp -= ContainerTouchUp;
        }

        private void ContainerMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            UIElement container = (UIElement)sender;
            UIElement selectedContainer;

            if (SelectionMode != SelectionMode.Extended || (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                Selector.SetIsSelected(container, !Selector.GetIsSelected(container));
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                SelectedItem = ItemFromContainer(container);
            }
            else if ((selectedContainer = ContainerFromItem(SelectedItem)) != null)
            {
                Point? p1 = MapPanel.GetViewportPosition(selectedContainer);
                Point? p2 = MapPanel.GetViewportPosition(container);

                if (p1.HasValue && p2.HasValue)
                {
                    Rect rect = new Rect(p1.Value, p2.Value);

                    BeginUpdateSelectedItems();
                    SelectedItems.Clear();
                    SelectedItems.Add(SelectedItem);

                    foreach (object item in Items)
                    {
                        if (item != SelectedItem && IsItemInRect(item, rect))
                        {
                            SelectedItems.Add(item);
                        }
                    }

                    EndUpdateSelectedItems();
                }
            }
        }

        private void ContainerTouchDown(object sender, TouchEventArgs e)
        {
            e.Handled = true; // get TouchUp event
        }

        private void ContainerTouchUp(object sender, TouchEventArgs e)
        {
            e.Handled = true;
            UIElement container = (UIElement)sender;
            Selector.SetIsSelected(container, !Selector.GetIsSelected(container));
        }

        private void OnCurrentItemChanging(object sender, CurrentChangingEventArgs e)
        {
            UIElement container = ContainerFromItem(Items.CurrentItem);

            if (container != null)
            {
                SetIsCurrent(container, false);
            }
        }

        private void OnCurrentItemChanged(object sender, EventArgs e)
        {
            UIElement container = ContainerFromItem(Items.CurrentItem);

            if (container != null)
            {
                SetIsCurrent(container, true);
            }
        }

        private void SelectionGeometryPropertyChanged(Geometry geometry)
        {
            if (geometry != null)
            {
                if (SelectionMode == SelectionMode.Single)
                {
                    SelectedItem = GetFirstItemInGeometry(geometry);
                }
                else
                {
                    BeginUpdateSelectedItems();
                    SelectedItems.Clear();
                    GetItemsInGeometry(geometry, SelectedItems);
                    EndUpdateSelectedItems();
                }
            }
        }

        private object GetFirstItemInGeometry(Geometry geometry)
        {
            if (!geometry.IsEmpty())
            {
                foreach (object item in Items)
                {
                    if (IsItemInGeometry(item, geometry))
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        private IList GetItemsInGeometry(Geometry geometry, IList items)
        {
            if (!geometry.IsEmpty())
            {
                foreach (object item in Items)
                {
                    if (IsItemInGeometry(item, geometry))
                    {
                        items.Add(item);
                    }
                }
            }

            return items;
        }

        private bool IsItemInGeometry(object item, Geometry geometry)
        {
            UIElement container = ContainerFromItem(item);
            Point? viewportPosition;

            return container != null
                && (viewportPosition = MapPanel.GetViewportPosition(container)).HasValue
                && geometry.FillContains(viewportPosition.Value);
        }

        private bool IsItemInRect(object item, Rect rect)
        {
            UIElement container = ContainerFromItem(item);
            Point? viewportPosition;

            return container != null
                && (viewportPosition = MapPanel.GetViewportPosition(container)).HasValue
                && rect.Contains(viewportPosition.Value);
        }
    }
}
