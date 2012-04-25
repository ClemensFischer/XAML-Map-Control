using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;

namespace MapControl
{
    [TypeConverter(typeof(TileSourceTypeConverter))]
    public class TileSource
    {
        public string UriFormat { get; set; }

        public virtual Uri GetUri(int x, int y, int zoomLevel)
        {
            return new Uri(UriFormat.
                Replace("{x}", x.ToString()).
                Replace("{y}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }
    }

    public class OpenStreetMapTileSource : TileSource
    {
        private static string[] hostChars = { "a", "b", "c" };
        private int hostChar = -1;

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            hostChar = (hostChar + 1) % 3;

            return new Uri(UriFormat.
                Replace("{c}", hostChars[hostChar]).
                Replace("{x}", x.ToString()).
                Replace("{y}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }
    }

    public class GoogleMapsTileSource : TileSource
    {
        private int hostIndex = -1;

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            hostIndex = (hostIndex + 1) % 4;

            return new Uri(UriFormat.
                Replace("{i}", hostIndex.ToString()).
                Replace("{x}", x.ToString()).
                Replace("{y}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }
    }

    public class MapQuestTileSource : TileSource
    {
        private int hostNumber;

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            hostNumber = (hostNumber % 4) + 1;

            return new Uri(UriFormat.
                Replace("{n}", hostNumber.ToString()).
                Replace("{x}", x.ToString()).
                Replace("{y}", y.ToString()).
                Replace("{z}", zoomLevel.ToString()));
        }
    }

    public class QuadKeyTileSource : TileSource
    {
        public override Uri GetUri(int x, int y, int zoomLevel)
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
    }

    public class BoundingBoxTileSource : TileSource
    {
        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            InverseMercatorTransform t = new InverseMercatorTransform();
            double n = 1 << zoomLevel;
            double x1 = (double)x * 360d / n - 180d;
            double x2 = (double)(x + 1) * 360d / n - 180d;
            double y1 = 180d - (double)(y + 1) * 360d / n;
            double y2 = 180d - (double)y * 360d / n;
            Point p1 = t.Transform(new Point(x1, y1));
            Point p2 = t.Transform(new Point(x2, y2));

            return new Uri(UriFormat.
                Replace("{w}", p1.X.ToString(CultureInfo.InvariantCulture)).
                Replace("{s}", p1.Y.ToString(CultureInfo.InvariantCulture)).
                Replace("{e}", p2.X.ToString(CultureInfo.InvariantCulture)).
                Replace("{n}", p2.Y.ToString(CultureInfo.InvariantCulture)));
        }
    }

    public class TileSourceTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string uriFormat = value as string;

            if (uriFormat != null)
            {
                TileSource tileSource = null;

                if (uriFormat.Contains("{x}") && uriFormat.Contains("{y}") && uriFormat.Contains("{z}"))
                {
                    if (uriFormat.Contains("{c}"))
                    {
                        tileSource = new OpenStreetMapTileSource();
                    }
                    else if (uriFormat.Contains("{i}"))
                    {
                        tileSource = new GoogleMapsTileSource();
                    }
                    else if (uriFormat.Contains("{n}"))
                    {
                        tileSource = new MapQuestTileSource();
                    }
                    else
                    {
                        tileSource = new TileSource();
                    }
                }
                else if (uriFormat.Contains("{q}"))
                {
                    tileSource = new QuadKeyTileSource();
                }
                else if (uriFormat.Contains("{w}") && uriFormat.Contains("{s}") && uriFormat.Contains("{e}") && uriFormat.Contains("{n}"))
                {
                    tileSource = new BoundingBoxTileSource();
                }

                if (tileSource != null)
                {
                    tileSource.UriFormat = uriFormat;
                    return tileSource;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
