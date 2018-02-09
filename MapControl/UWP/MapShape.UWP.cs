// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using Windows.UI.Xaml.Shapes;

namespace MapControl
{
    public abstract partial class MapShape : Path
    {
        private void ParentMapChanged()
        {
            UpdateData();
        }

        private void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            UpdateData();
        }
    }
}
