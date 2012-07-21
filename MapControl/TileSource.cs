// WPF MapControl - http://wpfmapcontrol.codeplex.com/
// Copyright © 2012 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapControl
{
    /// <summary>
    /// Provides the URI of a map tile.
    /// </summary>
    [TypeConverter(typeof(TileSourceTypeConverter))]
    public class TileSource
    {
        private Func<int, int, int, Uri> getUri;
        private string uriFormat = string.Empty;
        private int hostIndex = -1;

        public TileSource()
        {
        }

        public TileSource(string uriFormat)
        {
            UriFormat = uriFormat;
        }

        public string UriFormat
        {
            get { return uriFormat; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("The value of the UriFormat proprty must not be null or empty or white-space only.");
                }

                if (value.Contains("{x}") && value.Contains("{y}") && value.Contains("{z}"))
                {
                    if (value.Contains("{c}"))
                    {
                        getUri = GetOpenStreetMapUri;
                    }
                    else if (value.Contains("{i}"))
                    {
                        getUri = GetGoogleMapsUri;
                    }
                    else if (value.Contains("{n}"))
                    {
                        getUri = GetMapQuestUri;
                    }
                    else
                    {
                        getUri = GetDefaultUri;
                    }
                }
                else if (value.Contains("{q}")) // {i} is optional
                {
                    getUri = GetQuadKeyUri;
                }
                else if (value.Contains("{w}") && value.Contains("{s}") && value.Contains("{e}") && value.Contains("{n}"))
                {
                    getUri = GetBoundingBoxUri;
                }
                else
                {
                    throw new ArgumentException("The specified UriFormat is not supported.");
                }

                uriFormat = value;
            }
        }

        public virtual Uri GetUri(int x, int y, int zoomLevel)
        {
            return getUri != null ? getUri(x, y, zoomLevel) : null;
        }

        private Uri GetDefaultUri(int x, int y, int zoomLevel)
        {
            return new Uri(UriFormat.
                Replace("{x}", x.ToString()).
                Replace("{y}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }

        private Uri GetOpenStreetMapUri(int x, int y, int zoomLevel)
        {
            hostIndex = (hostIndex + 1) % 3;

            return new Uri(UriFormat.
                Replace("{c}", "abc".Substring(hostIndex, 1)).
                Replace("{x}", x.ToString()).
                Replace("{y}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }

        private Uri GetGoogleMapsUri(int x, int y, int zoomLevel)
        {
            hostIndex = (hostIndex + 1) % 4;

            return new Uri(UriFormat.
                Replace("{i}", hostIndex.ToString()).
                Replace("{x}", x.ToString()).
                Replace("{y}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }

        private Uri GetMapQuestUri(int x, int y, int zoomLevel)
        {
            hostIndex = (hostIndex % 4) + 1;

            return new Uri(UriFormat.
                Replace("{n}", hostIndex.ToString()).
                Replace("{x}", x.ToString()).
                Replace("{y}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }

        private Uri GetQuadKeyUri(int x, int y, int zoomLevel)
        {
            StringBuilder key = new StringBuilder { Length = zoomLevel };

            for (int z = zoomLevel - 1; z >= 0; z--, x /= 2, y /= 2)
            {
                key[z] = (char)('0' + 2 * (y % 2) + (x % 2));
            }

            return new Uri(UriFormat.
                Replace("{i}", key.ToString(key.Length - 1, 1)).
                Replace("{q}", key.ToString()));
        }

        private Uri GetBoundingBoxUri(int x, int y, int zoomLevel)
        {
            MercatorTransform t = new MercatorTransform();
            double n = 1 << zoomLevel;
            double x1 = (double)x * 360d / n - 180d;
            double x2 = (double)(x + 1) * 360d / n - 180d;
            double y1 = 180d - (double)(y + 1) * 360d / n;
            double y2 = 180d - (double)y * 360d / n;
            Location p1 = t.TransformBack(new Point(x1, y1));
            Location p2 = t.TransformBack(new Point(x2, y2));

            return new Uri(UriFormat.
                Replace("{w}", p1.Longitude.ToString(CultureInfo.InvariantCulture)).
                Replace("{s}", p1.Latitude.ToString(CultureInfo.InvariantCulture)).
                Replace("{e}", p2.Longitude.ToString(CultureInfo.InvariantCulture)).
                Replace("{n}", p2.Latitude.ToString(CultureInfo.InvariantCulture)));
        }
    }

    /// <summary>
    /// Provides the image of a map tile. ImageTileSource bypasses download and
    /// cache processing in TileImageLoader. By overriding the GetImage method,
    /// an application can provide tile images from an arbitrary source.
    /// </summary>
    public class ImageTileSource : TileSource
    {
        public virtual ImageSource GetImage(int x, int y, int zoomLevel)
        {
            return new BitmapImage(GetUri(x, y, zoomLevel));
        }
    }

    /// <summary>
    /// Converts from string to TileSource.
    /// </summary>
    public class TileSourceTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return new TileSource(value as string);
        }
    }
}
