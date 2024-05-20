
using Avalonia.Controls;
using System;

namespace MapControl
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("AvaloniaProperty", "AVP1001")]
    public static class DependencyPropertyHelper
    {
        public static AvaloniaProperty Register<TOwner, TValue>(
            string name,
            TValue defaultValue = default,
            bool bindTwoWayByDefault = false,
            Action<TOwner, TValue, TValue> propertyChanged = null)
            where TOwner : AvaloniaObject
        {
            StyledProperty<TValue> property = AvaloniaProperty.Register<TOwner, TValue>(name, defaultValue, false,
                bindTwoWayByDefault ? Avalonia.Data.BindingMode.TwoWay : Avalonia.Data.BindingMode.OneWay);

            if (propertyChanged != null)
            {
                property.Changed.AddClassHandler<TOwner, TValue>(
                    (o, e) => propertyChanged(o, e.OldValue.Value, e.NewValue.Value));
            }

            return property;
        }

        public static AvaloniaProperty RegisterAttached<TOwner, TValue>(
            string name,
            TValue defaultValue = default,
            bool inherits = false,
            Action<Control, TValue, TValue> propertyChanged = null)
            where TOwner : AvaloniaObject
        {
            AttachedProperty<TValue> property = AvaloniaProperty.RegisterAttached<TOwner, Control, TValue>(name, defaultValue, inherits);

            if (propertyChanged != null)
            {
                property.Changed.AddClassHandler<Control, TValue>(
                    (o, e) => propertyChanged(o, e.OldValue.Value, e.NewValue.Value));
            }

            return property;
        }
    }
}
