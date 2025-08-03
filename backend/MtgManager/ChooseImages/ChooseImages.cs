using ConsoleUtils;
using DomainUtils;
using ExtendedXmlSerializer;
using HttpUtils;
using MtgManager.ComputeDiff;
using MtgManager.GenerateProxies;
using System.Collections.Generic;
using System.Linq;
using WindowsUtils;
using static MtgManager.ChooseImages.ChooseImagesStep;

namespace MtgManager.ChooseImages
{
    /// <summary>
    /// Takes the diff output of the ComputeDiff step and downloads images of cards that need to be ordered.
    /// </summary>
    public class ChooseImagesStep
    {
        public enum ImageChoiceStrategy
        {
            ChooseBest,
            AllProxiable,
            AllHighres,
            AllImages
        }

        public const string DefaultSavePath = @"D:\WindowsTools\MtgManager\chooseImages";

        public const int MpcMaxOrderSize = 612;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public static void RunStep()
        {
            var inst = new ChooseImagesStep();
            inst.ChooseImages();
        }

        private readonly HttpClient httpClient = GlobalHttpClient.GetClient();
        private string SavePath;
        private int OrderID = 0;

        /// <summary>
        /// If set, debug print more messages.
        /// </summary>
        public bool Debug = false;

        public ChooseImagesStep(string savePath = DefaultSavePath)
        {
            SavePath = savePath;
        }

        public void ChooseImages()
        {
            var cardListMoves = XmlSerial.FromXmlArrayFile<CardListMove>(ComputeDiffStep.DiffPath);
            var orderList = new CardList();
            foreach (var move in cardListMoves.Where(x => x.Source == ComputeDiffStep.OrderSourceName))
            {
                orderList.AddCardList(move.CardList);
            }

            // ask user how much choice to give them
            var imageChoice = ConsoleUtil.AskUserEnum<ImageChoiceStrategy>("How many images to choose from?");

            // clear out directory
            FileSystemUtil.ClearDirectory(SavePath);

            using var client = new ScryfallApiClient();
            var cards = orderList.Cards.SelectMany(kv => Enumerable.Repeat(kv.Key, kv.Value)).ToArray();
            if (cards.Length > MpcMaxOrderSize) {
                var cont = ConsoleUtil.AskUserYesNo($"card count exceeds maximum MPC order size (orderSize={cards.Length}, max={MpcMaxOrderSize}). Are you sure you want to continue?");
                if (!cont) return;
            }
            FetchCards(client, imageChoice, cards);
        }

        public struct CardOrder {
            /// <summary>
            /// ex. "Negate"
            /// </summary>
            public string CardName;
            /// <summary>
            /// ex. "M20"
            /// </summary>
            public string? SetCode;
        }

        public void FetchCards(ScryfallApiClient client, ImageChoiceStrategy imageChoice, string[] cardNames)
        {
            var orders = cardNames.Select(x => new CardOrder { CardName = x }).ToArray();
            FetchCards(client, imageChoice, orders);
        }

        public void FetchCards(ScryfallApiClient client, ImageChoiceStrategy imageChoice, CardOrder[] cardOrders)
        {
            var cur = 0;
            var total = cardOrders.Length;
            // print enough digits to match total
            var format = $"D{total.ToString().Length}";
            foreach (var order in cardOrders)
            {
                cur++;
                log.Info($"({cur.ToString(format)}/{total}) {order.CardName}");
                FetchCard(client, imageChoice, order);
            }
        }

