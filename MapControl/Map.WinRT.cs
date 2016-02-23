// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

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

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            var zoomChange = MouseWheelZoomDelta * (double)point.Properties.MouseWheelDelta / 120d;
            ZoomMap(point.Position, TargetZoomLevel + zoomChange);
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            TransformMap(e.Position, e.Delta.Translation, e.Delta.Rotation, e.Delta.Scale);
        }
    }
}
