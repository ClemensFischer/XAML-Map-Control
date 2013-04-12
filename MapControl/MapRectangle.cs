// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
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
    public partial class MapRectangle : MapShape
    {
        private static Transform DefaultFillRelativeTransform =
            Brush.RelativeTransformProperty.GetMetadata(typeof(Brush)).DefaultValue as Transform;

        public static readonly DependencyProperty SouthProperty = DependencyProperty.Register(
            "South", typeof(double), typeof(MapRectangle),
            new PropertyMetadata(double.NaN, (o, e) => ((MapRectangle)o).UpdateGeometry()));

        public static readonly DependencyProperty NorthProperty = DependencyProperty.Register(
            "North", typeof(double), typeof(MapRectangle),
            new PropertyMetadata(double.NaN, (o, e) => ((MapRectangle)o).UpdateGeometry()));

        public static readonly DependencyProperty WestProperty = DependencyProperty.Register(
            "West", typeof(double), typeof(MapRectangle),
            new PropertyMetadata(double.NaN, (o, e) => ((MapRectangle)o).UpdateGeometry()));

        public static readonly DependencyProperty EastProperty = DependencyProperty.Register(
            "East", typeof(double), typeof(MapRectangle),
            new PropertyMetadata(double.NaN, (o, e) => ((MapRectangle)o).UpdateGeometry()));

        public MapRectangle()
            : base(new RectangleGeometry())
        {
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

        protected override void UpdateGeometry()
        {
            var geometry = (RectangleGeometry)Geometry;

            if (ParentMap != null &&
                !double.IsNaN(South) && !double.IsNaN(North) &&
                !double.IsNaN(West) && !double.IsNaN(East) &&
                South < North && West < East)
            {
                var p1 = ParentMap.MapTransform.Transform(new Location(South, West));
                var p2 = ParentMap.MapTransform.Transform(new Location(North, East));

                geometry.Rect = new Rect(p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
                RenderTransform = ParentMap.ViewportTransform;
            }
            else
            {
                geometry.Rect = Rect.Empty;
                ClearValue(RenderTransformProperty);
            }
        }

//#if !NETFX_CORE && !SILVERLIGHT
//        protected override void OnRender(DrawingContext drawingContext)
//        {
//            if (ParentMap != null)
//            {
//                drawingContext.PushTransform(ParentMap.ViewportTransform);
//                drawingContext.DrawRectangle(Fill, null, ((RectangleGeometry)Geometry).Rect);
//                drawingContext.Pop();
//            }
//        }
//#endif
    }
}