        private void FetchCard(ScryfallApiClient client, ImageChoiceStrategy imageChoice, CardOrder cardOrder)
        {
            var args = ScryfallSearchArgs.GetDefault();
            args.ExactName = cardOrder.CardName;
            if (!string.IsNullOrEmpty(cardOrder.SetCode)) args.SetCode = cardOrder.SetCode;
            args.Unique = ScryfallSearchArgs.ScryfallUnique.Prints;
            args.Lang = ScryfallSearchArgs.ScryfallLang.English;

            var cards = client.CardsSearch(args);
            if (cards.TotalCards == 0 || cards.Data == null)
            {
                throw new Exception($"No cards returned searching for: {cardOrder.CardName}");
            }

            // card face name => best image choice
            var bestImgs = new Dictionary<string, CardImageOption>();
            var saveCards = new List<CardImage>();
            OrderID++;
            foreach (var card in cards.Data)
            {
                if (card == null) throw new Exception("Null card encountered");

                var cardImgs = new List<CardImage>();
                // single face cards have image uris in base card object. multiface cards are nested into child CardFaces children.
                var multiFace = card.ImageUris == null;
                if (multiFace)
                {
                    foreach (var face in card.CardFaces!)
                    {
                        var cardImg = new CardImage();
                        cardImg.OrderID = OrderID;

                        cardImg.Name = face.Name;
                        cardImg.TypeLine = face.TypeLine;
                        cardImg.ImageUris = face.ImageUris;

                        cardImg.Set = card.Set;
                        cardImg.SetType = card.SetType;
                        cardImg.BorderColor = card.BorderColor;
                        cardImg.Frame = card.Frame;
                        cardImg.ReleasedAt = card.ReleasedAt;
                        cardImg.FrameEffects = card.FrameEffects;

                        cardImg.FileExtension = "png";
                        cardImgs.Add(cardImg);
                    }
                }
                else
                {
                    var cardImg = new CardImage();
                    cardImg.OrderID = OrderID;

                    cardImg.Name = card.Name;
                    cardImg.Set = card.Set;
                    cardImg.SetType = card.SetType;
                    cardImg.BorderColor = card.BorderColor;
                    cardImg.Frame = card.Frame;
                    cardImg.ReleasedAt = card.ReleasedAt;
                    cardImg.FrameEffects = card.FrameEffects;
                    cardImg.TypeLine = card.TypeLine;
                    cardImg.ImageUris = card.ImageUris;

                    cardImg.FileExtension = "png";
                    cardImgs.Add(cardImg);
                }

                foreach (var cardImg in cardImgs)
                {
                    if (Debug) log.Info($"processing image: Name={cardImg.Name} Set={cardImg.Set} SetType={cardImg.SetType} Frame={cardImg.Frame} ReleasedAt={cardImg.ReleasedAt}");
                    switch (imageChoice)
                    {
                        case ImageChoiceStrategy.ChooseBest:
                            var method = ProxyMethod.GetProxyMethod(cardImg);
                            if (method == null) break;
                            // choose most preferred proxy method, tie break by earliest release
                            var cur = new CardImageOption();
                            cur.ProxyMethod = method;
                            cur.CardImg = cardImg;
                            cur.Card = card!;

                            var name = cardImg!.Name!;
                            if (!bestImgs.ContainsKey(name))
                            {
                                bestImgs[name] = cur;
                            }
                            else
                            {
                                var best = GetBest(bestImgs[name], cur);
                                if (best != null) bestImgs[name] = best.Value;
                            }
                            if (Debug) log.Info($"Chose as best: Set={bestImgs[name].CardImg.Set} ProxyMethod={bestImgs[name].ProxyMethod.Name}");
                            break;
                        case ImageChoiceStrategy.AllProxiable:
                            if (ProxyMethod.GetProxyMethod(cardImg) != null) saveCards.Add(cardImg);
                            break;
                        case ImageChoiceStrategy.AllHighres:
                            if (card?.HighresImage ?? false) saveCards.Add(cardImg);
                            break;
                        case ImageChoiceStrategy.AllImages:
                            saveCards.Add(cardImg);
                            break;
                        default:
                            throw new Exception("Unhandled enum: " + imageChoice);
                    }
                }
            }
            if (imageChoice == ImageChoiceStrategy.ChooseBest)
            {
                foreach (var kv in bestImgs)
                {
                    saveCards.Add(kv.Value.CardImg);
                }
            }

            if (!saveCards.Any()) throw new Exception($"No card image found for {cardOrder.CardName}");

            foreach (CardImage cardImg in saveCards)
            {
                var imgUrl = cardImg?.ImageUris?.Png;
                if (imgUrl == null) throw new Exception($"Unable to find imgUrl for card: {cardImg?.Name} ({cardImg?.Set})");
                httpClient.SaveToFile(
                    imgUrl,
                    Path.Combine(
                        SavePath,
                        cardImg!.ToImgFileName()
                    )
                );
            }
        }

        private struct CardImageOption
        {
            public ProxyMethod ProxyMethod;
            public CardImage CardImg;
            public ScryfallCard Card;
        }

        /// <summary>
        /// between two given card image options, return the preferred one to proxy
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static CardImageOption? GetBest(CardImageOption x, CardImageOption y)
        {
            // I don't want to put in the work to handle white borders
            if (x.CardImg.BorderColor == "black" && y.CardImg.BorderColor != "black") return x;
            if (x.CardImg.BorderColor != "black" && y.CardImg.BorderColor == "black") return y;

            // don't proxy the oldest cards if we can avoid it, it looks ugly
            if (x.ProxyMethod.Name != ProxyMethod.ProxyMethodName.Pre1994 && y.ProxyMethod.Name == ProxyMethod.ProxyMethodName.Pre1994) return x;
            if (x.ProxyMethod.Name == ProxyMethod.ProxyMethodName.Pre1994 && y.ProxyMethod.Name != ProxyMethod.ProxyMethodName.Pre1994) return y;

            // foils don't scan well
            if (x.Card.NonFoil!.Value && !y.Card.NonFoil!.Value) return x;
            if (!x.Card.NonFoil!.Value && y.Card.NonFoil!.Value) return y;

            // get by most recent "printing style"
            var bestMethod = ProxyMethod.GetBest(x.ProxyMethod, y.ProxyMethod);
            if (bestMethod != null) return bestMethod == x.ProxyMethod ? x : y;

            // oldest 'modern style' print preferred, I don't want program returning new cards often on commonly reprinted cards
            if (x.Card!.ReleasedAt < y.Card!.ReleasedAt) return x;
            if (y.Card!.ReleasedAt < x.Card!.ReleasedAt) return y;

            return null;
        }
    }
}
