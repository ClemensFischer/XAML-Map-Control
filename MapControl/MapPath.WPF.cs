// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapControl
{
    public partial class MapPath : Shape
    {
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            "Data", typeof(Geometry), typeof(MapPath), new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.AffectsRender, DataPropertyChanged, CoerceDataProperty));

        static MapPath()
        {
            StretchProperty.OverrideMetadata(typeof(MapPath),
                new FrameworkPropertyMetadata { CoerceValueCallback = (o, v) => Stretch.None });
        }

        public Geometry Data
        {
            get { return (Geometry)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        protected override Geometry DefiningGeometry
        {
            get { return Data; }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // Shape.MeasureOverride sometimes returns an empty Size.
            return new Size(1, 1);
        }

        private static void DataPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(e.OldValue, e.NewValue))
            {
                ((MapPath)obj).UpdateData();
            }
        }

        private static object CoerceDataProperty(DependencyObject obj, object value)
        {
            var data = (Geometry)value;
            return (data != null && data.IsFrozen) ? data.CloneCurrentValue() : data;
        }
    }
}
