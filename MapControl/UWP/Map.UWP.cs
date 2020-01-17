// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2020 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;

namespace MapControl
{
    public partial class Map
    {
        public Map()
        {
            ManipulationMode = ManipulationModes.Scale
                | ManipulationModes.TranslateX
                | ManipulationModes.TranslateY
                | ManipulationModes.TranslateInertia;

            ManipulationDelta += OnManipulationDelta;
            PointerWheelChanged += OnPointerWheelChanged;
        }

        private async void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            translation.X += e.Delta.Translation.X;
            translation.Y += e.Delta.Translation.Y;
            rotation += e.Delta.Rotation;
            scale *= e.Delta.Scale;

            if (!transformPending)
            {
                transformPending = true;

                await Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                    () => TransformMap(e.Position, translation, rotation, scale));

                ResetTransform();
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
