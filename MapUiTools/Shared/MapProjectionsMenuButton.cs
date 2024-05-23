// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// Copyright © 2024 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
#if WPF
using System.Windows;
using System.Windows.Markup;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
#endif

namespace MapControl.UiTools
{
#if WPF
    [ContentProperty(nameof(Projection))]
#else
    [ContentProperty(Name = nameof(Projection))]
#endif
    public class MapProjectionItem
    {
        public string Text { get; set; }
        public string Projection { get; set; }
    }

#if WPF
    [ContentProperty(nameof(MapProjections))]
#else
    [ContentProperty(Name = nameof(MapProjections))]
#endif
    public class MapProjectionsMenuButton : MenuButton
    {
        private string selectedProjection;

        public MapProjectionsMenuButton()
            : base("\uE809")
        {
            ((INotifyCollectionChanged)MapProjections).CollectionChanged += (s, e) => InitializeMenu();
        }

        public static readonly DependencyProperty MapProperty =
            DependencyPropertyHelper.Register<MapProjectionsMenuButton, MapBase>(nameof(Map), null, false,
                (button, oldValue, newValue) => button.InitializeMenu());

        public MapBase Map
        {
            get => (MapBase)GetValue(MapProperty);
            set => SetValue(MapProperty, value);
        }

        public Collection<MapProjectionItem> MapProjections { get; } = new ObservableCollection<MapProjectionItem>();

        private void InitializeMenu()
        {
            if (Map != null)
            {
                var menu = CreateMenu();

                foreach (var item in MapProjections)
                {
                    menu.Items.Add(CreateMenuItem(item.Text, item.Projection, MapProjectionClicked));
                }

                var initialProjection = MapProjections.Select(p => p.Projection).FirstOrDefault();

                if (initialProjection != null)
                {
                    SetMapProjection(initialProjection);
                }
            }
        }

        private void MapProjectionClicked(object sender, RoutedEventArgs e)
        {
            var item = (FrameworkElement)sender;
            var projection = (string)item.Tag;

            SetMapProjection(projection);
        }

        private void SetMapProjection(string projection)
        {
            if (selectedProjection != projection)
            {
                selectedProjection = projection;
                Map.MapProjection = MapProjectionFactory.Instance.GetProjection(selectedProjection);
            }

            UpdateCheckedStates();
        }

        private void UpdateCheckedStates()
        {
            foreach (var item in GetMenuItems())
            {
                item.IsChecked = selectedProjection == (string)item.Tag;
            }
        }
    }
}
