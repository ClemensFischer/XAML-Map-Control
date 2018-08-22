using System;

// Ref: http://www.yortondotnet.com/2009/11/tryparse-for-compact-framework.html

namespace GeoAPI
{
    /// <summary>
    /// Provides methods to parse simple value types without throwing format exception.
    /// </summary>
    internal static class ValueParser
    {
        /// <summary>
        /// Attempts to convert the string representation of a number in a
        /// specified style and culture-specific format to its double-precision
        /// floating-point number equivalent.
        /// </summary>
        /// <param name="s">The string to attempt to parse.</param>
        /// <param name="style">
        /// A bitwise combination of <see cref="System.Globalization.NumberStyles"/>
        /// values that indicates the permitted format of <paramref name="s"/>.
        /// </param>
        /// <param name="provider">
        /// A <see cref="System.IFormatProvider"/> that supplies
        /// culture-specific formatting information about <paramref name="s"/>.
        /// </param>
        /// <param name="result">The result of the parsed string, or zero if parsing failed.</param>
        /// <returns>A boolean value indicating whether or not the parse succeeded.</returns>
        /// <remarks>Returns 0 in the result parameter if the parse fails.</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s")]
        public static bool TryParse(string s, System.Globalization.NumberStyles style, IFormatProvider provider, out double result)
        {
            bool retVal = false;
#if HAS_SYSTEM_DOUBLE_TRYPARSE
            retVal = double.TryParse(s, style, provider, out result);
#else
            try
            {
                result = double.Parse(s, style, provider);
                retVal = true;
            }
            catch (FormatException) { result = 0; }
            catch (InvalidCastException) { result = 0; }
#endif
            return retVal;
        }
    }
}
