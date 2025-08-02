using ConsoleUtils;
using DomainUtils;
using MtgManager.ComputeDiff;
using System.Xml.Linq;
using WindowsUtils;

namespace MtgManager.ImportMoxfield
{
    internal class MoxDeckList : XmlSerial
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public enum DeckListType
        {
            BrewingNew,
            BrewingOwned,
            Collection,
            DecksCurrent
        }

        /// <summary>
        /// Name of DeckList
        /// </summary>
        public string Name = string.Empty;

        public DeckListType? DeckType = null;

        public MoxCard[] MainBoard = Array.Empty<MoxCard>();

        public MoxCard[] SideBoard = Array.Empty<MoxCard>();

        public MoxCard[] Considering = Array.Empty<MoxCard>();

        public MoxDeckList() { }

        public static MoxDeckList ImportFromCommandLine()
        {
            while (true)
            {
                var deckListType = ConsoleUtil.AskUserEnum<DeckListType>("What kind of decklist to import?");
                if (deckListType == DeckListType.Collection)
                {
                    log.Info(@"Collection moved to MoxField Collection Manager! Please export csv from https://www.moxfield.com/collection and drop into MtgManager\decklists");
                    ConsoleUtil.AskUser("Hit enter once file is present.");

                    var files = Directory.GetFiles(ImportMoxfieldStep.GetPath(DeckListType.Collection))
                        .Where(x => x.ToLower().EndsWith(".csv"))
                        .ToArray();
                    if (files.Length > 2)
                    {
                        log.Warn("More than 1 csv file found! Please provide 1 csv file and retry.");
                        continue;
                    }
                    if (files.Length == 0)
                    {
                        log.Warn("No csv files found! Please provide 1 csv file and retry.");
                        continue;
                    }

                    var filename = files.First();

                    var cards = MoxCard.ParseCollectionCsv(filename);
                    if (cards.Length == 0)
                    {
                        log.Warn("csv parsing failed!");
                        continue;
                    }
                    var res = new MoxDeckList
                    {
                        Name = "Collection",
                        DeckType = deckListType,
                        MainBoard = cards
                    };
                    return res;
                }
                else
                {
                    var name = ConsoleUtil.AskUser("Name of desklist");

                    var mainboardStr = ConsoleUtil.ReadClipboard("Read mainboard from clipboard.");
                    var mainboard = MoxCard.ParseList(mainboardStr);
                    var sideboardStr = ConsoleUtil.ReadClipboard("Read sideboard from clipboard, or just re-hit enter to skip.");
                    var sideboard = Array.Empty<MoxCard>();
                    if (!string.IsNullOrEmpty(sideboardStr) && sideboardStr != mainboardStr)
                    {
                        sideboard = MoxCard.ParseList(sideboardStr);
                    }
                    var consideringStr = ConsoleUtil.ReadClipboard("Read considering from clipboard, or just re-hit enter to skip.");
                    var considering = Array.Empty<MoxCard>();
                    if (!string.IsNullOrEmpty(consideringStr) && consideringStr != sideboardStr)
                    {
                        considering = MoxCard.ParseList(consideringStr);
                    }

                    var res = new MoxDeckList
                    {
                        Name = name,
                        DeckType = deckListType,
                        MainBoard = mainboard,
                        SideBoard = sideboard,
                        Considering = considering
                    };
                    if (res.UserValidateCommander())
                    {
                        return res;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the given deck is a valid commander setup for v1 list setup, or if user confirms every error is okay.
        /// </summary>
        private bool UserValidateCommander()
        {
            var cnt = MainBoard.Length + SideBoard.Length;
            if (cnt != 100 && !ConsoleUtil.AskUserYesNo($"Decklist has {cnt} cards between mainboard and sideboard, expected 100 (sideboard should just have commander). Continue with error?"))
            {
                return false;
            }
            // TODO WESD Check 1 of each copy, excluding basics
            // TODO WESD Check balanced 'out' and 'in' tags in considering vs mainboard
            return true;
        }

        /// <summary>
        /// Returns all cards in list
        /// </summary>
        public CardList GetCards()
        {
            var res = new CardList();
            foreach (var card in MainBoard.Concat(SideBoard).Concat(Considering))
            {
                res.AddCard(card);
            }
            return res;
        }

        /// <summary>
        /// Returns all card that have the specified tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public CardList GetCardsByTag(string tag)
        {
            var res = new CardList();
            foreach (var card in MainBoard.Concat(SideBoard).Concat(Considering))
            {
                if (card.Tags.Contains(tag, StringComparer.InvariantCultureIgnoreCase))
                {
                    res.AddCard(card);
                }
            }
            return res;
        }
    }
}
