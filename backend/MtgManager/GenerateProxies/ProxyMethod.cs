using ConsoleUtils;
using DomainUtils;
using System.Linq;
using MtgManager.ChooseImages;
using ImageMagick;
using WindowsUtils;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using System.Security.Cryptography.Pkcs;

namespace MtgManager.GenerateProxies
{
    /// <summary>
    /// Handles generating proxy images of a specific set of card faces.
    /// </summary>
    public class ProxyMethod
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public static readonly IMagickColor<ushort>? LightColor = new MagickColor("#E0E0E0");

        public enum ProxyMethodName
        {
            Frame2015p1 = 1,
            Frame2015p2 = 2,
            Frame2003 = 3,
            Pre2003 = 4,
            Pre1998 = 5,
            Pre1994 = 6
        }

        public ProxyMethodName Name;

        /// <summary>
        /// Proxifies the given image. Changes are made to given ImageMagick argument.
        /// </summary>
        public Action<ProxifyArgs>? Proxify;

        private static Dictionary<ProxyMethodName, ProxyMethod>? _proxyMethods;

        /// <summary>
        /// Returns the proxy method suited for proxying the given card image.
        /// Returns null if no proxy methods match.
        /// </summary>
        public static ProxyMethod? GetProxyMethod(CardImage cardImg)
        {
            // don't include the multitude of fancy reprints, alternate art, and fancy frames in novelty sets.
            var validSetTypes = new string[]
            {
                // 'normal' cards
                "core", "expansion", "commander", "masters", "draft_innovation", "box",
                // semi fancy set of old card, often foiled. Still preferrable over oldest 1994 border cards.
                "from_the_vault"
            };
            // "The List", reprints of lots of old cards. Messes up old card figuring out proxy method by date.
            var excludeSetNames = new string[] { "plst" };

            if (!validSetTypes.Contains(cardImg.SetType)) return null;
            if (excludeSetNames.Contains(cardImg.Set)) return null;

            var proxyMethods = GetProxyMethods();

            if (cardImg.Frame == "2015")
            {
                var typeMatches = new string[] { "creature", "vehicle", "planeswalker" };
                var typeLine = cardImg.TypeLine?.Split(" ") ?? Array.Empty<string>();
                if (typeLine.Any(x => typeMatches.Any(y => x.ToLower() == y.ToLower()))) return proxyMethods[ProxyMethodName.Frame2015p1];
                return proxyMethods[ProxyMethodName.Frame2015p2];
            }

            if (cardImg.Frame == "2003")
            {
                return proxyMethods[ProxyMethodName.Frame2003];
            }

            if (cardImg.Frame == "2003")
            {
                return proxyMethods[ProxyMethodName.Frame2003];
            }

            if (cardImg.Frame == "1997" && cardImg.ReleasedAt >= new DateTime(1998, 6, 15))
            {
                return proxyMethods[ProxyMethodName.Pre2003];
            }

            if (cardImg.ReleasedAt >= new DateTime(1994, 11, 1) && cardImg.ReleasedAt < new DateTime(1998, 6, 15))
            {
                return proxyMethods[ProxyMethodName.Pre1998];
            }

            if (cardImg.ReleasedAt < new DateTime(1994, 11, 1))
            {
                return proxyMethods[ProxyMethodName.Pre1994];
            }

            return null;
        }

