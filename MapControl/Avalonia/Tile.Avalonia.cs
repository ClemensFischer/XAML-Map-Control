// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

namespace MapControl
{
    public partial class Tile
    {
        private void AnimateImageOpacity()
        {
            _ = OpacityHelper.FadeIn(Image);
        }
    }
}
