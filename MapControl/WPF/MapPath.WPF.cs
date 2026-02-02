using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    public partial class MapPath : Shape
    {
        public static readonly DependencyProperty DataProperty =
            DependencyPropertyHelper.AddOwner<MapPath, Geometry>(Path.DataProperty,
                (path, oldValue, newValue) => path.DataPropertyChanged(oldValue, newValue));

        public Geometry Data
        {
            get => (Geometry)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Geometry DefiningGeometry => Data;

        private void DataPropertyChanged(Geometry oldValue, Geometry newValue)
        {
            // Check if Data is actually a new Geometry.
            //
            if (newValue != null && !ReferenceEquals(newValue, oldValue))
            {
                if (newValue.IsFrozen)
                {
                    Data = newValue.Clone(); // DataPropertyChanged called again
                }
                else
                {
                    UpdateData();
                }
            }
        }
    }
}
