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
#elif AVALONIA
using Avalonia.Media;
using DependencyProperty = Avalonia.AvaloniaProperty;
#endif

namespace MapControl
{
    /// <summary>
    /// A polygon defined by a collection of Locations.
    /// </summary>
    public class MapPolygon : MapPath
    {
        public static readonly DependencyProperty LocationsProperty =
            DependencyPropertyHelper.Register<MapPolygon, IEnumerable<Location>>(nameof(Locations), null,
                (polygon, oldValue, newValue) => polygon.DataCollectionPropertyChanged(oldValue, newValue));

        public static readonly DependencyProperty FillRuleProperty =
            DependencyPropertyHelper.Register<MapPolygon, FillRule>(nameof(FillRule), FillRule.EvenOdd,
                (polygon, oldValue, newValue) => ((PathGeometry)polygon.Data).FillRule = newValue);

        /// <summary>
        /// Gets or sets the Locations that define the polygon points.
        /// </summary>
#if WPF || AVALONIA
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

        public MapPolygon()
        {
            Data = new PathGeometry();
        }

        protected override void UpdateData()
        {
            ((PathGeometry)Data).Figures = GetPathFigures(Locations, true);
#if AVALONIA
            InvalidateGeometry();
#endif
        }
    }
}
