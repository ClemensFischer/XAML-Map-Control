// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Collections.Specialized;
#if NETFX_CORE
using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// A polyline or polygon created from a collection of Locations.
    /// </summary>
    public partial class MapPolyline : MapPath
    {
#if NETFX_CORE
        // Binding fails on Windows Runtime when property type is IEnumerable<Location>
        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register(
            "Locations", typeof(IEnumerable), typeof(MapPolyline),
            new PropertyMetadata(null, (o, e) => ((MapPolyline)o).LocationsChanged(e.OldValue as INotifyCollectionChanged, e.NewValue as INotifyCollectionChanged)));
#else
        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register(
            "Locations", typeof(IEnumerable<Location>), typeof(MapPolyline),
            new PropertyMetadata(null, (o, e) => ((MapPolyline)o).LocationsChanged(e.OldValue as INotifyCollectionChanged, e.NewValue as INotifyCollectionChanged)));
#endif
        public static readonly DependencyProperty IsClosedProperty = DependencyProperty.Register(
            "IsClosed", typeof(bool), typeof(MapPolyline),
            new PropertyMetadata(false, (o, e) => ((MapPolyline)o).UpdateData()));

        /// <summary>
        /// Gets or sets the locations that define the polyline points.
        /// </summary>
#if !NETFX_CORE
        [TypeConverter(typeof(LocationCollectionConverter))]
#endif
        public IEnumerable<Location> Locations
        {
            get { return (IEnumerable<Location>)GetValue(LocationsProperty); }
            set { SetValue(LocationsProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates if the polyline is closed, i.e. is a polygon.
        /// </summary>
        public bool IsClosed
        {
            get { return (bool)GetValue(IsClosedProperty); }
            set { SetValue(IsClosedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the FillRule of the PathGeometry that represents the polyline.
        /// </summary>
        public FillRule FillRule
        {
            get { return (FillRule)GetValue(FillRuleProperty); }
            set { SetValue(FillRuleProperty, value); }
        }

        private void LocationsChanged(INotifyCollectionChanged oldCollection, INotifyCollectionChanged newCollection)
        {
            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= LocationCollectionChanged;
            }

            if (newCollection != null)
            {
                newCollection.CollectionChanged += LocationCollectionChanged;
            }

            UpdateData();
        }

        private void LocationCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateData();
        }
    }
}
