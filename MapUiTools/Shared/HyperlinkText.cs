using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#if WPF
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
#elif WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
#elif AVALONIA
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using DependencyProperty = Avalonia.AvaloniaProperty;
#endif

namespace MapControl.UiTools
{
    public static class HyperlinkText
    {
        private static readonly Regex regex = new Regex(@"\[([^\]]+)\]\(([^\)]+)\)");

        /// <summary>
        /// Converts text containing hyperlinks in markdown syntax [text](url)
        /// to a collection of Run and Hyperlink inlines.
        /// </summary>
        public static IEnumerable<Inline> TextToInlines(this string text)
        {
            var inlines = new List<Inline>();

            while (!string.IsNullOrEmpty(text))
            {
                var match = regex.Match(text);

                if (match.Success &&
                    match.Groups.Count == 3 &&
                    Uri.TryCreate(match.Groups[2].Value, UriKind.Absolute, out Uri uri))
                {
                    inlines.Add(new Run
                    {
                        Text = text.Substring(0, match.Index),
#if WPF || AVALONIA
                        BaselineAlignment = BaselineAlignment.Center
#endif
                    });

                    text = text.Substring(match.Index + match.Length);

                    var hyperlinkText = match.Groups[1].Value;
#if AVALONIA
                    var button = new HyperlinkButton
                    {
                        Content = hyperlinkText,
                        NavigateUri = uri,
                        Padding = new Thickness(0),
                        Margin = new Thickness(0)
                    };

                    var link = new InlineUIContainer
                    {
                        Child = button,
                        BaselineAlignment = BaselineAlignment.Center
                    };
#else
                    var run = new Run { Text = hyperlinkText };
                    var link = new Hyperlink { NavigateUri = uri };
                    link.Inlines.Add(run);
#if WPF
                    run.BaselineAlignment = BaselineAlignment.Center;
                    link.BaselineAlignment = BaselineAlignment.Center;
                    link.ToolTip = uri.ToString();
                    link.RequestNavigate += (_, e) =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"{e.Uri}: {ex}");
                        }
                    };
#endif
#endif
                    inlines.Add(link);
                }
                else
                {
                    inlines.Add(new Run { Text = text });
                    text = null;
                }
            }

            return inlines;
        }

        public static readonly DependencyProperty InlinesSourceProperty =
            DependencyPropertyHelper.RegisterAttached<string>("InlinesSource", typeof(HyperlinkText), null,
                (element, oldValue, newValue) =>
                {
                    if (element is TextBlock textBlock)
                    {
                        textBlock.Inlines.Clear();

                        foreach (var inline in TextToInlines(newValue))
                        {
                            textBlock.Inlines.Add(inline);
                        }
                    }
                });

        public static string GetInlinesSource(TextBlock element)
        {
            return (string)element.GetValue(InlinesSourceProperty);
        }

        public static void SetInlinesSource(TextBlock element, string value)
        {
            element.SetValue(InlinesSourceProperty, value);
        }
    }
}
