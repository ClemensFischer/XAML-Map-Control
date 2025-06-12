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

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            // Standard mouse wheel delta value is 120.
            //
            OnMouseWheel(e.GetPosition(this), e.Delta / 120d);
        }

        private Point? mousePosition;

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                // Do not call CaptureMouse here because it avoids MapItem selection.
                //
                mousePosition = e.GetPosition(this);
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (mousePosition.HasValue)
            {
                mousePosition = null;
                ReleaseMouseCapture();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mousePosition.HasValue)
            {
                if (!IsMouseCaptured)
                {
                    CaptureMouse();
                }

                var p = e.GetPosition(this);
                TranslateMap(new Point(p.X - mousePosition.Value.X, p.Y - mousePosition.Value.Y));
                mousePosition = p;
            }
        }

        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            Manipulation.SetManipulationMode(this, ManipulationMode);

            base.OnManipulationStarted(e);
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);

            TransformMap(e.ManipulationOrigin,
                (Point)e.DeltaManipulation.Translation,
                e.DeltaManipulation.Rotation,
                e.DeltaManipulation.Scale.LengthSquared / 2d);
        }
    }
}
