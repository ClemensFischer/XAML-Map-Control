// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;

namespace MapControl
{
    public partial class MapItemsControl
    {
        static MapItemsControl()
        {
            TemplateProperty.OverrideDefaultValue<MapItemsControl>(
                new FuncControlTemplate<MapItemsControl>(
                    (itemsControl, namescope) => new ItemsPresenter { ItemsPanel = itemsControl.ItemsPanel }));

            ItemsPanelProperty.OverrideDefaultValue<MapItemsControl>(
                new FuncTemplate<Panel>(() => new MapPanel()));
        }

        public void SelectItemsInGeometry(Geometry geometry)
        {
            SelectItemsByPosition(p => geometry.FillContains(p));
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            recycleKey = null;

            return item is not MapItem;
        }

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new MapItem();
        }

        protected override void PrepareContainerForItemOverride(Control container, object item, int index)
        {
            base.PrepareContainerForItemOverride(container, item, index);

            if (LocationMemberPath != null && container is MapItem mapItem)
            {
                mapItem.SetBinding(MapItem.LocationProperty,
                    new Binding
                    {
                        Path = new PropertyPath(LocationMemberPath),
                        Source = item
                    });
            }
        }

        protected override void ClearContainerForItemOverride(Control container)
        {
            base.ClearContainerForItemOverride(container);

            if (LocationMemberPath != null && container is MapItem mapItem)
            {
                mapItem.ClearValue(MapItem.LocationProperty);
            }
        }

        protected void ResetSelectedItems(object item)
        {
            SelectedItem = item;
        }
    }
}
