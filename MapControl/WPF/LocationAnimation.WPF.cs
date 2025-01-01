// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace MapControl
{
    public class LocationAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(Location);

        public Location To { get; set; }

        public IEasingFunction EasingFunction { get; set; }

        protected override Freezable CreateInstanceCore()
        {
            return new LocationAnimation
            {
                To = To,
                EasingFunction = EasingFunction
            };
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            var from = (Location)defaultOriginValue;
            var progress = animationClock.CurrentProgress ?? 1d;

            if (EasingFunction != null)
            {
                progress = EasingFunction.Ease(progress);
            }

            return new Location(
                (1d - progress) * from.Latitude + progress * To.Latitude,
                (1d - progress) * from.Longitude + progress * To.Longitude);
        }
    }
}
