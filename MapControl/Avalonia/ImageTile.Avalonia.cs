using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace MapControl
{
    public class ImageTile(int zoomLevel, int x, int y, int columnCount)
        : Tile(zoomLevel, x, y, columnCount)
    {
        public Image Image { get; } = new Image { Stretch = Stretch.Fill };

        public override async Task LoadImageAsync(Func<Task<IImage>> loadImageFunc)
        {
            var image = await loadImageFunc().ConfigureAwait(false);

            void SetImageSource()
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
            }

            await Dispatcher.UIThread.InvokeAsync(SetImageSource);
        }
    }
}
