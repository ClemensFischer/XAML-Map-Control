// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
#if WPF
using System.Windows;
#elif UWP
using Windows.UI.Xaml;
#elif WINUI
using Microsoft.UI.Xaml;
#elif AVALONIA
using DependencyProperty = Avalonia.AvaloniaProperty;
#endif

namespace MapControl
{
    /// <summary>
    /// A polygon defined by a collection of Locations.
    /// </summary>
    public class MapPolygon : MapPolypoint
    {
        public static readonly DependencyProperty LocationsProperty =
            DependencyPropertyHelper.Register<MapPolygon, IEnumerable<Location>>(nameof(Locations), null,
                (polygon, oldValue, newValue) => polygon.DataCollectionPropertyChanged(oldValue, newValue));

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

        protected override void UpdateData()
        {
            UpdateData(Locations, true);
        }
    }
}
