// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public struct Int32Rect
    {
        public Int32Rect(int x, int y, int width, int height)
            : this()
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public override int GetHashCode()
        {
            return X ^ Y ^ Width ^ Height;
        }

        public override bool Equals(object obj)
        {
            return obj is Int32Rect && (Int32Rect)obj == this;
        }

        public static bool operator ==(Int32Rect rect1, Int32Rect rect2)
        {
            return rect1.X == rect2.X && rect1.Y == rect2.Y && rect1.Width == rect2.Width && rect1.Height == rect2.Height;
        }

        public static bool operator !=(Int32Rect rect1, Int32Rect rect2)
        {
            return !(rect1 == rect2);
        }
    }
}
