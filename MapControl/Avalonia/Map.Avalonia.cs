using Avalonia;
using Avalonia.Input;
using System;

namespace MapControl
{
    [Flags]
    public enum ManipulationModes
    {
        None = 0,
        Translate = 1,
        Rotate = 2,
        Scale = 4,
        All = Translate | Rotate | Scale
    }

    public partial class Map
    {
        public static readonly StyledProperty<ManipulationModes> ManipulationModeProperty =
            DependencyPropertyHelper.Register<Map, ManipulationModes>(nameof(ManipulationMode), ManipulationModes.Translate | ManipulationModes.Scale);

        /// <summary>
        /// Gets or sets a value that specifies how the map control handles manipulations.
        /// </summary>
        public ManipulationModes ManipulationMode
        {
            get => GetValue(ManipulationModeProperty);
            set => SetValue(ManipulationModeProperty, value);
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);

            OnMouseWheel(e.GetPosition(this), e.Delta.Y);
        }

        private IPointer pointer1;
        private IPointer pointer2;
        private Point position1;
        private Point position2;

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            var point = e.GetCurrentPoint(this);

            if (point.Pointer == pointer1 || point.Pointer == pointer2)
            {
                if (pointer2 != null)
                {
                    HandleManipulation(point.Pointer, point.Position);
                }
                else if (point.Pointer.Type == PointerType.Mouse ||
                    ManipulationMode.HasFlag(ManipulationModes.Translate))
                {
                    TranslateMap(new Point(point.Position.X - position1.X, point.Position.Y - position1.Y));
                    position1 = point.Position;
                }
            }
            else if (pointer1 == null &&
                point.Pointer.Type == PointerType.Mouse &&
                point.Properties.IsLeftButtonPressed &&
                e.KeyModifiers == KeyModifiers.None ||
                pointer2 == null &&
                point.Pointer.Type == PointerType.Touch &&
                ManipulationMode != ManipulationModes.None)
            {
                point.Pointer.Capture(this);

                if (pointer1 == null)
                {
                    pointer1 = point.Pointer;
                    position1 = point.Position;
                }
                else
                {
                    pointer2 = point.Pointer;
                    position2 = point.Position;
                }
            }
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);

            if (e.Pointer == pointer1 || e.Pointer == pointer2)
            {
                if (e.Pointer == pointer1)
                {
                    pointer1 = pointer2;
                    position1 = position2;
                }

                pointer2 = null;
            }
        }

        private void HandleManipulation(IPointer pointer, Point position)
        {
            var oldDistance = new Vector(position2.X - position1.X, position2.Y - position1.Y);
            var oldOrigin = new Point((position1.X + position2.X) / 2d, (position1.Y + position2.Y) / 2d);

            if (pointer == pointer1)
            {
                position1 = position;
            }
            else
            {
                position2 = position;
            }

            var newDistance = new Vector(position2.X - position1.X, position2.Y - position1.Y);
            var newOrigin = oldOrigin;
            var translation = new Point();
            var rotation = 0d;
            var scale = 1d;

            if (ManipulationMode.HasFlag(ManipulationModes.Translate))
            {
                newOrigin = new Point((position1.X + position2.X) / 2d, (position1.Y + position2.Y) / 2d);
                translation = newOrigin - oldOrigin;
            }

            if (ManipulationMode.HasFlag(ManipulationModes.Rotate))
            {
                rotation = 180d / Math.PI
                    * (Math.Atan2(newDistance.Y, newDistance.X) - Math.Atan2(oldDistance.Y, oldDistance.X));
            }

            if (ManipulationMode.HasFlag(ManipulationModes.Scale))
            {
                scale = newDistance.Length / oldDistance.Length;
            }

            TransformMap(newOrigin, translation, rotation, scale);
        }
    }
}
