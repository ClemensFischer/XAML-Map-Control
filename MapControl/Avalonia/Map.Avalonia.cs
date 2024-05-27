// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;

namespace MapControl
{
    /// <summary>
    /// MapBase with default input event handling.
    /// </summary>
    public class Map : MapBase
    {
        public static readonly StyledProperty<double> MouseWheelZoomDeltaProperty =
            DependencyPropertyHelper.Register<Map, double>(nameof(MouseWheelZoomDelta), 0.25);

        private IPointer pointer1;
        private IPointer pointer2;
        private Point position1;
        private Point position2;

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

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);

            if (pointer2 == null &&
                (point.Properties.IsLeftButtonPressed || point.Pointer.Type == PointerType.Touch))
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

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (e.Pointer == pointer1 || e.Pointer == pointer2)
            {
                var position = e.GetPosition(this);

                if (pointer2 == null)
                {
                    TranslateMap(position - position1);
                    position1 = position;
                }
                else
                {
                    Point oldOrigin = new((position1.X + position2.X) / 2d, (position1.Y + position2.Y) / 2d);
                    Vector oldDistance = position2 - position1;

                    if (e.Pointer == pointer1)
                    {
                        position1 = position;
                    }
                    else
                    {
                        position2 = position;
                    }

                    Point newOrigin = new((position1.X + position2.X) / 2d, (position1.Y + position2.Y) / 2d);
                    Vector newDistance = position2 - position1;

                    var oldAngle = Math.Atan2(oldDistance.Y, oldDistance.X) * 180d / Math.PI;
                    var newAngle = Math.Atan2(newDistance.Y, newDistance.X) * 180d / Math.PI;
                    var scale = newDistance.Length / oldDistance.Length;

                    TransformMap(newOrigin, newOrigin - oldOrigin, newAngle - oldAngle, scale);
                }

                e.Handled = true;
            }

            base.OnPointerMoved(e);
        }
    }
}
