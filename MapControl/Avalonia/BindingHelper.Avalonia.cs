// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Avalonia.Data;

namespace MapControl
{
    internal static class BindingHelper
    {
        public static Binding CreateBinding(this object source, string property)
        {
            return new Binding
            {
                Source = source,
                Path = property
            };
        }

        public static void SetBinding(this AvaloniaObject target, AvaloniaProperty property, Binding binding)
        {
            target.Bind(property, binding);
        }
    }
}
