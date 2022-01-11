// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2022 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.Generic;
using System.Linq;
#if WINUI
using Microsoft.UI.Xaml;
#elif UWP
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MapControl.UiTools
{
    public class MapProjectionsMenuButton : MenuButton
    {
        public MapProjectionsMenuButton()
            : base("\uE809")
        {
        }

        public static readonly DependencyProperty MapProperty = DependencyProperty.Register(
            nameof(Map), typeof(MapBase), typeof(MapProjectionsMenuButton),
            new PropertyMetadata(null, (o, e) => ((MapProjectionsMenuButton)o).InitializeMenu()));

        public static readonly DependencyProperty MapProjectionsProperty = DependencyProperty.Register(
            nameof(MapProjections), typeof(IDictionary<string, MapProjection>), typeof(MapProjectionsMenuButton),
            new PropertyMetadata(null, (o, e) => ((MapProjectionsMenuButton)o).InitializeMenu()));

        public MapBase Map
        {
            get { return (MapBase)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

        public IDictionary<string, MapProjection> MapProjections
        {
            get { return (IDictionary<string, MapProjection>)GetValue(MapProjectionsProperty); }
            set { SetValue(MapProjectionsProperty, value); }
        }

        private void InitializeMenu()
        {
            if (Map != null && MapProjections != null)
            {
                var menu = CreateMenu();

                foreach (var projection in MapProjections)
                {
                    menu.Items.Add(CreateMenuItem(projection.Key, projection.Value, MapProjectionClicked));
                }

                var initialProjection = MapProjections.Values.FirstOrDefault();

                if (initialProjection != null)
                {
                    SetMapProjection(initialProjection);
                }
            }
        }

        private void MapProjectionClicked(object sender, RoutedEventArgs e)
        {
            var item = (FrameworkElement)sender;
            var projection = (MapProjection)item.Tag;

            SetMapProjection(projection);
        }

        private void SetMapProjection(MapProjection projection)
        {
            Map.MapProjection = projection;

            foreach (var item in GetMenuItems())
            {
                item.IsChecked = Map.MapProjection == (MapProjection)item.Tag;
            }
        }
    }
}
