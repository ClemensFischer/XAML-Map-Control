// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
#if WPF
using System.Windows;
using System.Windows.Media;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// A polyline defined by a collection of Locations.
    /// </summary>
    public class MapPolyline : MapPath
    {
        public static readonly DependencyProperty LocationsProperty =
            DependencyPropertyHelper.Register<MapPolyline, IEnumerable<Location>>(nameof(Locations), null, false,
                (polyline, oldValue, newValue) => polyline.DataCollectionPropertyChanged(oldValue, newValue));

        public static readonly DependencyProperty FillRuleProperty =
            DependencyPropertyHelper.Register<MapPolyline, FillRule>(nameof(FillRule), FillRule.EvenOdd, false,
                (polyline, oldValue, newValue) => ((PathGeometry)polyline.Data).FillRule = newValue);

        /// <summary>
        /// Gets or sets the Locations that define the polyline points.
        /// </summary>
#if WPF
        [System.ComponentModel.TypeConverter(typeof(LocationCollectionConverter))]
#endif
        public IEnumerable<Location> Locations
        {
            get => (IEnumerable<Location>)GetValue(LocationsProperty);
            set => SetValue(LocationsProperty, value);
        }

        public FillRule FillRule
        {
            get => (FillRule)GetValue(FillRuleProperty);
            set => SetValue(FillRuleProperty, value);
        }

        public MapPolyline()
        {
            Data = new PathGeometry();
        }

        protected override void UpdateData()
        {
            var figures = ((PathGeometry)Data).Figures;
            figures.Clear();
            AddPolylinePoints(figures, Locations, false);
        }
    }
}
