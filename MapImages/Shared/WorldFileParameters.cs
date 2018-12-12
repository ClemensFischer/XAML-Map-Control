// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Globalization;
using System.IO;
using System.Linq;
#if WINDOWS_UWP
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace MapControl.Images
{
    public class WorldFileParameters
    {
        public WorldFileParameters()
        {
        }

        public WorldFileParameters(string path)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException("World file \"" + path + "\"not found.");
            }

            var lines = File.ReadLines(path).Take(6).ToList();

            if (lines.Count != 6)
            {
                throw new ArgumentException("Invalid number of parameters in world file \"" + path + "\".");
            }

            double xscale, yskew, xskew, yscale, xorigin, yorigin;

            if (!double.TryParse(lines[0], NumberStyles.Float, CultureInfo.InvariantCulture, out xscale) ||
                !double.TryParse(lines[1], NumberStyles.Float, CultureInfo.InvariantCulture, out yskew) ||
                !double.TryParse(lines[2], NumberStyles.Float, CultureInfo.InvariantCulture, out xskew) ||
                !double.TryParse(lines[3], NumberStyles.Float, CultureInfo.InvariantCulture, out yscale) ||
                !double.TryParse(lines[4], NumberStyles.Float, CultureInfo.InvariantCulture, out xorigin) ||
                !double.TryParse(lines[5], NumberStyles.Float, CultureInfo.InvariantCulture, out yorigin))
            {
                throw new ArgumentException("Failed parsing parameters in world file \"" + path + "\".");
            }

            XScale = xscale;
            YSkew = yskew;
            XSkew = xskew;
            YScale = yscale;
            XOrigin = xorigin;
            YOrigin = yorigin;
        }

        public double XScale { get; set; } // A
        public double YSkew { get; set; } // D
        public double XSkew { get; set; } // B
        public double YScale { get; set; } // E
        public double XOrigin { get; set; } // C
        public double YOrigin { get; set; } // F

        public BoundingBox GetBoundingBox(double imageWidth, double imageHeight, MapProjection projection = null)
        {
            if (XScale == 0d || YScale == 0d)
            {
                throw new ArgumentException("Invalid WorldFileParameters, XScale and YScale must be non-zero.");
            }

            if (YSkew != 0d || XSkew != 0d)
            {
                throw new ArgumentException("Invalid WorldFileParameters, YSkew and XSkew must be zero.");
            }

            var p1 = new Point(XOrigin, YOrigin);
            var p2 = new Point(XOrigin + XScale * imageWidth, YOrigin + YScale * imageHeight);
            var rect = new Rect(p1, p2);

            if (projection != null)
            {
                return projection.RectToBoundingBox(rect);
            }

            return new BoundingBox
            {
                West = rect.X,
                East = rect.X + rect.Width,
                South = rect.Y,
                North = rect.Y + rect.Height
            };
        }
    }
}
