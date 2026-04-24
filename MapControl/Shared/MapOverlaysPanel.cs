using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
#if WPF
using System.Windows;
#elif UWP
using Windows.UI.Xaml;
#elif WINUI
using Microsoft.UI.Xaml;
#endif

namespace MapControl;

/// <summary>
/// A MapPanel with a collection of GroundOverlay or GeoImage children.
/// </summary>
public partial class MapOverlaysPanel : MapPanel
{
    public static readonly DependencyProperty SourcePathsProperty =
        DependencyPropertyHelper.Register<MapOverlaysPanel, IEnumerable<string>>(nameof(SourcePaths), null,
            async (control, oldValue, newValue) => await control.SourcePathsPropertyChanged(oldValue, newValue));

    private readonly UpdateTimer updateTimer;

    public MapOverlaysPanel()
    {
        updateTimer = new UpdateTimer { Interval = TimeSpan.FromMilliseconds(100) };
        updateTimer.Tick += async (_, _) => await UpdateChildren();
    }

    public IEnumerable<string> SourcePaths
    {
        get => (IEnumerable<string>)GetValue(SourcePathsProperty);
        set => SetValue(SourcePathsProperty, value);
    }

    protected virtual async Task<FrameworkElement> CreateOverlayAsync(string sourcePath)
    {
        var ext = Path.GetExtension(sourcePath).ToLower();

        return ext == ".kmz" || ext == ".kml"
            ? await GroundOverlay.CreateAsync(sourcePath)
            : await GeoImage.CreateAsync(sourcePath);
    }

    private async Task<FrameworkElement> GetOverlay(string sourcePath)
    {
        var overlay = Children.Cast<FrameworkElement>().FirstOrDefault(child => (string)child.Tag == sourcePath);

        if (overlay == null)
        {
            overlay = await CreateOverlayAsync(sourcePath);
            overlay?.Tag = sourcePath;
        }

        return overlay;
    }

    private async Task UpdateChildren()
    {
        updateTimer.Stop();

        var overlays = SourcePaths != null
            ? await Task.WhenAll(SourcePaths.ToList().Select(GetOverlay))
            : [];

        Children.Clear();

        foreach (var overlay in overlays.Where(overlay => overlay != null))
        {
            Children.Add(overlay);
        }
    }

    private void SourcePathsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        updateTimer.Run(true);
    }

    private async Task SourcePathsPropertyChanged(IEnumerable<string> oldSourcePaths, IEnumerable<string> newSourcePaths)
    {
        if (oldSourcePaths is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= SourcePathsCollectionChanged;
        }

        if (newSourcePaths is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += SourcePathsCollectionChanged;
        }

        await UpdateChildren();
    }
}
