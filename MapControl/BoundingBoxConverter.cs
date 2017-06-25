// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2017 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.ComponentModel;
using System.Globalization;

namespace MapControl
{
    public class BoundingBoxConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return BoundingBox.Parse((string)value);
        }
    }

    [TypeConverter(typeof(BoundingBoxConverter))]
#if !SILVERLIGHT
    [Serializable]
#endif
    public partial class BoundingBox
    {
    }
}
