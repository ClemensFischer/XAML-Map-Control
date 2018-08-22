using System;
#if COMPAT_BOOTSTRAP_USING_REFLECTION && HAS_SYSTEM_APPDOMAIN_GETASSEMBLIES && HAS_SYSTEM_REFLECTION_ASSEMBLY_GETEXPORTEDTYPES
using System.Reflection;
#endif

namespace GeoAPI
{
    /// <summary>
    /// Static class that provides access to a  <see cref="IGeometryServices"/> class.
    /// </summary>
    public static class GeometryServiceProvider
    {
        private static volatile IGeometryServices s_instance;

        /// <summary>
        /// Make sure only one thread runs <see cref="InitializeInstance"/> at a time.
        /// </summary>
        private static readonly object s_autoInitLock = new object();

        /// <summary>
        /// Make sure that anyone who directly sets <see cref="Instance"/>, including the automatic
        /// initializer, behaves consistently, regarding <see cref="s_instanceSetDirectly"/> and the
        /// semantics of <see cref="SetInstanceIfNotAlreadySetDirectly"/>.
        /// </summary>
        private static readonly object s_explicitInitLock = new object();

        /// <summary>
        /// Indicates whether or not <see cref="s_instance"/> has been set directly (i.e., outside
        /// of the reflection-based initializer).
        /// </summary>
        private static bool s_instanceSetDirectly = false;

        /// <summary>
        /// Gets or sets the <see cref="IGeometryServices"/> instance.
        /// </summary>
        public static IGeometryServices Instance
        {
            get => s_instance ?? InitializeInstance();
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                lock (s_explicitInitLock)
                {
                    s_instance = value;
                    s_instanceSetDirectly = true;
                }
            }
        }

        /// <summary>
        /// Sets <see cref="Instance"/> to the given value, unless it has already been set directly.
        /// Both this method and the property's setter itself count as setting it "directly".
        /// </summary>
        /// <param name="instance">
        /// The new value to put into <see cref="Instance"/> if it hasn't already been set directly.
        /// </param>
        /// <returns>
        /// <c>true</c> if <see cref="Instance"/> was set, <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="instance"/> is <see langword="null"/>.
        /// </exception>
        public static bool SetInstanceIfNotAlreadySetDirectly(IGeometryServices instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            lock (s_explicitInitLock)
            {
                if (s_instanceSetDirectly)
                {
                    // someone has already set the value directly before.
                    return false;
                }

                s_instance = instance;

                // calling this method counts.
                s_instanceSetDirectly = true;
                return true;
            }
        }

        private static IGeometryServices InitializeInstance()
        {
#if COMPAT_BOOTSTRAP_USING_REFLECTION && HAS_SYSTEM_APPDOMAIN_GETASSEMBLIES && HAS_SYSTEM_REFLECTION_ASSEMBLY_GETEXPORTEDTYPES
            lock (s_autoInitLock)
            {
                // see if someone has already set it while we were waiting for the lock.
                var instance = s_instance;
                if (instance != null) return instance;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GlobalAssemblyCache && assembly.CodeBase == Assembly.GetExecutingAssembly().CodeBase)
                        continue;

                    var assemblyType = assembly.GetType().FullName;
                    if (assemblyType == "System.Reflection.Emit.AssemblyBuilder" ||
                        assemblyType == "System.Reflection.Emit.InternalAssemblyBuilder")
                        continue;

                    Type[] types;

                    try
                    {
                        types = assembly.GetExportedTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        types = ex.Types;
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    var requiredType = typeof(IGeometryServices);
                    foreach (var type in types)
                    {
                        if (type.IsNotPublic || type.IsInterface || type.IsAbstract || !requiredType.IsAssignableFrom(type))
                            continue;

                        foreach (var constructor in type.GetConstructors())
                            if (constructor.IsPublic && constructor.GetParameters().Length == 0)
                            {
                                instance = (IGeometryServices)Activator.CreateInstance(type);
                                lock (s_explicitInitLock)
                                {
                                    if (!s_instanceSetDirectly)
                                    {
                                        s_instance = instance;
                                    }

                                    return s_instance;
                                }
                            }
                    }
                }
            }
#endif
            throw new InvalidOperationException("Cannot use GeometryServiceProvider without an assigned IGeometryServices class");
        }
    }
}
