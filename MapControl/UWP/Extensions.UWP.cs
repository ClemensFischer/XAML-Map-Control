// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace MapControl
{
    internal static class Extensions
    {
        public static void BeginAnimation(this DependencyObject obj, DependencyProperty property, Timeline animation)
        {
            if (animation != null)
            {
                string propertyName = null;

                if (property == MapBase.CenterPointProperty)
                {
                    propertyName = "CenterPoint";
                    ((PointAnimation)animation).EnableDependentAnimation = true;
                }
                else if (property == MapBase.ZoomLevelProperty)
                {
                    propertyName = "ZoomLevel";
                    ((DoubleAnimation)animation).EnableDependentAnimation = true;
                }
                else if (property == MapBase.HeadingProperty)
                {
                    propertyName = "Heading";
                    ((DoubleAnimation)animation).EnableDependentAnimation = true;
                }
                else if (property == UIElement.OpacityProperty)
                {
                    propertyName = "Opacity";
                }

                if (propertyName != null)
                {
                    Storyboard.SetTargetProperty(animation, propertyName);
                    Storyboard.SetTarget(animation, obj);

                    var storyboard = new Storyboard();
                    storyboard.Children.Add(animation);
                    storyboard.Begin();
                }
            }
        }
    }
}
