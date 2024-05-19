// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Input;
using System.Threading;

namespace MapControl
{
    /// <summary>
    /// MapBase with default input event handling.
    /// </summary>
    public class Map : MapBase
    {
        public static readonly StyledProperty<double> MouseWheelZoomDeltaProperty
            = AvaloniaProperty.Register<Map, double>(nameof(MouseWheelZoomDelta), 0.25);

        private Point? mousePosition;
        private double targetZoomLevel;
        private CancellationTokenSource cancellationTokenSource;

        public Map()
        {
            PointerWheelChanged += OnPointerWheelChanged;
            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
            PointerMoved += OnPointerMoved;
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

        private async void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            var delta = MouseWheelZoomDelta * e.Delta.Y;

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                targetZoomLevel += delta;
            }
            else
            {
                targetZoomLevel = ZoomLevel + delta;
            }

            cancellationTokenSource = new CancellationTokenSource();

            await ZoomMap(e.GetPosition(this), targetZoomLevel, cancellationTokenSource.Token);

            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);

            if (point.Properties.IsLeftButtonPressed)
            {
                e.Pointer.Capture(this);
                mousePosition = point.Position;
            }
        }

        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (mousePosition.HasValue)
            {
                e.Pointer.Capture(null);
                mousePosition = null;
            }
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (mousePosition.HasValue)
            {
                var position = e.GetPosition(this);
                TranslateMap(position - mousePosition.Value);
                mousePosition = position;
            }
        }
    }
}
