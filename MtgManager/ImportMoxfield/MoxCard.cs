using DomainUtils;
using System.Formats.Asn1;
using System.Globalization;

namespace MtgManager.ImportMoxfield
{
    internal class MoxCard : XmlSerial
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public string Name = string.Empty;

        public string[] Tags = Array.Empty<string>();

        public MoxCard() {}

        /// <summary>
        /// Parses a string representation of a list of moxfield cards (lines of bulk edit tool)
        /// </summary>
        public static MoxCard[] ParseList(string list)
        {
            var res = new List<MoxCard>();
            foreach (var line in list.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                var tokens = line.Split(' ');
                var count = int.Parse(tokens[0]);

                // example "1 Verdant Command (MH2) 182 #token-base"
                var seenSet = false;
                var seenTag = false;
                var name = string.Empty;
                var tags = new List<string>();
                for (int i = 1; i < tokens.Length; i++)
                {
                    var token = tokens[i];
                    // ignore token to denote foil cards
                    if (token == "*F*") continue;

                    seenTag = seenTag || token.StartsWith("#");
                    seenSet = seenSet || (token.StartsWith("(") && token.EndsWith(")"));
                    if (seenTag)
                    {
                        tags.Add(token.Replace("#", "").Replace("!", ""));
                    }
                    else if (seenSet)
                    {
                        // ignore set and collection number
                        continue;
                    }
                    else if (name.Length == 0)
                    {
                        name = token;
                    }
                    else if (name.Length > 0)
                    {
                        name += " " + token;
                    }
                }
                for (var i = 0; i < count; i++)
                {
                    res.Add(
                        new MoxCard
                        {
                            Name = name,
                            Tags = tags.Distinct().ToArray()
                        }
                    );
                }
            }
            return res.ToArray();
        }

        /// <summary>
        /// Parses a the csv export of a moxfield collection into an array of moxfield cards.
        /// </summary>
        /// <returns></returns>
        public static MoxCard[] ParseCollectionCsv(string file)
        {
            using (var reader = File.OpenText(file))
            using (var parser = new NotVisualBasic.FileIO.CsvTextFieldParser(reader))
            {
                // Skip the header line
                if (!parser.EndOfData)
                {
                    // assert correctness of csv headers
                    var header = parser.ReadFields();
                    if (header.Length < 3)
                    {
                        log.Error("not enough header rows in collection csv!");
                        return Array.Empty<MoxCard>();
                    }
                    if (header[0].ToLower() != "count")
                    {
                        log.Error("first csv col is not count!");
                        return Array.Empty<MoxCard>();
                    }
                    if (header[2].ToLower() != "name")
                    {
                        log.Error("3rd csv col is not name!");
                        return Array.Empty<MoxCard>();
                    }
                }

                var res = new List<MoxCard>();
                while (!parser.EndOfData)
                {
                    var csvLine = parser.ReadFields();
                    var count = int.Parse(csvLine[0]);
                    var name = csvLine[2];

                    for (var i = 0; i < count; i++)
                    {
                        res.Add(
                            new MoxCard
                            {
                                Name = name
                            }
                        );
                    }
                }
                return res.ToArray();
            }
        }
    }
}
