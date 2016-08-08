// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using Windows.Foundation;
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
            "MouseWheelZoomDelta", typeof(double), typeof(Map), new PropertyMetadata(1d));

        private bool transformPending;
        private Point transformTranslation;
        private double transformRotation;
        private double transformScale = 1d;

        public Map()
        {
            ManipulationMode = ManipulationModes.Scale |
                ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.TranslateInertia;

            ManipulationDelta += OnManipulationDelta;
            PointerWheelChanged += OnPointerWheelChanged;
        }

        /// <summary>
        /// Gets or sets the amount by which the ZoomLevel property changes during a MouseWheel event.
        /// </summary>
        public double MouseWheelZoomDelta
        {
            get { return (double)GetValue(MouseWheelZoomDeltaProperty); }
            set { SetValue(MouseWheelZoomDeltaProperty, value); }
        }

        protected virtual void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            var zoomChange = MouseWheelZoomDelta * point.Properties.MouseWheelDelta / 120d;

            ZoomMap(point.Position, TargetZoomLevel + zoomChange);
        }

        protected virtual async void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            transformTranslation.X += e.Delta.Translation.X;
            transformTranslation.Y += e.Delta.Translation.Y;
            transformRotation += e.Delta.Rotation;
            transformScale *= e.Delta.Scale;

            if (!transformPending)
            {
                transformPending = true;

                await Dispatcher.RunIdleAsync(a =>
                {
                    TransformMap(e.Position, transformTranslation, transformRotation, transformScale);

                    transformPending = false;
                    transformTranslation.X = 0d;
                    transformTranslation.Y = 0d;
                    transformRotation = 0d;
                    transformScale = 1d;
                });
            }
        }
    }
}
