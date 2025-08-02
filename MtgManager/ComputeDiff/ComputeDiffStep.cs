using DomainUtils;
using MtgManager.ChooseImages;
using MtgManager.ImportMoxfield;
using System.IO;

namespace MtgManager.ComputeDiff
{
    public class ComputeDiffStep
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public const string DiffPath = @"D:\WindowsTools\MtgManager\diff\diff.xml";

        public const string DiscardDestName = "discard";
        public const string OrderSourceName = "order";
        public const string BasicLandSourceName = "basic lands";

        public static void RunStep()
        {
            var savedDecks = ImportMoxfieldStep.ListDecks();
            var brewingNew = new List<MoxDeckList>();
            var brewingOwned = new List<MoxDeckList>();
            var collections = new List<MoxDeckList>();
            var decksCurrent = new List<MoxDeckList>();
            foreach (var kv in savedDecks)
            {
                var decktype = kv.Key;
                List<MoxDeckList> list = decktype switch
                {
                    MoxDeckList.DeckListType.BrewingNew => brewingNew,
                    MoxDeckList.DeckListType.BrewingOwned => brewingOwned,
                    MoxDeckList.DeckListType.Collection => collections,
                    MoxDeckList.DeckListType.DecksCurrent => decksCurrent,
                    _ => throw new Exception("Unhandled DeckListType: " + decktype),
                };
                foreach (var name in kv.Value)
                {
                    list.Add(ImportMoxfieldStep.LoadDeck(decktype, name));
                }
            }

            // final set of cards needed to be moved around or ordered.
            var finalMoves = new List<CardListMove>();
            // set of cards needed to complete lists, but source hasn't been found yet.
            var dests = new List<CardListMove>();
            // set of cards needed to be removed from lists, but dest hasn't been found yet.
            var sources = new List<CardListMove>();

            // take all cards in brewing new to make new decks
            foreach (var newDeck in brewingNew)
            {
                var newNeeded = new CardListMove()
                {
                    Destination = newDeck.Name,
                    CardList = newDeck.GetCards()
                };
                log.Info($"Found new deck: {newDeck.Name}");
                dests.Add(newNeeded);
            }

            // take all brewingOwned and decksCurrent pairs. Items without pairs are ignored.
            var ownedPairs = new List<Tuple<MoxDeckList, MoxDeckList>>();
            foreach (var brew in brewingOwned)
            {
                var name = brew.Name.ToLower();
                var curDeck = decksCurrent.FirstOrDefault(x => x.Name.ToLower() == name);
                if (curDeck == null)
                {
                    log.Error($"Found BrewingOwned deck but no matching current deck, skipping. deck: {name}");
                }
                else
                {
                    log.Info($"Found brewing owned deck: {name}");
                    ownedPairs.Add(Tuple.Create(curDeck, brew));
                }
            }
            foreach (var pair in ownedPairs)
            {
                var curDeck = pair.Item1;
                var brewDeck = pair.Item2;

                // take out #bad-proxy cards in current list and put into discard
                var badProxies = curDeck.GetCardsByTag("bad-proxy");
                var listToDiscard = new CardListMove()
                {
                    Source = curDeck.Name,
                    Destination = DiscardDestName,
                    CardList = badProxies
                };
                finalMoves.Add(listToDiscard);
                var remainingCards = curDeck.GetCards().Subtract(badProxies);

                // diff decks to get sets to add/remove into current deck to have it match brewing deck.
                var brewCards = brewDeck.GetCards();
                var dest = new CardListMove()
                {
                    Destination = curDeck.Name,
                    CardList = brewCards.Subtract(remainingCards)
                };
                dests.Add(dest);
                var source = new CardListMove()
                {
                    Source = curDeck.Name,
                    CardList = remainingCards.Subtract(brewCards)
                };
                sources.Add(source);
            }

            if (collections.Count != 1) throw new Exception($"collection not found.");
            var collection = collections.First();
            var collectionSource = new CardListMove()
            {
                Source = collection.Name,
                CardList = collection.GetCards()
            };

            // for each needed card, pull from potential sources in priority order
            foreach (var dest in dests)
            {
                // check if coming out of another deck
                foreach (var source in sources)
                {
                    var subMatch = CardListMove.Match(source, dest);
                    if (!subMatch.CardList.IsEmpty())
                    {
                        finalMoves.Add(subMatch);
                    }
                }

                // have basic lands come from basic land source
                var lands = new CardListMove()
                {
                    Source = BasicLandSourceName,
                    CardList = CardList.GetBasicLandPile()
                };
                var landMatch = CardListMove.Match(lands, dest);
                if (!landMatch.CardList.IsEmpty())
                {
                    finalMoves.Add(landMatch);
                }

                // check if in collection
                var collectionMatch = CardListMove.Match(collectionSource, dest);
                if (!collectionMatch.CardList.IsEmpty())
                {
                    finalMoves.Add(collectionMatch);
                }

                // remaining cards need to be ordered
                if (!dest.CardList.IsEmpty())
                {
                    dest.Source = OrderSourceName;
                    finalMoves.Add(dest);
                }
            }

            // handle remaining cards
            foreach (var source in sources)
            {
                // basic lands go back to basic land pile
                var lands = new CardListMove()
                {
                    Destination = BasicLandSourceName,
                    CardList = CardList.GetBasicLandPile()
                };
                var landMatch = CardListMove.Match(source, lands);
                if (!lands.CardList.IsEmpty())
                {
                    finalMoves.Add(landMatch);
                }

                // remaining cards in outgoing from decks go to collection
                if (!source.CardList.IsEmpty())
                {
                    source.Destination = collection.Name;
                    finalMoves.Add(source);
                }
            }

            // included hardcoded list of cards; often to top off order.
            var topoff = GetTopOffOrder(collection.Name);
            if (topoff != null)
            {
                log.Info($"topoff cards found! adding {topoff.CardList.Cards.Count} cards to order");
                finalMoves.Add(topoff);
            }

            XmlSerial.ToXmlArrayFile(finalMoves.ToArray(), DiffPath);
        }

        private static CardListMove? GetTopOffOrder(string collectionName) {
            var file = Path.Combine(ImportMoxfieldStep.DeckListPath, "topoff.txt");
            if (!File.Exists(file))
            {
                log.Info($"topoff file not found at {file}, skipping top off order");
                return null;
            }

            using var fileStream = File.OpenRead(file);
            using var streamReader = new StreamReader(fileStream);
            string? line;
            var list = new CardList();
            while ((line = streamReader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                line = line.Trim();
                list.AddCard(line);
            }
            if (list.Cards.Count == 0)
            {
                log.Info($"topoff file empty, skipping top off order");
                return null;
            }

            var res = new CardListMove();
            res.Source = OrderSourceName;
            res.Destination = collectionName;
            res.CardList = list;
            return res;
        }
    }
}
