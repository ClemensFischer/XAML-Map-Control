// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    public partial class MapPath : Shape
    {
        public MapPath()
        {
            Stretch = Stretch.None;
        }

        public static readonly DependencyProperty DataProperty =
            DependencyPropertyHelper.AddOwner<MapPath, Geometry>(Path.DataProperty, null,
                (path, oldValue, newValue) => path.DataPropertyChanged(oldValue, newValue));

        public Geometry Data
        {
            get => (Geometry)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Geometry DefiningGeometry => Data;

        private void DataPropertyChanged(Geometry oldData, Geometry newData)
        {
            // Check if data is actually a new Geometry.
            //
            if (newData != null && !ReferenceEquals(newData, oldData))
            {
                if (newData.IsFrozen)
                {
                    Data = newData.Clone(); // DataPropertyChanged called again
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
