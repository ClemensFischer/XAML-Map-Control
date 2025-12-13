using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace MapControl
{
    public partial class MapPath : Shape
    {
        public static readonly StyledProperty<Geometry> DataProperty = Path.DataProperty.AddOwner<MapPath>();

        static MapPath()
        {
            DataProperty.Changed.AddClassHandler<MapPath, Geometry>((path, e) => path.UpdateData());
        }

        public Geometry Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Geometry CreateDefiningGeometry() => Data;

        private void SetDataTransform(Matrix matrix)
        {
            if (Data.Transform is MatrixTransform transform)
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
