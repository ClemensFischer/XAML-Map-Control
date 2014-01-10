// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace MapControl
{
    public static class AnimationEx
    {
        public static void BeginAnimation(this DependencyObject obj, DependencyProperty property, DoubleAnimation animation)
        {
            animation.EnableDependentAnimation = true;

            if (property == UIElement.OpacityProperty)
            {
                BeginAnimation(obj, "Opacity", animation);
            }
            else if (property == MapBase.ZoomLevelProperty)
            {
                BeginAnimation(obj, "ZoomLevel", animation);
            }
            else if (property == MapBase.HeadingProperty)
            {
                BeginAnimation(obj, "Heading", animation);
            }
        }

        public static void BeginAnimation(this DependencyObject obj, DependencyProperty property, PointAnimation animation)
        {
            animation.EnableDependentAnimation = true;

            if (property == MapBase.CenterPointProperty)
            {
                BeginAnimation(obj, "CenterPoint", animation);
            }
        }

        private static void BeginAnimation(DependencyObject obj, string property, Timeline animation)
        {
            Storyboard.SetTargetProperty(animation, property);
            Storyboard.SetTarget(animation, obj);
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }
}
