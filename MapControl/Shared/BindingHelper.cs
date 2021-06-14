// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
#elif WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
#endif

namespace MapControl
{
    internal static class BindingHelper
    {
        public static Binding GetBinding(this object sourceObject, string sourceProperty)
        {
            return new Binding { Source = sourceObject, Path = new PropertyPath(sourceProperty) };
        }

        public static void ValidateProperty(
            this FrameworkElement targetObject, DependencyProperty targetProperty, object sourceObject, string sourceProperty)
        {
            if (targetObject.GetValue(targetProperty) == null &&
                targetObject.GetBindingExpression(targetProperty) == null)
            {
                targetObject.SetBinding(targetProperty, sourceObject.GetBinding(sourceProperty));
            }
        }
    }
}
