using SqliteUtils;
using System.Linq;
using System.Web;

namespace MtgManager.ChooseImages
{
    public class ScryfallSearchArgs
    {
        public enum ScryfallLang
        {
            English
        }

        public enum ScryfallLegal
        {
            Commander
        }

        public enum ScryfallUnique
        {
            Cards,
            Art,
            Prints
        }
        public enum ScryfallGame
        {
            Paper
        }

        public string ExactName = string.Empty;
        public string Name = string.Empty;
        public string SetCode = string.Empty;
        public ScryfallLang? Lang = null;
        public ScryfallGame? Game = null;
        public ScryfallLegal? Legal = null;
        public ScryfallUnique? Unique = null;

        public string ToQueryString()
        {
            var qs = HttpUtility.ParseQueryString(string.Empty);

            var query = new List<string>();
            if (Unique != null) query.Add($"unique:{Enum.GetName(Unique.Value)}");
            if (!string.IsNullOrEmpty(ExactName)) query.Add($"!\"{ExactName}\"");
            else if (!string.IsNullOrEmpty(Name)) query.Add($"name:\"{ExactName}\"");
            if (!string.IsNullOrEmpty(SetCode)) query.Add($"set:\"{SetCode}\"");
            if (Lang != null) query.Add($"lang:{Enum.GetName(Lang.Value)}");
            if (Game != null) query.Add($"game:{Enum.GetName(Game.Value)}");
            if (Legal != null) query.Add($"legal:{Enum.GetName(Legal.Value)}");

            qs["q"] = string.Join(" ", query);

            var res = qs.ToString();
            if (res == null) throw new Exception("null query string generation");
            return res;
        }

        public static ScryfallSearchArgs GetDefault()
        {
            return new ScryfallSearchArgs()
            {
                Lang = ScryfallLang.English,
                Game = ScryfallGame.Paper
            };
        }
    }
}
