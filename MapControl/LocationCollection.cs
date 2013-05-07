// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MapControl
{
    /// <summary>
    /// A collection of geographic locations.
    /// </summary>
    public partial class LocationCollection : ObservableCollection<Location>
    {
        public LocationCollection()
        {
        }

        public LocationCollection(IEnumerable<Location> locations)
        {
            foreach (var location in locations)
            {
                Add(location);
            }
        }

        public static LocationCollection Parse(string s)
        {
            LocationCollection locations = new LocationCollection();

            foreach (var locString in s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                locations.Add(Location.Parse(locString));
            }

            return locations;
        }
    }
}
