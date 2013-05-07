// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
// Licensed under the Microsoft Public License (Ms-PL)

using System.Windows;
using System.Windows.Media.Animation;

namespace MapControl
{
    public static class AnimationEx
    {
        public static void BeginAnimation(this DependencyObject obj, DependencyProperty property, Timeline animation)
        {
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));
            Storyboard.SetTarget(animation, obj);
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }
}
