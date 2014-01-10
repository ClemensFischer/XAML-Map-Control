// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public partial class ImageTileSource
    {
        public virtual bool CanLoadAsync
        {
            get { return false; }
        }
    }
}
