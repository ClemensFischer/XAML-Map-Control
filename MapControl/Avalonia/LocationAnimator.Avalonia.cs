// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Animation;

namespace MapControl
{
    public class LocationAnimator : InterpolatingAnimator<Location>
    {
        public override Location Interpolate(double progress, Location oldValue, Location newValue)
        {
            throw new System.NotImplementedException();
        }
    }
}
