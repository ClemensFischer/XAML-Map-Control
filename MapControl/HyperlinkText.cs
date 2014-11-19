// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#if WINDOWS_RUNTIME
using Windows.UI.Xaml.Documents;
#else
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
        public static List<Inline> ToInlines(this string text)
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
#elif !WINDOWS_RUNTIME
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
    }
}
