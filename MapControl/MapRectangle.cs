// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
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

        private bool boundingBoxValid;

        public MapRectangle()
        {
            StrokeThickness = 0d;
            Data = new RectangleGeometry();
            boundingBoxValid = true;
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

        public void SetBoundingBox(double west, double east, double south, double north)
        {
            if (West != west || East != east || South != south || North != north)
            {
                boundingBoxValid = false;
                West = west;
                East = east;
                South = south;
                North = north;
                boundingBoxValid = true;
                UpdateData();
            }
        }

        protected override void UpdateData()
        {
            if (boundingBoxValid)
            {
                var geometry = (RectangleGeometry)Data;

                if (ParentMap != null &&
                    !double.IsNaN(South) && !double.IsNaN(North) &&
                    !double.IsNaN(West) && !double.IsNaN(East) &&
                    South < North && West < East)
                {
                    var rect = new Rect(ParentMap.MapTransform.Transform(new Location(South, West)),
                                        ParentMap.MapTransform.Transform(new Location(North, East)));
                    Transform transform = ViewportTransform;

                    ScaleRect(ref rect, ref transform);

                    geometry.Rect = rect;
                    RenderTransform = transform;
                }
                else
                {
                    geometry.ClearValue(RectangleGeometry.RectProperty);
                    ClearValue(RenderTransformProperty);
                }
            }
        }

        static partial void ScaleRect(ref Rect rect, ref Transform transform); // WPF only
    }
}
