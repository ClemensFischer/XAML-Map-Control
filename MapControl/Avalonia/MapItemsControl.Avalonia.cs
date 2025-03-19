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
            SelectItemsByPosition(geometry.FillContains);
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

        internal void UpdateSelection(MapItem mapItem, bool controlKeyPressed, bool shiftKeyPressed)
        {
            if (SelectionMode != SelectionMode.Single && shiftKeyPressed)
            {
                SelectItemsInRange(mapItem);
            }
            else
            {
                UpdateSelection(mapItem, true, false, controlKeyPressed);
            }

            //var item = ItemFromContainer(mapItem);

            //if (SelectionMode == SelectionMode.Single)
            //{
            //    if (SelectedItem != item)
            //    {
            //        SelectedItem = item;
            //    }
            //    else if (controlKeyPressed)
            //    {
            //        SelectedItem = null;
            //    }
            //}
            //else if (controlKeyPressed)
            //{
            //    if (SelectedItems.Contains(item))
            //    {
            //        SelectedItems.Remove(item);
            //    }
            //    else
            //    {
            //        SelectedItems.Add(item);
            //    }
            //}
            //else if (shiftKeyPressed)
            //{
            //    SelectItemsInRange(mapItem);
            //}
            //else if (SelectedItem != item || SelectedItems.Count != 1)
            //{
            //    SelectedItem = item;
            //}
        }
    }
}
