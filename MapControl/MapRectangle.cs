// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_RUNTIME
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Fills a rectangular area defined by South, North, West and East with a Brush.
    /// </summary>
    public partial class MapRectangle : MapPath
    {
        /// <summary>
        /// Used in derived classes like MapImage.
        /// </summary>
        protected static readonly MatrixTransform FillTransform = new MatrixTransform
        {
            Matrix = new Matrix(1d, 0d, 0d, -1d, 0d, 1d)
        };

        public static readonly DependencyProperty WestProperty = DependencyProperty.Register(
            "West", typeof(double), typeof(MapRectangle),
            new PropertyMetadata(double.NaN, (o, e) => ((MapRectangle)o).UpdateData()));

        public static readonly DependencyProperty EastProperty = DependencyProperty.Register(
            "East", typeof(double), typeof(MapRectangle),
            new PropertyMetadata(double.NaN, (o, e) => ((MapRectangle)o).UpdateData()));

        public static readonly DependencyProperty SouthProperty = DependencyProperty.Register(
            "South", typeof(double), typeof(MapRectangle),
            new PropertyMetadata(double.NaN, (o, e) => ((MapRectangle)o).UpdateData()));

        public static readonly DependencyProperty NorthProperty = DependencyProperty.Register(
            "North", typeof(double), typeof(MapRectangle),
            new PropertyMetadata(double.NaN, (o, e) => ((MapRectangle)o).UpdateData()));

        public MapRectangle()
        {
            Data = new RectangleGeometry();
            StrokeThickness = 0d;
        }

        public double West
        {
            get { return (double)GetValue(WestProperty); }
            set { SetValue(WestProperty, value); }
        }

        public double East
        {
            get { return (double)GetValue(EastProperty); }
            set { SetValue(EastProperty, value); }
        }

        public double South
        {
            get { return (double)GetValue(SouthProperty); }
            set { SetValue(SouthProperty, value); }
        }

        public double North
        {
            get { return (double)GetValue(NorthProperty); }
            set { SetValue(NorthProperty, value); }
        }

        protected override void UpdateData()
        {
            var geometry = (RectangleGeometry)Data;

            if (ParentMap != null &&
                !double.IsNaN(South) && !double.IsNaN(North) &&
                !double.IsNaN(West) && !double.IsNaN(East) &&
                South < North && West < East)
            {
                SetRect(new Rect(
                    ParentMap.MapTransform.Transform(new Location(South, West)),
                    ParentMap.MapTransform.Transform(new Location(North, East))));
            }
            else
            {
                geometry.ClearValue(RectangleGeometry.RectProperty);
                ClearValue(RenderTransformProperty);
            }
        }
    }
}
