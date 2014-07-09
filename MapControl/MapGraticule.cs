// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINDOWS_RUNTIME
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MapControl
{
    /// <summary>
    /// Draws a graticule overlay.
    /// </summary>
    public partial class MapGraticule : MapOverlay
    {
        /// <summary>
        /// Graticule line spacings in degrees.
        /// </summary>
        public static double[] LineSpacings =
            new double[] { 1d / 60d, 1d / 30d, 1d / 12d, 1d / 6d, 1d / 4d, 1d / 3d, 1d / 2d, 1d, 2d, 5d, 10d, 15d, 20d, 30d, 45d };

        public static readonly DependencyProperty MinLineSpacingProperty = DependencyProperty.Register(
            "MinLineSpacing", typeof(double), typeof(MapGraticule), new PropertyMetadata(150d));

        /// <summary>
        /// Minimum spacing in pixels between adjacent graticule lines.
        /// </summary>
        public double MinLineSpacing
        {
            get { return (double)GetValue(MinLineSpacingProperty); }
            set { SetValue(MinLineSpacingProperty, value); }
        }

        private static string CoordinateString(double value, string format, string hemispheres)
        {
            var hemisphere = hemispheres[0];

            if (value < -1e-8) // ~1mm
            {
                value = -value;
                hemisphere = hemispheres[1];
            }

            var minutes = (int)Math.Round(value * 60d);

            return string.Format(format, hemisphere, minutes / 60, (double)(minutes % 60));
        }
    }
}
