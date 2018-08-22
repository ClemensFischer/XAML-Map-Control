#if !HAS_SYSTEM_ICLONEABLE
namespace GeoAPI
{
    /// <summary>
    /// A framework replacement for the System.ICloneable interface.
    /// </summary>
    public interface ICloneable
    {
        /// <summary>
        /// Function to create a new object that is a (deep) copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        object Clone();
    }
}
#endif
