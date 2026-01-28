#if WPF
using System.Windows;
#elif AVALONIA
using Avalonia;
#endif

namespace MapControl
{
    public class MapRect(Rect rect, Location origin)
    {
        public Rect Rect { get; private set; } = rect;
        public Location Origin { get; private set; } = origin;

        public void Update(MapProjection projection)
        {
            Point? origin;

            if (Origin != null && projection.Center != null &&
                !Origin.Equals(projection.Center) &&
                (origin = projection.LocationToMap(Origin)).HasValue)
            {
                Rect = new Rect(Rect.X + origin.Value.X, Rect.Y + origin.Value.Y, Rect.Width, Rect.Height);
                Origin = projection.Center;
            }
        }
    }
}
