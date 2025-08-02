using MtgManager.ChooseImages;
using MtgManager.GenerateProxies;
using System.Diagnostics;
using WindowsUtils;

namespace MtgManager.Test
{
    public class MtgProxyTest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private const string TestDir = @"D:\WindowsTools\MtgManager\test";

        public static void RunTest()
        {
            var testCardsFile = Path.Combine(TestDir, "TestCards.txt");
            var chooseImgsDir = Path.Combine(TestDir, "chooseImages");
            Directory.CreateDirectory(chooseImgsDir);
            var proxiesDir = Path.Combine(TestDir, "proxies");
            Directory.CreateDirectory(proxiesDir);

            using var fileStream = File.OpenRead(testCardsFile);
            using var streamReader = new StreamReader(fileStream);
            using var client = new ScryfallApiClient();
            var chooseImagesStep = new ChooseImagesStep(chooseImgsDir);
            chooseImagesStep.Debug = true;
            var generateProxiesStep = new GenerateProxiesStep(chooseImgsDir, proxiesDir);
            generateProxiesStep.Debug = true;

            string? line;
            var list = new List<ChooseImagesStep.CardOrder>();
            while ((line = streamReader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                line = line.Trim();
                var order = new ChooseImagesStep.CardOrder();
                if (line.Contains("[") && line.Contains("]"))
                {
                    // ex. "Black Lotus [LBE]"
                    var parts = line.Split(" [");
                    order.CardName = parts[0];
                    order.SetCode = parts[1].Replace("]", "");
                }
                else
                {
                    order.CardName = line;
                }
                list.Add(order);
            }

            log.Info("Fetching best images");
            FileSystemUtil.ClearDirectory(chooseImgsDir);
            chooseImagesStep.FetchCards(client, ChooseImagesStep.ImageChoiceStrategy.ChooseBest, list.ToArray());
            log.Info("Proxying images");
            generateProxiesStep.ProxyImages();
            log.Info($"done with test! see results at {TestDir}");
        }
    }
}
