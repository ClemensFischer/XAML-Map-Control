using System;

#pragma warning disable AVP1001

namespace MapControl
{
    public static class DependencyPropertyHelper
    {
        public static AttachedProperty<TValue> RegisterAttached<TValue>(
            string name,
            Type ownerType,
            TValue defaultValue = default,
            Action<Control, TValue, TValue> changed = null,
            bool inherits = false)
        {
            var property = AvaloniaProperty.RegisterAttached<Control, TValue>(name, ownerType, defaultValue, inherits);

            if (changed != null)
            {
                property.Changed.AddClassHandler<Control, TValue>((o, e) => changed(o, e.OldValue.Value, e.NewValue.Value));
            }

            return property;
        }

        public static StyledProperty<TValue> Register<TOwner, TValue>(
            string name,
            TValue defaultValue = default,
            Action<TOwner, TValue, TValue> changed = null,
            Func<TOwner, TValue, TValue> coerce = null,
            bool bindTwoWayByDefault = false)
            where TOwner : AvaloniaObject
        {
            Func<AvaloniaObject, TValue, TValue> coerceFunc = null;

            if (coerce != null)
            {
                // Do not coerce default value.
                //
                coerceFunc = (obj, value) => Equals(value, defaultValue) ? value : coerce((TOwner)obj, value);
            }

            var bindingMode = bindTwoWayByDefault ? BindingMode.TwoWay : BindingMode.OneWay;

            var property = AvaloniaProperty.Register<TOwner, TValue>(name, defaultValue, false, bindingMode, null, coerceFunc);

            if (changed != null)
            {
                property.Changed.AddClassHandler<TOwner, TValue>((o, e) => changed(o, e.OldValue.Value, e.NewValue.Value));
            }

            return property;
        }

        public static void SetBinding(this AvaloniaObject target, AvaloniaProperty property, Binding binding)
        {
            target.Bind(property, binding);
        }
    }
}
