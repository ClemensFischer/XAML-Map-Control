using MapControl;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace UniversalApp
{
    public class BindingHelper
    {
        public static readonly DependencyProperty LocationPathProperty = DependencyProperty.RegisterAttached(
            "LocationPath", typeof(string), typeof(BindingHelper),
            new PropertyMetadata(null, LocationPathPropertyChanged));

        public static string GetLocationPath(DependencyObject obj)
        {
            return (string)obj.GetValue(LocationPathProperty);
        }

        public static void SetLocationPath(DependencyObject obj, string value)
        {
            obj.SetValue(LocationPathProperty, value);
        }

        private static void LocationPathPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var propertyPath = e.NewValue as string;

            if (propertyPath != null)
            {
                BindingOperations.SetBinding(
                    obj,
                    MapPanel.LocationProperty,
                    new Binding { Path = new PropertyPath(propertyPath) });
            }
        }
    }
}
