using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MapControl;

namespace MapControlTestApp
{
    class SampleItem
    {
        public string Name { get; set; }
    }

    class SamplePoint : SampleItem
    {
        public Location Location { get; set; }
    }

    class SamplePushpin : SamplePoint
    {
    }

    class SampleShape : SamplePoint
    {
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }
        public double Rotation { get; set; }
    }

    class SamplePolyline : SampleItem
    {
        public LocationCollection Locations { get; set; }
    }

    class SamplePolygon : SampleItem
    {
        public LocationCollection Locations { get; set; }
    }

    class SampleItemCollection : ObservableCollection<SampleItem>
    {
    }

    class SampleItemStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is SamplePolyline)
            {
                return Application.Current.Windows[0].Resources["SamplePolylineItemStyle"] as Style;
            }

            if (item is SamplePolygon)
            {
                return Application.Current.Windows[0].Resources["SamplePolygonItemStyle"] as Style;
            }

            if (item is SampleShape)
            {
                return Application.Current.Windows[0].Resources["SampleShapeItemStyle"] as Style;
            }

            if (item is SamplePushpin)
            {
                return Application.Current.Windows[0].Resources["SamplePushpinItemStyle"] as Style;
            }

            return Application.Current.Windows[0].Resources["SamplePointItemStyle"] as Style;
        }
    }
}
