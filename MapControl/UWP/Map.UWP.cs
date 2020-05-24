// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace MapControl
{
    /// <summary>
    /// MapBase with default input event handling.
    /// </summary>
    public class Map : MapBase
    {
        public static readonly DependencyProperty MouseWheelZoomDeltaProperty = DependencyProperty.Register(
            nameof(MouseWheelZoomDelta), typeof(double), typeof(Map), new PropertyMetadata(1d));

        private Vector transformTranslation;
        private double transformRotation;
        private double transformScale = 1d;
        private bool transformPending;

        public Map()
        {
            ManipulationMode = ManipulationModes.Scale
                | ManipulationModes.TranslateX
                | ManipulationModes.TranslateY
                | ManipulationModes.TranslateInertia;

            ManipulationDelta += OnManipulationDelta;
            PointerWheelChanged += OnPointerWheelChanged;
        }

        /// <summary>
        /// Gets or sets the amount by which the ZoomLevel property changes during a MouseWheel event.
        /// The default value is 1.
        /// </summary>
        public double MouseWheelZoomDelta
        {
            get { return (double)GetValue(MouseWheelZoomDeltaProperty); }
            set { SetValue(MouseWheelZoomDeltaProperty, value); }
        }

        private async void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            transformTranslation.X += e.Delta.Translation.X;
            transformTranslation.Y += e.Delta.Translation.Y;
            transformRotation += e.Delta.Rotation;
            transformScale *= e.Delta.Scale;

            if (!transformPending)
            {
                transformPending = true;

                await Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                    () => TransformMap(e.Position, transformTranslation, transformRotation, transformScale));

                transformTranslation.X = 0d;
                transformTranslation.Y = 0d;
                transformRotation = 0d;
                transformScale = 1d;
                transformPending = false;
            }
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            var zoomDelta = MouseWheelZoomDelta * point.Properties.MouseWheelDelta / 120d;

            ZoomMap(point.Position, TargetZoomLevel + zoomDelta);
        }
    }
}
