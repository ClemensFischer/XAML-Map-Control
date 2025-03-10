﻿using System;
using System.Windows;
using System.Windows.Input;

namespace MapControl
{
    /// <summary>
    /// MapBase with default input event handling.
    /// </summary>
    public class Map : MapBase
    {
        public static readonly DependencyProperty MouseWheelZoomDeltaProperty =
            DependencyPropertyHelper.Register<Map, double>(nameof(MouseWheelZoomDelta), 0.25);

        public static readonly DependencyProperty ManipulationModeProperty =
            DependencyPropertyHelper.Register<Map, ManipulationModes>(nameof(ManipulationMode), ManipulationModes.Translate | ManipulationModes.Scale);

        private Point? mousePosition;
        private double mouseWheelDelta;

        static Map()
        {
            IsManipulationEnabledProperty.OverrideMetadata(typeof(Map), new FrameworkPropertyMetadata(true));
        }

        public Map()
        {
            ManipulationStarted += OnManipulationStarted;
            ManipulationDelta += OnManipulationDelta;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
            MouseWheel += OnMouseWheel;
        }

        /// <summary>
        /// Gets or sets the amount by which the ZoomLevel property changes by a MouseWheel event.
        /// The default value is 0.25.
        /// </summary>
        public double MouseWheelZoomDelta
        {
            get => (double)GetValue(MouseWheelZoomDeltaProperty);
            set => SetValue(MouseWheelZoomDeltaProperty, value);
        }

        /// <summary>
        /// Gets or sets a value that specifies how the map control handles manipulations.
        /// </summary>
        public ManipulationModes ManipulationMode
        {
            get => (ManipulationModes)GetValue(ManipulationModeProperty);
            set => SetValue(ManipulationModeProperty, value);
        }

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            Manipulation.SetManipulationMode(this, ManipulationMode);
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            TransformMap(e.ManipulationOrigin,
                (Point)e.DeltaManipulation.Translation,
                e.DeltaManipulation.Rotation,
                e.DeltaManipulation.Scale.LengthSquared / 2d);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None && CaptureMouse())
            {
                mousePosition = e.GetPosition(this);
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mousePosition.HasValue)
            {
                mousePosition = null;
                ReleaseMouseCapture();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (mousePosition.HasValue)
            {
                var p = e.GetPosition(this);
                TranslateMap(new Point(p.X - mousePosition.Value.X, p.Y - mousePosition.Value.Y));
                mousePosition = p;
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Standard mouse wheel delta value is 120.
            //
            mouseWheelDelta += e.Delta / 120d;

            if (Math.Abs(mouseWheelDelta) >= 1d)
            {
                // Zoom to integer multiple of MouseWheelZoomDelta.
                //
                ZoomMap(e.GetPosition(this),
                    MouseWheelZoomDelta * Math.Round(TargetZoomLevel / MouseWheelZoomDelta + mouseWheelDelta));

                mouseWheelDelta = 0d;
            }
        }
    }
}
