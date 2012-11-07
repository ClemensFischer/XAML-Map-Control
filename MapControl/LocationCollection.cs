// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace MapControl
{
    /// <summary>
    /// A collection of geographic locations.
    /// </summary>
    [TypeConverter(typeof(LocationCollectionConverter))]
    public class LocationCollection : ObservableCollection<Location>
    {
        public LocationCollection()
        {
        }

        public LocationCollection(IEnumerable<Location> locations)
        {
            foreach (Location location in locations)
            {
                Add(location);
            }
        }

        public static LocationCollection Parse(string source)
        {
            LocationCollection locations = new LocationCollection();

            foreach (string locString in source.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                locations.Add(Location.Parse(locString));
            }

            return locations;
        }
    }

    /// <summary>
    /// Converts from string to LocationCollection.
    /// </summary>
    public class LocationCollectionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return LocationCollection.Parse((string)value);
        }
    }
}
