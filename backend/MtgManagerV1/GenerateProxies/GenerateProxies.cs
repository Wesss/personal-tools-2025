using ConsoleUtils;
using DomainUtils;
using System.Linq;
using MtgManager.ChooseImages;
using ImageMagick;
using WindowsUtils;
using System.Drawing;

namespace MtgManager.GenerateProxies
{
    /// <summary>
    /// Takes card images from the ChooseImages step and turns them into proxy-able images (replace copyright with proxy text, etc).
    /// </summary>
    public class GenerateProxiesStep
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private static bool init = false;

        public const string DefaultSavePath = @"D:\WindowsTools\MtgManager\proxies";

        /// <summary>
        /// If set, debug print more messages.
        /// </summary>
        public bool Debug = false;

        public static void Init()
        {
            if (init) return;
            MagickNET.Initialize();
            init = true;
        }

        public static void RunStep()
        {
            var inst = new GenerateProxiesStep();
            inst.ProxyImages();
        }

        private string LoadPath;
        private string SavePath;

        public GenerateProxiesStep(
            string loadPath = ChooseImagesStep.DefaultSavePath,
            string savePath = DefaultSavePath
        )
        {
            LoadPath = loadPath;
            SavePath = savePath;
        }

        public void ProxyImages()
        {
            if (!FileSystemUtil.DirectoryIsEmpty(SavePath))
            {
                log.Info("clearing out proxies folder");
                FileSystemUtil.ClearDirectory(SavePath);
            }
            Directory.CreateDirectory(Path.Combine(SavePath, "front"));
            Directory.CreateDirectory(Path.Combine(SavePath, "back"));
            var imgGroups = Directory.EnumerateFiles(LoadPath)
                .Select(x => CardImage.FromImgFile(x))
                .GroupBy(x => x.OrderID)
                .ToArray();
            var cur = 0;
            var total = imgGroups.Length;
            // print enough digits to match total
            var format = $"D{total.ToString().Length}";
            foreach (var orderGroup in imgGroups)
            {
                cur++;
                log.Info($"({cur.ToString(format)}/{total}) {string.Join(" // ", orderGroup.Select(x => x.Name))}");
                var front = true;
                foreach (var cardImg in orderGroup)
                {
                    var filename = Path.GetFileName(cardImg.FilePath!);
                    var destPath = Path.Combine(SavePath, front ? "front" : "back", filename);
                    MakeProxy(cardImg, destPath);
                    front = false;
                }
                if (orderGroup.Count() == 1)
                {
                    var destPath = Path.Combine(SavePath, "back", $"{orderGroup.Key.ToString("D4")}--back.png");
                    using var proxyFront = GetProxyFace(orderGroup.Key);
                    proxyFront.Write(destPath);
                }
            }
        }

        /// <summary>
        /// Modifies images at given file path to be suitable to proxy at MPC.
        /// </summary>
        /// <remarks>
        /// Test cases: https://docs.google.com/spreadsheets/d/1CVL4vdnl36nPhKxswlTUEnCz7jEcebvQtZoYqVTwi3Q/edit
        /// </remarks>
        private void MakeProxy(CardImage cardImg, string destPath)
        {
            var typeLine = cardImg.TypeLine!.Split(" ");
            var file = cardImg.FilePath!;
            // sample bottom border, most full art cards don't care about bottom frame too much.
            var borderColor = GetAverageColor(file, 50, 1020, 700, 1030);

            if (Debug) log.Info($"cardImg Frame={cardImg.Frame} Set={cardImg.Set} SetType={cardImg.SetType} ReleasedAt={cardImg.ReleasedAt}");
            var proxyMethod = ProxyMethod.GetProxyMethod(cardImg);
            if (proxyMethod == null) throw new Exception($"Unable to find proxy method for card: {cardImg.Name} ({cardImg.Set})");
            if (Debug) log.Info("Using proxyMethod: " + proxyMethod.Name);

            using var magickImage = new MagickImage(file);
            var args = new ProxifyArgs();
            args.CardImage = cardImg;
            args.MagickImage = magickImage;
            args.BorderColor = borderColor;
            proxyMethod.Proxify!(args);

            // add border to account for MPC bleed area
            magickImage.BorderColor = borderColor;
            magickImage.Border(38);

            UniqueMark(magickImage, cardImg.OrderID);

            magickImage.Write(destPath);
        }

        /// <summary>
        /// Returns blank card face with text marking the card as a not for sale proxy.
        /// </summary>
        private static MagickImage GetProxyFace(int id)
        {
            var width = 870;
            var height = 1200;
            var magickImage = new MagickImage(new MagickColor("#101010"), width, height);
            var textArgs = new ProxyMethod.WriteTextArgs()
            {
                Text = "Proxy",
                X = width / 2,
                Y = height / 3,
                StrokeWidth = 1,
                TextAlignment = TextAlignment.Center,
                FontSize = 96,
                Color = ProxyMethod.LightColor
            };
            ProxyMethod.WriteText(magickImage, textArgs);
            textArgs.Text = "Not For Sale";
            textArgs.Y = (height / 3) + 70;
            textArgs.FontSize = 48;
            ProxyMethod.WriteText(magickImage, textArgs);

            UniqueMark(magickImage, id);

            return magickImage;
        }

        /// <summary>
        /// MPC ignores duplicate images. Ensure each is unique by writing a unique pixel in top left (will get cropped)
        /// </summary>
        private static void UniqueMark(MagickImage magickImage, int id)
        {
            // generate a unique color based on id
            if (id > 999999) throw new Exception("orders of 1,000,000 or more not supported; unable to guarantee image uniqueness");
            var color = new MagickColor($"#{id.ToString("D6")}");

            var fillArgs = new ProxyMethod.FillRectArgs()
            {
                X = 0,
                Y = 0,
                X2 = 5,
                Y2 = 5,
                Color = color
            };
            ProxyMethod.FillRect(magickImage, fillArgs);
        }

        private static IMagickColor<ushort> GetAverageColor(string imgPath, int x1, int y1, int x2, int y2)
        {
            using var image = new MagickImage(imgPath);
            image.Crop(new MagickGeometry(x1, y1, x2 - x1, y2 - y1));
            // resize to a single pixel, averaging all RGB values
            image.Resize(new MagickGeometry(1, 1));
            var pixels = image.GetPixels();
            if (pixels.Count() != 1)
            {
                throw new Exception("Image did not average to a single pixel");
            }
            var pixel = pixels.First();
            var res = pixel.ToColor();
            if (res == null)
            {
                throw new Exception("Unable to extract color out of pixel");
            }
            return res;
        }
    }
}
