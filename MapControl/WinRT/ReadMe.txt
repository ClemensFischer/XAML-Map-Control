The Visual Studio Project for Windows Runtime resides in a separate folder
MapControl/WinRT, because it needs to have its own Themes/Generic.xaml file.
Output is generated to ../bin.

This folder also contains the file MapControl.WinRT.xr.xml, which overwrites
the generated file in the output folder. This is to remove the XML namespace
declaration on the <Roots> element, which prevents a .NET Native build of a
Universal Windows App that uses the MapControl.WinRT portable library.