        /// <summary>
        /// Returns an array of all available methods, ranked in priority order
        /// </summary>
        /// <returns></returns>
        public static Dictionary<ProxyMethodName, ProxyMethod> GetProxyMethods()
        {
            if (_proxyMethods != null) return _proxyMethods;

            var res = new List<ProxyMethod>();

            var frame2015p1 = new ProxyMethod();
            frame2015p1.Name = ProxyMethodName.Frame2015p1;
            frame2015p1.Proxify = x =>
            {
                var img = x.MagickImage!;
                var fillArgs = new FillRectArgs()
                {
                    X = 430,
                    Y = 990,
                    X2 = 705,
                    Y2 = 1015,
                    Color = x.BorderColor
                };
                FillRect(img, fillArgs);
                var textArgs = new WriteTextArgs()
                {
                    Text = "Proxy - Not For Sale",
                    X = 695,
                    Y = 1005,
                    StrokeWidth = 0.7,
                    TextAlignment = TextAlignment.Right,
                    FontSize = 14,
                    Color = LightColor
                };
                WriteText(img, textArgs);
            };
            res.Add(frame2015p1);
            var frame2015p2 = new ProxyMethod();
            frame2015p2.Name = ProxyMethodName.Frame2015p2;
            frame2015p2.Proxify = x =>
            {
                var img = x.MagickImage!;
                var fillArgs = new FillRectArgs()
                {
                    X = 430,
                    Y = 970,
                    X2 = 705,
                    Y2 = 1020,
                    Color = x.BorderColor
                };
                FillRect(img, fillArgs);
                var textArgs = new WriteTextArgs()
                {
                    Text = "Proxy - Not For Sale",
                    X = 690,
                    Y = 985,
                    StrokeWidth = 0.7,
                    TextAlignment = TextAlignment.Right,
                    FontSize = 14,
                    Color = LightColor
                };
                WriteText(img, textArgs);
            };
            res.Add(frame2015p2);
            var frame2003 = new ProxyMethod();
            frame2003.Name = ProxyMethodName.Frame2003;
            frame2003.Proxify = x =>
            {
                var img = x.MagickImage!;
                var fillArgs = new FillRectArgs()
                {
                    X = 30,
                    Y = 985,
                    X2 = 1010,
                    Y2 = 1020,
                    Color = x.BorderColor
                };
                FillRect(img, fillArgs);
                var textArgs = new WriteTextArgs()
                {
                    Text = "Proxy - Not For Sale",
                    X = 695,
                    Y = 1005,
                    StrokeWidth = 0.7,
                    TextAlignment = TextAlignment.Right,
                    FontSize = 14,
                    Color = LightColor
                };
                WriteText(img, textArgs);
            };
            res.Add(frame2003);

            // set:usg 1998-10-12 bottom center (yawgmoth's will)
            // set:exo 1998-06-15 bottom center
            // set:sth 1998-03-02 bottom left (mogg maniac)
            // set:tmp 1997-10-14 bottom left

            var pre2003 = new ProxyMethod();
            pre2003.Name = ProxyMethodName.Pre2003;
            pre2003.Proxify = x =>
            {
                var img = x.MagickImage!;
                var fillArgs = new FillRectArgs()
                {
                    X = 170,
                    Y = 965,
                    X2 = 570,
                    Y2 = 988,
                    Color = x.BorderColor
                };
                FillRect(img, fillArgs);
                var textArgs = new WriteTextArgs()
                {
                    Text = "Proxy - Not For Sale",
                    X = (fillArgs.X + fillArgs.X2) / 2,
                    Y = 980,
                    StrokeWidth = 0.7,
                    TextAlignment = TextAlignment.Center,
                    FontSize = 14,
                    Color = LightColor
                };
                WriteText(img, textArgs);
            };
            res.Add(pre2003);

            var pre1998 = new ProxyMethod();
            pre1998.Name = ProxyMethodName.Pre1998;
            pre1998.Proxify = x =>
            {
                var img = x.MagickImage!;
                var fillArgs = new FillRectArgs()
                {
                    X = 60,
                    Y = 965,
                    X2 = 460,
                    Y2 = 988,
                    Color = x.BorderColor
                };
                FillRect(img, fillArgs);
                var textArgs = new WriteTextArgs()
                {
                    Text = "Proxy - Not For Sale",
                    X = 65,
                    Y = 982,
                    StrokeWidth = 0.7,
                    TextAlignment = TextAlignment.Left,
                    FontSize = 14,
                    Color = LightColor
                };
                WriteText(img, textArgs);
            };
            res.Add(pre1998);

            var pre1994 = new ProxyMethod();
            pre1994.Name = ProxyMethodName.Pre1994;
            pre1994.Proxify = x =>
            {
                var img = x.MagickImage!;
                var fillArgs = new FillRectArgs()
                {
                    X = 60,
                    Y = 945,
                    X2 = 530,
                    Y2 = 985,
                    Color = x.BorderColor
                };
                FillRect(img, fillArgs);
                var textArgs = new WriteTextArgs()
                {
                    Text = "Proxy - Not For Sale",
                    X = 75,
                    Y = 973,
                    StrokeWidth = 0.7,
                    FontSize = 20,
                    Color = LightColor
                };
                WriteText(img, textArgs);
            };
            res.Add(pre1994);

            _proxyMethods = res.ToDictionary(x => x.Name);
            return _proxyMethods;
        }

        /// <summary>
        /// Returns the preffered method to use for proxying. Returns null if equal.
        /// </summary>
        public static ProxyMethod? GetBest(ProxyMethod x, ProxyMethod y)
        {
            if ((int)x.Name < (int)y.Name) return x;
            if ((int)y.Name < (int)x.Name) return y;

            return null;
        }

        public class FillRectArgs
        {
            public int X;
            public int Y;
            public int X2;
            public int Y2;
            public IMagickColor<ushort>? Color;
        }

        /// <summary>
        /// Fills a rect with the given color onto the given image.
        /// </summary>
        public static void FillRect(MagickImage image, FillRectArgs args)
        {
            if (args.Color == null) throw new ArgumentNullException(nameof(args.Color));

            var rect = new DrawableRectangle(args.X, args.Y, args.X2, args.Y2);
            var strokeColor = new DrawableStrokeColor(args.Color);
            var fillColor = new DrawableFillColor(args.Color);
            image.Draw(strokeColor, fillColor, rect);
        }

        public class WriteTextArgs
        {
            public string? Text;
            public int X;
            public int Y;
            public double StrokeWidth = 1;
            public TextAlignment TextAlignment = TextAlignment.Undefined;
            public double FontSize = 10;
            public IMagickColor<ushort>? Color;
        }

        /// <summary>
        /// Fills a rect with the given color onto the given image.
        /// </summary>
        public static void WriteText(MagickImage image, WriteTextArgs args)
        {
            if (args.Text == null) throw new ArgumentNullException(nameof(args.Text));
            if (args.Color == null) throw new ArgumentNullException(nameof(args.Color));

            var text = new DrawableText(args.X, args.Y, args.Text);
            var scolor = new DrawableStrokeColor(args.Color);
            var fcolor = new DrawableFillColor(args.Color);
            var strokeWidth = new DrawableStrokeWidth(args.StrokeWidth);
            var alignment = new DrawableTextAlignment(args.TextAlignment);
            var fontSize = new DrawableFontPointSize(args.FontSize);
            image.Draw(scolor, fcolor, strokeWidth, alignment, fontSize, text);
        }
    }

    public class ProxifyArgs
    {
        public CardImage? CardImage { get; set; }
        public MagickImage? MagickImage { get; set; }
        public IMagickColor<ushort>? BorderColor { get; set; }
    }
}
