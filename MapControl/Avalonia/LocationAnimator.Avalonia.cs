namespace MapControl
{
    public class LocationAnimator : InterpolatingAnimator<Location>
    {
        public override Location Interpolate(double progress, Location oldValue, Location newValue)
        {
            return new Location(
                (1d - progress) * oldValue.Latitude + progress * newValue.Latitude,
                (1d - progress) * oldValue.Longitude + progress * newValue.Longitude);
        }
    }
}
