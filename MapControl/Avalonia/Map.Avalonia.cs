// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
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
            ZoomMap(e.GetPosition(this), TargetZoomLevel + MouseWheelZoomDelta * e.Delta.Y);

            e.Handled = true;

            base.OnPointerWheelChanged(e);
        }

        private bool HandleMousePressed(PointerPoint point)
        {
            var handled = pointer1 == null && point.Properties.IsLeftButtonPressed;
            
            if (handled)
            {
                pointer1 = point.Pointer;
                position1 = point.Position;
            }

            return handled;
        }

        private bool HandleTouchPressed(PointerPoint point)
        {
            var handled = pointer2 == null && ManipulationModes != ManipulationModes.None;

            if (handled)
            {
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

            return handled;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);

            if (point.Pointer.Type == PointerType.Mouse && HandleMousePressed(point) ||
                point.Pointer.Type == PointerType.Touch && HandleTouchPressed(point))
            {
                point.Pointer.Capture(this);

                e.Handled = true;
            }

            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.Pointer == pointer1 || e.Pointer == pointer2)
            {
                e.Pointer.Capture(null);

                if (e.Pointer == pointer1)
                {
                    pointer1 = pointer2;
                    position1 = position2;
                }

                pointer2 = null;

                e.Handled = true;
            }

            base.OnPointerReleased(e);
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            if (e.Pointer == pointer1 || e.Pointer == pointer2)
            {
                if (e.Pointer == pointer1)
                {
                    pointer1 = pointer2;
                    position1 = position2;
                }

                pointer2 = null;

                e.Handled = true;
            }

            base.OnPointerCaptureLost(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (e.Pointer == pointer1 || e.Pointer == pointer2)
            {
                var position = e.GetPosition(this);

                if (pointer2 != null)
                {
                    var oldDistance = new Vector(position2.X - position1.X, position2.Y - position1.Y);
                    var oldOrigin = new Point((position1.X + position2.X) / 2d, (position1.Y + position2.Y) / 2d);

                    if (e.Pointer == pointer1)
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
                else if (e.Pointer.Type != PointerType.Touch || ManipulationModes.HasFlag(ManipulationModes.Translate))
                {
                    TranslateMap(position - position1);

                    position1 = position;
                }

                e.Handled = true;
            }

            base.OnPointerMoved(e);
        }
    }
}
