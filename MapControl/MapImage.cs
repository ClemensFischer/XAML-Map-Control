// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2013 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    public class MapImage : MapRectangle
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

        private void SourceChanged(ImageSource source)
        {
            Fill = new ImageBrush
            {
                ImageSource = source,
                RelativeTransform = imageTransform
            };
        }
    }
}
