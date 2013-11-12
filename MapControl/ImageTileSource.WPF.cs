// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © Clemens Fischer 2012-2013
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
