// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace MapControl
{
    internal static class Extensions
    {
        public static IAsyncAction BeginInvoke(this CoreDispatcher dispatcher, Action action)
        {
            return dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(action));
        }

        public static Point Transform(this GeneralTransform transform, Point point)
        {
            return transform.TransformPoint(point);
        }

        public static void BeginAnimation(this DependencyObject obj, DependencyProperty property, DoubleAnimation animation)
        {
            animation.EnableDependentAnimation = true;
            BeginAnimation(obj, property, (Timeline)animation);
        }

        public static void BeginAnimation(this DependencyObject obj, DependencyProperty property, PointAnimation animation)
        {
            animation.EnableDependentAnimation = true;
            BeginAnimation(obj, property, (Timeline)animation);
        }

        private static Dictionary<DependencyProperty, string> properties = new Dictionary<DependencyProperty, string>()
        {
            { UIElement.OpacityProperty, "Opacity" },
            { MapBase.ZoomLevelProperty, "ZoomLevel" },
            { MapBase.HeadingProperty, "Heading" },
            { MapBase.CenterPointProperty, "CenterPoint" }
        };

        private static void BeginAnimation(DependencyObject obj, DependencyProperty property, Timeline animation)
        {
            string propertyName;
            if (properties.TryGetValue(property, out propertyName))
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
