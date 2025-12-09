using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using System;

namespace MapControl.UiTools
{
    public partial class MenuButton
    {
        public MenuButton()
        {
            var style = new Style();
            style.Setters.Add(new Setter(TextBlock.FontFamilyProperty, new FontFamily("Segoe MDL2 Assets")));
            style.Setters.Add(new Setter(TextBlock.FontSizeProperty, 20d));
            style.Setters.Add(new Setter(PaddingProperty, new Thickness(8)));
            Styles.Add(style);

            Flyout = new MenuFlyout();
            Loaded += async (_, _) => await Initialize();
        }

        public string Icon
        {
            get => Content as string;
            set => Content = value;
        }

        public MenuFlyout Menu => (MenuFlyout)Flyout;

        [Content]
        public ItemCollection Items => Menu.Items;

        protected override Type StyleKeyOverride => typeof(Button);
    }
}
