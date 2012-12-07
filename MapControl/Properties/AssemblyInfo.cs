using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

#if SILVERLIGHT
[assembly: AssemblyTitle("MapControl.Silverlight")]
[assembly: AssemblyDescription("XAML Map Control for Silverlight")]
#else
[assembly: AssemblyTitle("MapControl.WPF")]
[assembly: AssemblyDescription("XAML Map Control for WPF")]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
#endif
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Clemens Fischer")]
[assembly: AssemblyProduct("XAML Map Control")]
[assembly: AssemblyCopyright("Copyright © 2012 Clemens Fischer")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.1.0")]
[assembly: AssemblyFileVersion("1.1.0")]
[assembly: ComVisible(false)]
