// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
#if WINDOWS_UWP
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
    /// A polygon defined by a collection of Locations.
    /// </summary>
    public class MapPolygon : MapShape
    {
        public static readonly DependencyProperty LocationsProperty = DependencyProperty.Register(
            nameof(Locations), typeof(IEnumerable<Location>), typeof(MapPolygon),
            new PropertyMetadata(null, (o, e) => ((MapPolygon)o).DataCollectionPropertyChanged(e)));

        /// <summary>
        /// Gets or sets the Locations that define the polygon points.
        /// </summary>
#if !WINDOWS_UWP
        [TypeConverter(typeof(LocationCollectionConverter))]
#endif
        public IEnumerable<Location> Locations
        {
            get { return (IEnumerable<Location>)GetValue(LocationsProperty); }
            set { SetValue(LocationsProperty, value); }
        }

        protected override void UpdateData()
        {
            var figures = ((PathGeometry)Data).Figures;
            figures.Clear();

            if (ParentMap != null)
            {
                AddPolylineLocations(figures, Locations, true);
            }
        }
    }
}
