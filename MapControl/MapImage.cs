// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
#endif

namespace MapControl
{
    public partial class MapImage : MapRectangle
    {
        private static readonly Transform imageTransform = new MatrixTransform
        {
            Matrix = new Matrix(1, 0, 0, -1, 0, 1)
        };

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(ImageSource), typeof(MapImage),
            new PropertyMetadata(null, (o, e) => ((MapImage)o).SourceChanged((ImageSource)e.NewValue)));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public bool AnimateOpacity { get; set; }

        private void SourceChanged(ImageSource image)
        {
            Fill = new ImageBrush
            {
                ImageSource = image,
                RelativeTransform = imageTransform,
                Opacity = 0d
            };

            if (AnimateOpacity)
            {
                BeginOpacityAnimation(image);
            }
            else
            {
                Fill.Opacity = 1d;
            }
        }

        private void BeginOpacityAnimation()
        {
            Fill.BeginAnimation(Brush.OpacityProperty,
                new DoubleAnimation
                {
                    To = 1d,
                    Duration = Tile.AnimationDuration,
                    FillBehavior = FillBehavior.HoldEnd
                });
        }
    }
}
