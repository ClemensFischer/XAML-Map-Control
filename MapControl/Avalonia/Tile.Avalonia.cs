using Avalonia;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace MapControl
{
    public partial class Tile
    {
        public async Task LoadImageAsync(Func<Task<IImage>> loadImageFunc)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(
                () =>
                {
                    Image.Source = image;

                    if (image != null && MapBase.ImageFadeDuration > TimeSpan.Zero)
                    {
                        var fadeInAnimation = new Animation
                        {
                            Duration = MapBase.ImageFadeDuration,
                            Children =
                            {
                                new KeyFrame
                                {
                                    KeyTime = TimeSpan.Zero,
                                    Setters = { new Setter(Visual.OpacityProperty, 0d) }
                                },
                                new KeyFrame
                                {
                                    KeyTime = MapBase.ImageFadeDuration,
                                    Setters = { new Setter(Visual.OpacityProperty, 1d) }
                                }
                            }
                        };

                        _ = fadeInAnimation.RunAsync(Image);
                    }
                });
        }
    }
}
