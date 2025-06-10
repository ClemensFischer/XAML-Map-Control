using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    public partial class MapPath : Shape
    {
        public static readonly DependencyProperty DataProperty =
            Path.DataProperty.AddOwner(typeof(MapPath),
                new FrameworkPropertyMetadata(null, (o, e) => ((MapPath)o).DataPropertyChanged(e)));

        public Geometry Data
        {
            get => (Geometry)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Geometry DefiningGeometry => Data;

        private void DataPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            // Check if Data is actually a new Geometry.
            //
            if (e.NewValue != null && !ReferenceEquals(e.NewValue, e.OldValue))
            {
                var data = (Geometry)e.NewValue;

                if (data.IsFrozen)
                {
                    Data = data.Clone(); // DataPropertyChanged called again
                }
                else
                {
                    UpdateData();
                }
            }
        }

        private void SetMapTransform(Matrix matrix)
        {
            if (Data.Transform is MatrixTransform transform && !transform.IsFrozen)
            {
                transform.Matrix = matrix;
            }
            else
            {
                Data.Transform = new MatrixTransform(matrix);
            }
        }
    }
}
