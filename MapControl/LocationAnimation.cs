using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace MapControl
{
    /// <summary>
    /// Animates the value of a Location property between two values.
    /// </summary>
    public class LocationAnimation : AnimationTimeline
    {
        public Location From { get; set; }
        public Location To { get; set; }
        public IEasingFunction EasingFunction { get; set; }

        public override Type TargetPropertyType
        {
            get { return typeof(Location); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LocationAnimation
            {
                From = From,
                To = To,
                EasingFunction = EasingFunction
            };
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (!animationClock.CurrentProgress.HasValue)
            {
                return defaultOriginValue;
            }

            double progress = animationClock.CurrentProgress.Value;

            if (EasingFunction != null)
            {
                progress = EasingFunction.Ease(progress);
            }

            double deltaLongitude = progress * Location.NormalizeLongitude(To.Longitude - From.Longitude);

            return new Location(
                (1d - progress) * From.Latitude + progress * To.Latitude,
                Location.NormalizeLongitude(From.Longitude + deltaLongitude));
        }
    }
}
