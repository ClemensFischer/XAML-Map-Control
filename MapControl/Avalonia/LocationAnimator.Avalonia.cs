// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

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
