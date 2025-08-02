using DomainUtils;
using MtgManager.ImportMoxfield;

namespace MtgManager.ComputeDiff
{
    internal class CardList : XmlSerial
    {
        public enum BasicLandNames
        {
            Plains,
            Island,
            Swamp,
            Mountain,
            Forest
        }

        /// <summary>
        /// name of card -> count of card in list.
        /// </summary>
        public Dictionary<string, int> Cards = new();

        public CardList() {}

        public void AddCard(MoxCard card)
        {
            AddCard(card.Name);
        }

        public void AddCard(string name)
        {
            if (!Cards.ContainsKey(name)) Cards[name] = 0;
            Cards[name]++;
        }

        public void AddCardList(CardList other)
        {
            foreach (var cardCnt in other.Cards)
            {
                for (var i = 0; i < cardCnt.Value; i++)
                {
                    AddCard(cardCnt.Key);
                }
            }
        }

        /// <summary>
        /// Returns true if no cards are contained in this list
        /// </summary>
        public bool IsEmpty()
        {
            return Cards.Count == 0;
        }

        /// <summary>
        /// Returns a new card list representing the intersection of cards between both lists.
        /// </summary>
        public CardList Intersect(CardList list)
        {
            CardList res = new();

            foreach (var key in Cards.Keys.Where(x => list.Cards.ContainsKey(x)))
            {
                res.Cards[key] = Math.Min(Cards[key], list.Cards[key]);
            }

            return res;
        }

        /// <summary>
        /// Returns a new card list representing this list minus all cards in given list.
        /// </summary>
        public CardList Subtract(CardList list)
        {
            CardList res = new();

            foreach (var key in Cards.Keys)
            {
                var minuen = Cards[key];
                var subtrahend = list.Cards.ContainsKey(key) ? list.Cards[key] : 0;
                var resCnt = minuen - subtrahend;

                if (resCnt > 0)
                {
                    res.Cards[key] = resCnt;
                }
            }

            return res;
        }

        /// <summary>
        /// Returns a list of 999 of each basic land type
        /// </summary>
        public static CardList GetBasicLandPile()
        {
            var res = new CardList();
            foreach (var landName in GetBasicLandNames())
            {
                for (var i = 0; i < 999; i++)
                {
                    res.AddCard(landName);
                }
            }
            return res;
        }

        /// <summary>
        /// Returns an array of all BasicLandNames as strings
        /// </summary>
        public static string[] GetBasicLandNames()
        {
            return Enum.GetValues(typeof(BasicLandNames))
                .Cast<BasicLandNames>()
                .Select(x => {
                    var name = Enum.GetName(typeof(BasicLandNames), x);
                    if (name == null) throw new Exception("null value encountered");
                    return name;
                })
                .ToArray();
        }
    }
}
