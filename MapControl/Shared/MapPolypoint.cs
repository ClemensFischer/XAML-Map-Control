using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

#if WPF
using System.Windows;
using System.Windows.Media;
using PolypointGeometry = System.Windows.Media.StreamGeometry;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using PolypointGeometry = Windows.UI.Xaml.Media.PathGeometry;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using PolypointGeometry = Microsoft.UI.Xaml.Media.PathGeometry;
#elif AVALONIA
using Avalonia.Media;
using PolypointGeometry = Avalonia.Media.PathGeometry;
#endif

namespace MapControl
{
    /// <summary>
    /// Base class of MapPolyline and MapPolygon and MapMultiPolygon.
    /// </summary>
    public partial class MapPolypoint
    {
        public static readonly DependencyProperty FillRuleProperty =
            DependencyPropertyHelper.Register<MapPolygon, FillRule>(nameof(FillRule), FillRule.EvenOdd,
                (polypoint, oldValue, newValue) => ((PolypointGeometry)polypoint.Data).FillRule = newValue);

        public FillRule FillRule
        {
            get => (FillRule)GetValue(FillRuleProperty);
            set => SetValue(FillRuleProperty, value);
        }

        protected MapPolypoint()
        {
            Data = new PolypointGeometry();
        }

        protected void DataCollectionPropertyChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= DataCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += DataCollectionChanged;
            }

            UpdateData();
        }

        protected void DataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateData();
        }

        protected double GetLongitudeOffset(IEnumerable<Location> locations)
        {
            var longitudeOffset = 0d;

            if (ParentMap.MapProjection.IsNormalCylindrical)
            {
                var location = Location ?? locations?.FirstOrDefault();

                if (location != null &&
                    !ParentMap.InsideViewBounds(ParentMap.LocationToView(location)))
                {
                    longitudeOffset = ParentMap.NearestLongitude(location.Longitude) - location.Longitude;
                }
            }

            return longitudeOffset;
        }
    }
}
