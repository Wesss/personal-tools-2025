using ConsoleUtils;
using DomainUtils;

namespace MtgManager.ImportMoxfield
{
    public class ImportMoxfieldStep
    {
        public const string DeckListPath = @"D:\WindowsTools\MtgManager\decklists";

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public enum DecklistAction
        {
            Upsert,
            List,
            Delete
        }

        public static void RunStep()
        {
            var loop = true;
            while (loop)
            {
                var action = ConsoleUtil.AskUserEnum<DecklistAction>("What would you like to do?");
                switch (action)
                {
                    case DecklistAction.Upsert:
                        Upsert();
                        break;
                    case DecklistAction.List:
                        List();
                        break;
                    case DecklistAction.Delete:
                        Delete();
                        break;
                    default:
                        throw new Exception("Unhandled DecklistAction: " + action);
                }

                loop = ConsoleUtil.AskUserYesNo("Would you like to continue editing decklists?");
            }
        }

        public static void Upsert()
        {
            MoxDeckList decklist = MoxDeckList.ImportFromCommandLine();
            var path = GetPath(decklist);
            decklist.ToXmlFile(path);
        }

        public static void List()
        {
            Console.WriteLine();
            var decklists = ListDecks();
            foreach (var kv in decklists)
            {
                var decklistType = kv.Key;
                var decks = kv.Value;
                Console.WriteLine("----------------");
                Console.WriteLine(decklistType);
                Console.WriteLine("----------------");
                var path = GetPath(decklistType);
                if (decks.Count > 0)
                {
                    foreach (var deckname in decks)
                    {
                        Console.WriteLine(deckname);
                    }
                }
                else
                {
                    Console.WriteLine("None");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public static void Delete()
        {
            throw new NotImplementedException("TODO implement Delete");
        }

        /// <summary>
        /// Dictionary<deck type, List<deck name>>
        /// </summary>
        internal static Dictionary<MoxDeckList.DeckListType, List<string>> ListDecks()
        {
            var res = new Dictionary<MoxDeckList.DeckListType, List<string>>();
            foreach (MoxDeckList.DeckListType decklistType in Enum.GetValues(typeof(MoxDeckList.DeckListType)))
            {
                var list = new List<string>();
                var path = GetPath(decklistType);
                if (Directory.Exists(path))
                {
                    foreach (var file in Directory.GetFiles(path))
                    {
                        if (!file.EndsWith(".xml")) continue;
                        list.Add(Path.GetFileNameWithoutExtension(file));
                    }
                }
                res[decklistType] = list;
            }
            return res;
        }

        /// <summary>
        /// loads a deck of the given type/name.
        /// </summary>
        internal static MoxDeckList LoadDeck(MoxDeckList.DeckListType decktype, string name)
        {
            var path = GetPath(decktype, name);
            return XmlSerial.FromXmlFile<MoxDeckList>(path);
        }

        /// <summary>
        /// Returns the path where decks of the given type are stored.
        /// </summary>
        internal static string GetPath(MoxDeckList.DeckListType decktype)
        {
            var decktypeName = Enum.GetName(typeof(MoxDeckList.DeckListType), decktype);
            if (decktypeName == null) throw new Exception("Unable to get decktype of: " + decktype);
            return Path.Combine(DeckListPath, decktypeName);
        }

        /// <summary>
        /// Returns the path where the given deck is stored.
        /// </summary>
        internal static string GetPath(MoxDeckList deckList)
        {
            if (!deckList.DeckType.HasValue) throw new ArgumentException("No deck list type provided", nameof(deckList));
            return GetPath(deckList.DeckType.Value, deckList.Name);
        }

        /// <summary>
        /// Returns the path where a deck of the specified name/type is stored.
        /// </summary>
        internal static string GetPath(MoxDeckList.DeckListType decktype, string name)
        {
            return Path.Combine(GetPath(decktype), name + ".xml");
        }
    }
}
