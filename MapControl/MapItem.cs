// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINRT
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
#endif

namespace MapControl
{
    /// <summary>
    /// Container class for an item in a MapItemsControl.
    /// </summary>
    public partial class MapItem : ListBoxItem
    {
        public static readonly DependencyProperty LocationPathProperty = DependencyProperty.Register(
            "LocationPath", typeof(string), typeof(MapItem), new PropertyMetadata(null, LocationPathPropertyChanged));

        /// <summary>
        /// Gets or sets the property path that is used to bind the MapPanel.Location attached property.
        /// </summary>
        public string LocationPath
        {
            get { return (string)GetValue(LocationPathProperty); }
            set { SetValue(LocationPathProperty, value); }
        }

        private static void LocationPathPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            string path = e.NewValue as string;

            if (!string.IsNullOrWhiteSpace(path))
            {
                var binding = new Binding
                {
                    Path = new PropertyPath(path)
                };

                BindingOperations.SetBinding(obj, MapPanel.LocationProperty, binding);
            }
        }
    }
}
