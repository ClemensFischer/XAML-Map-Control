using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace MapControl
{
    public partial class MapImageLayer
    {
        private void FadeOver()
        {
            var fadeInAnimation = new DoubleAnimation
            {
                To = 1d,
                Duration = MapBase.ImageFadeDuration
            };

            var fadeOutAnimation = new DoubleAnimation
            {
                To = 0d,
                BeginTime = MapBase.ImageFadeDuration,
                Duration = TimeSpan.Zero
            };

            Storyboard.SetTarget(fadeInAnimation, Children[1]);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(OpacityProperty));

            Storyboard.SetTarget(fadeOutAnimation, Children[0]);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(OpacityProperty));

            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeInAnimation);
            storyboard.Children.Add(fadeOutAnimation);
            storyboard.Begin();
        }
    }
}
