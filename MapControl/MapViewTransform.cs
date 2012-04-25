using System;
using System.Windows;
using System.Windows.Media;

namespace MapControl
{
    public class MapViewTransform : GeneralTransform
    {
        private readonly GeneralTransform inverse;

        public MapViewTransform()
        {
            MapTransform = new MercatorTransform();
            inverse = new InverseMapViewTransform(this);
        }

        public MapTransform MapTransform { get; set; }
        public Transform ViewTransform { get; set; }

        public override GeneralTransform Inverse
        {
            get { return inverse; }
        }

        public override bool TryTransform(Point point, out Point result)
        {
            result = ViewTransform.Transform(MapTransform.Transform(point));
            return true;
        }

        public override Rect TransformBounds(Rect rect)
        {
            return ViewTransform.TransformBounds(MapTransform.TransformBounds(rect));
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MapViewTransform();
        }
    }

    internal class InverseMapViewTransform : GeneralTransform
    {
        private readonly MapViewTransform inverse;

        public InverseMapViewTransform(MapViewTransform inverse)
        {
            this.inverse = inverse;
        }

        public override GeneralTransform Inverse
        {
            get { return inverse; }
        }

        public override bool TryTransform(Point point, out Point result)
        {
            result = inverse.MapTransform.Inverse.Transform(inverse.ViewTransform.Inverse.Transform(point));
            return true;
        }

        public override Rect TransformBounds(Rect rect)
        {
            return inverse.MapTransform.Inverse.TransformBounds(inverse.ViewTransform.Inverse.TransformBounds(rect));
        }

        protected override Freezable CreateInstanceCore()
        {
            return new InverseMapViewTransform(inverse);
        }
    }
}
