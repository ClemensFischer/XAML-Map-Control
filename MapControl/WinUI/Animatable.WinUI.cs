// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
#endif

namespace MapControl
{
    internal static class Animatable
    {
        public static void BeginAnimation(this DependencyObject obj, string property, Timeline animation)
        {
            Storyboard.SetTargetProperty(animation, property);
            Storyboard.SetTarget(animation, obj);

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        public static void BeginAnimation(this DependencyObject obj, DependencyProperty property, Timeline animation)
        {
            if (animation != null && property == UIElement.OpacityProperty)
            {
                BeginAnimation(obj, nameof(UIElement.Opacity), animation);
            }
        }
    }
}
