using System.Windows;
using System.Windows.Input;

namespace MapControl
{
    public partial class Map
    {
        static Map()
        {
            IsManipulationEnabledProperty.OverrideMetadata(typeof(Map), new FrameworkPropertyMetadata(true));
        }

        public static readonly DependencyProperty ManipulationModeProperty =
            DependencyPropertyHelper.Register<Map, ManipulationModes>(nameof(ManipulationMode), ManipulationModes.Translate | ManipulationModes.Scale);

        /// <summary>
        /// Gets or sets a value that specifies how the map control handles manipulations.
        /// </summary>
        public ManipulationModes ManipulationMode
        {
            get => (ManipulationModes)GetValue(ManipulationModeProperty);
            set => SetValue(ManipulationModeProperty, value);
        }

        private Point? mousePosition;

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            // Standard mouse wheel delta value is 120.
            //
            OnMouseWheel(e.GetPosition(this), e.Delta / 120d);

            base.OnMouseWheel(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None &&
                CaptureMouse())
            {
                mousePosition = e.GetPosition(this);
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (mousePosition.HasValue)
            {
                mousePosition = null;
                ReleaseMouseCapture();
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mousePosition.HasValue)
            {
                var p = e.GetPosition(this);
                TranslateMap(new Point(p.X - mousePosition.Value.X, p.Y - mousePosition.Value.Y));
                mousePosition = p;
            }
            else if (e.LeftButton == MouseButtonState.Pressed &&
                Keyboard.Modifiers == ModifierKeys.None &&
                CaptureMouse())
            {
                // Set mousePosition when no MouseLeftButtonDown event was received.
                //
                mousePosition = e.GetPosition(this);
            }

            base.OnMouseMove(e);
        }

        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            Manipulation.SetManipulationMode(this, ManipulationMode);

            base.OnManipulationStarted(e);
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            TransformMap(e.ManipulationOrigin,
                (Point)e.DeltaManipulation.Translation,
                e.DeltaManipulation.Rotation,
                e.DeltaManipulation.Scale.LengthSquared / 2d);

            base.OnManipulationDelta(e);
        }
    }
}
