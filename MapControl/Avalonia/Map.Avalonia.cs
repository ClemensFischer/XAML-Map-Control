// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

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

    /// <summary>
    /// MapBase with default input event handling.
    /// </summary>
    public class Map : MapBase
    {
        public static readonly StyledProperty<ManipulationModes> ManipulationModesProperty =
            DependencyPropertyHelper.Register<Map, ManipulationModes>(nameof(ManipulationModes), ManipulationModes.Translate | ManipulationModes.Scale);

        public static readonly StyledProperty<double> MouseWheelZoomDeltaProperty =
            DependencyPropertyHelper.Register<Map, double>(nameof(MouseWheelZoomDelta), 0.25);

        private double mouseWheelDelta;
        private IPointer pointer1;
        private IPointer pointer2;
        private Point position1;
        private Point position2;

        public ManipulationModes ManipulationModes
        {
            get => GetValue(ManipulationModesProperty);
            set => SetValue(ManipulationModesProperty, value);
        }

        /// <summary>
        /// Gets or sets the amount by which the ZoomLevel property changes by a MouseWheel event.
        /// The default value is 0.25.
        /// </summary>
        public double MouseWheelZoomDelta
        {
            get => GetValue(MouseWheelZoomDeltaProperty);
            set => SetValue(MouseWheelZoomDeltaProperty, value);
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            mouseWheelDelta += e.Delta.Y;

            if (Math.Abs(mouseWheelDelta) >= 1d)
            {
                // Zoom to integer multiple of MouseWheelZoomDelta.
                //
                ZoomMap(e.GetPosition(this),
                    MouseWheelZoomDelta * Math.Round(TargetZoomLevel / MouseWheelZoomDelta + mouseWheelDelta));

                mouseWheelDelta = 0d;
            }

            base.OnPointerWheelChanged(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);

            if (pointer1 == null && point.Pointer.Type == PointerType.Mouse && point.Properties.IsLeftButtonPressed ||
                pointer2 == null && point.Pointer.Type == PointerType.Touch && ManipulationModes != ManipulationModes.None)
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

            base.OnPointerPressed(e);
        }

        private void HandlePointerReleased(IPointer pointer, bool releaseCapture)
        {
            if (pointer == pointer1 || pointer == pointer2)
            {
                if (pointer == pointer1)
                {
                    pointer1 = pointer2;
                    position1 = position2;
                }

                pointer2 = null;

                if (releaseCapture)
                {
                    pointer.Capture(null);
                }
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            HandlePointerReleased(e.Pointer, true);

            base.OnPointerReleased(e);
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            HandlePointerReleased(e.Pointer, false);

            base.OnPointerCaptureLost(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (e.Pointer == pointer1 || e.Pointer == pointer2)
            {
                var position = e.GetPosition(this);

                if (pointer2 != null)
                {
                    HandleManipulation(e.Pointer, position);
                }
                else if (e.Pointer.Type == PointerType.Mouse || ManipulationModes.HasFlag(ManipulationModes.Translate))
                {
                    TranslateMap(new Point(position.X - position1.X, position.Y - position1.Y));
                    position1 = position;
                }
            }

            base.OnPointerMoved(e);
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

            if (ManipulationModes.HasFlag(ManipulationModes.Translate))
            {
                newOrigin = new Point((position1.X + position2.X) / 2d, (position1.Y + position2.Y) / 2d);
                translation = newOrigin - oldOrigin;
            }

            if (ManipulationModes.HasFlag(ManipulationModes.Rotate))
            {
                rotation = 180d / Math.PI
                    * (Math.Atan2(newDistance.Y, newDistance.X) - Math.Atan2(oldDistance.Y, oldDistance.X));
            }

            if (ManipulationModes.HasFlag(ManipulationModes.Scale))
            {
                scale = newDistance.Length / oldDistance.Length;
            }

            TransformMap(newOrigin, translation, rotation, scale);
        }
    }
}
