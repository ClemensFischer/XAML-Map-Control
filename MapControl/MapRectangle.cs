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
    public class MapRectangle : MapPath
    {
        public static readonly DependencyProperty SouthProperty = DependencyProperty.Register(
            "South", typeof(double), typeof(MapRectangle),
            new PropertyMetadata(double.NaN, (o, e) => ((MapRectangle)o).UpdateData()));

        public static readonly DependencyProperty NorthProperty = DependencyProperty.Register(
            "North", typeof(double), typeof(MapRectangle),
            new PropertyMetadata(double.NaN, (o, e) => ((MapRectangle)o).UpdateData()));

        public static readonly DependencyProperty WestProperty = DependencyProperty.Register(
            "West", typeof(double), typeof(MapRectangle),
            new PropertyMetadata(double.NaN, (o, e) => ((MapRectangle)o).UpdateData()));

        public static readonly DependencyProperty EastProperty = DependencyProperty.Register(
            "East", typeof(double), typeof(MapRectangle),
            new PropertyMetadata(double.NaN, (o, e) => ((MapRectangle)o).UpdateData()));

        public MapRectangle()
        {
            Data = new RectangleGeometry();
            StrokeThickness = 0d;
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

        protected override void UpdateData()
        {
            var geometry = (RectangleGeometry)Data;

            if (ParentMap != null &&
                !double.IsNaN(South) && !double.IsNaN(North) &&
                !double.IsNaN(West) && !double.IsNaN(East) &&
                South < North && West < East)
            {
                // Create a scaled RectangleGeometry due to inaccurate hit testing in WPF.
                // See http://stackoverflow.com/a/19335624/1136211

                const double scale = 1e6;
                var p1 = ParentMap.MapTransform.Transform(new Location(South, West));
                var p2 = ParentMap.MapTransform.Transform(new Location(North, East));
                geometry.Rect = new Rect(p1.X * scale, p1.Y * scale, (p2.X - p1.X) * scale, (p2.Y - p1.Y) * scale);

                var scaleTransform = new ScaleTransform // revert scaling
                {
                    ScaleX = 1d / scale,
                    ScaleY = 1d / scale
                };
                scaleTransform.Freeze();

                var transform = new TransformGroup();
                transform.Children.Add(scaleTransform);
                transform.Children.Add(ParentMap.ViewportTransform);
                RenderTransform = transform;
            }
            else
            {
                geometry.ClearValue(RectangleGeometry.RectProperty);
                ClearValue(RenderTransformProperty);
            }
        }
    }
}
