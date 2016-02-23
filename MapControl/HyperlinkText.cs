// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// © 2016 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
#endif

namespace MapControl
{
    public static class HyperlinkText
    {
        private static Regex regex = new Regex(@"\[([^\]]+)\]\(([^\)]+)\)");

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
                Uri uri;

                if (match.Success &&
                    match.Groups.Count == 3 &&
                    Uri.TryCreate(match.Groups[2].Value, UriKind.Absolute, out uri))
                {
                    inlines.Add(new Run { Text = text.Substring(0, match.Index) });
                    text = text.Substring(match.Index + match.Length);

                    var link = new Hyperlink { NavigateUri = uri };
                    link.Inlines.Add(new Run { Text = match.Groups[1].Value });
#if SILVERLIGHT
                    link.TargetName = "_blank";
#elif !NETFX_CORE
                    link.ToolTip = uri.ToString();
                    link.RequestNavigate += (s, e) => System.Diagnostics.Process.Start(e.Uri.ToString());
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

        public static readonly DependencyProperty InlinesSourceProperty = DependencyProperty.RegisterAttached(
            "InlinesSource", typeof(string), typeof(HyperlinkText), new PropertyMetadata(null, InlinesSourcePropertyChanged));

        public static string GetInlinesSource(UIElement element)
        {
            return (string)element.GetValue(InlinesSourceProperty);
        }

        public static void SetInlinesSource(UIElement element, string value)
        {
            element.SetValue(InlinesSourceProperty, value);
        }

        private static void InlinesSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            InlineCollection inlines = null;

            if (obj is TextBlock)
            {
                inlines = ((TextBlock)obj).Inlines;
            }
            else if (obj is Paragraph)
            {
                inlines = ((Paragraph)obj).Inlines;
            }
#if NETFX_CORE || SILVERLIGHT
            else if (obj is RichTextBlock)
            {
                var paragraph = new Paragraph();
                inlines = paragraph.Inlines;
                var richTextBlock = (RichTextBlock)obj;
                richTextBlock.Blocks.Clear();
                richTextBlock.Blocks.Add(paragraph);
            }
#endif
            if (inlines != null)
            {
                inlines.Clear();

                foreach (var inline in TextToInlines((string)e.NewValue))
                {
                    inlines.Add(inline);
                }
            }
        }
    }
}
