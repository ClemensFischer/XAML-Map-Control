using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace MapControl;

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

    public new MapItem ContainerFromItem(object item)
    {
        return (MapItem)base.ContainerFromItem(item);
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
        PrepareContainer((MapItem)container, item);
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        base.ClearContainerForItemOverride(container);
        ClearContainer((MapItem)container);
    }

    protected override bool ShouldTriggerSelection(Visual selectable, PointerEventArgs eventArgs)
    {
        return true;
    }

    public override bool UpdateSelectionFromEvent(UIElement container, RoutedEventArgs eventArgs)
    {
        if (SelectionMode == SelectionMode.Multiple &&
            eventArgs is PointerEventArgs e &&
            e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            SelectItemsInRange((MapItem)container);
            return true;
        }

        return base.UpdateSelectionFromEvent(container, eventArgs);
    }
}
