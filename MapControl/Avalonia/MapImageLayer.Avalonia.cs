using Avalonia.Animation;
using Avalonia.Styling;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class MapImageLayer
    {
        private void FadeOver()
        {
            var fadeInAnimation = new Animation
            {
                FillMode = FillMode.Forward,
                Duration = MapBase.ImageFadeDuration,
                Children =
                {
                    new KeyFrame
                    {
                        KeyTime = MapBase.ImageFadeDuration,
                        Setters = { new Setter(OpacityProperty, 1d) }
                    }
                }
            };

            _ = fadeInAnimation.RunAsync(Children[1]).ContinueWith(
                _ => Children[0].Opacity = 0d,
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
