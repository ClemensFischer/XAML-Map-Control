using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace MapControl
{
    /// <summary>
    /// An ObservableCollection of IEnumerable of Location. PolygonCollection adds a CollectionChanged
    /// listener to each element that implements INotifyCollectionChanged and, when such an element changes,
    /// fires its own CollectionChanged event with NotifyCollectionChangedAction.Replace for that element.
    /// </summary>
    public class PolygonCollection : ObservableCollection<IEnumerable<Location>>
    {
        private void PolygonChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender));
        }

        protected override void InsertItem(int index, IEnumerable<Location> polygon)
        {
            if (polygon is INotifyCollectionChanged addedPolygon)
            {
                addedPolygon.CollectionChanged += PolygonChanged;
            }

            base.InsertItem(index, polygon);
        }

        protected override void SetItem(int index, IEnumerable<Location> polygon)
        {
            if (this[index] is INotifyCollectionChanged removedPolygon)
            {
                removedPolygon.CollectionChanged -= PolygonChanged;
            }

            if (polygon is INotifyCollectionChanged addedPolygon)
            {
                addedPolygon.CollectionChanged += PolygonChanged;
            }

            base.SetItem(index, polygon);
        }

        protected override void RemoveItem(int index)
        {
            if (this[index] is INotifyCollectionChanged removedPolygon)
            {
                removedPolygon.CollectionChanged -= PolygonChanged;
            }

            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            foreach (var polygon in this.OfType<INotifyCollectionChanged>())
            {
                polygon.CollectionChanged -= PolygonChanged;
            }

            base.ClearItems();
        }
    }
}
