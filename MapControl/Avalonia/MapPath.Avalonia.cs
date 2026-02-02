using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace MapControl
{
    public partial class MapPath : Shape
    {
        public static readonly StyledProperty<Geometry> DataProperty =
            DependencyPropertyHelper.AddOwner<MapPath, Geometry>(Path.DataProperty,
                (path, oldValue, newValue) => path.UpdateData());

        public Geometry Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Geometry CreateDefiningGeometry() => Data;
    }
}
