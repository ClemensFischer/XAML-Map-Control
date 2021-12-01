// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2021 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
#elif UWP
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
        /// <summary>
        /// Returns a Binding to the specified dependency property of a FrameworkElement.
        /// If the source property is itself already bound, the method returns the existing Binding,
        /// otherwise it creates one with sourceElement as Source and sourcePropertyName as Path.
        /// </summary>
        public static Binding GetOrCreateBinding(
            this FrameworkElement sourceElement, DependencyProperty sourceProperty, string sourcePropertyName)
        {
            var sourceBinding = sourceElement.GetBindingExpression(sourceProperty);

            return sourceBinding != null
                ? sourceBinding.ParentBinding
                : new Binding { Source = sourceElement, Path = new PropertyPath(sourcePropertyName) };
        }

        /// <summary>
        /// Sets a Binding on the specified dependency property of targetElement, if the target property does
        /// not yet have a value or a Binding assigned to it. The potentially assigned Binding is created by
        /// GetOrCreateBinding(sourceElement, sourceProperty, sourcePropertyName).
        /// </summary>
        public static void SetBindingOnUnsetProperty(
            this FrameworkElement targetElement, DependencyProperty targetProperty,
            FrameworkElement sourceElement, DependencyProperty sourceProperty, string sourcePropertyName)
        {
            if (targetElement.GetValue(targetProperty) == null &&
                targetElement.GetBindingExpression(targetProperty) == null)
            {
                targetElement.SetBinding(targetProperty, GetOrCreateBinding(sourceElement, sourceProperty, sourcePropertyName));
            }
        }
    }
}
