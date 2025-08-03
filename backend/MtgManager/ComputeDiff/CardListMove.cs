using DomainUtils;

namespace MtgManager.ComputeDiff
{
    internal class CardListMove : XmlSerial
    {
        /// <summary>
        /// Where cards are moving from. May be name of decklist or some other arbitrary identifier.
        /// </summary>
        public string? Source;

        /// <summary>
        /// Where cards are moving to. May be name of decklist or some other arbitrary identifier.
        /// </summary>
        public string? Destination;

        /// <summary>
        /// List of cards to move.
        /// </summary>
        public CardList CardList = new();

        public CardListMove() {}

        /// <summary>
        /// returns a new CarListMove with every cards that matches between the given source and given dest,
        /// using the source/dest name given.
        /// 
        /// Matched cards are removed from the given source/dest.
        /// </summary>
        public static CardListMove Match(CardListMove source, CardListMove dest)
        {
            var intersect = source.CardList.Intersect(dest.CardList);

            source.CardList = source.CardList.Subtract(intersect);
            dest.CardList = dest.CardList.Subtract(intersect);
            return new CardListMove()
            {
                Source = source.Source,
                Destination = dest.Destination,
                CardList = intersect
            };
        }
    }
}
