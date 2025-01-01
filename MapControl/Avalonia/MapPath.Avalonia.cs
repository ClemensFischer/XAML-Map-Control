// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Controls.Shapes;

namespace MapControl
{
    public partial class MapPath : Shape
    {
        public MapPath()
        {
            Stretch = Stretch.None;
        }

        public static readonly StyledProperty<Geometry> DataProperty =
            DependencyPropertyHelper.AddOwner<MapPath, Geometry>(Path.DataProperty, null,
                (path, oldValue, newValue) => path.UpdateData());

        public Geometry Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Geometry CreateDefiningGeometry() => Data;

        private void SetMapTransform(Matrix matrix)
        {
            if (Data.Transform is MatrixTransform transform)
            {
                transform.Matrix = matrix;
            }
            else
            {
                Data.Transform = new MatrixTransform(matrix);
            }

            InvalidateVisual();
        }
    }
}
