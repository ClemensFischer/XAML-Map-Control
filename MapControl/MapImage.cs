// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
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
    /// <summary>
    /// Fills a rectangular area with an ImageBrush from the Source property.
    /// </summary>
    public class MapImage : MapRectangle
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(ImageSource), typeof(MapImage),
            new PropertyMetadata(null, (o, e) => ((ImageBrush)((MapImage)o).Fill).ImageSource = (ImageSource)e.NewValue));

        public MapImage()
        {
            Fill = new ImageBrush
            {
                RelativeTransform = new MatrixTransform
                {
                    Matrix = new Matrix(1d, 0d, 0d, -1d, 0d, 1d)
                }
            };
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
    }
}
