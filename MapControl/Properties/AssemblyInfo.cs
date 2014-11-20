using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

#if WINDOWS_PHONE
[assembly: AssemblyTitle("XAML Map Control (Windows Phone Silverlight)")]
[assembly: AssemblyDescription("XAML Map Control Library for Windows Phone Silverlight")]
#elif SILVERLIGHT
[assembly: AssemblyTitle("XAML Map Control (Silverlight)")]
[assembly: AssemblyDescription("XAML Map Control Library for Silverlight")]
#else
[assembly: AssemblyTitle("XAML Map Control (WPF)")]
[assembly: AssemblyDescription("XAML Map Control Library for WPF")]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
#endif
[assembly: AssemblyProduct("XAML Map Control")]
[assembly: AssemblyCompany("Clemens Fischer")]
[assembly: AssemblyCopyright("Copyright © 2014 Clemens Fischer")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyVersion("2.4.1")]
[assembly: AssemblyFileVersion("2.4.1")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
