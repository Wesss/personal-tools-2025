namespace MtgManager.ChooseImages
{
    /// <summary>
    /// Represents a proxiable image, with card/order data saved via file name.
    /// </summary>
    public class CardImage
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public string? FilePath { get; set; }

        public string? Name { get; set; }

        /// <summary>
        /// Internal order ID, to group cards with multiple images together
        /// </summary>
        public int OrderID { get; set; }
        public string? Set { get; set; }
        public string? SetType { get; set; }
        public string? BorderColor { get; set; }
        public string? Frame { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public string[]? FrameEffects { get; set; } = Array.Empty<string>();
        public string? TypeLine { get; set; }

        public ScryfallImageUris? ImageUris { get; set; }

        public string? FileExtension { get; set; }

        public string ToImgFileName()
        {
            var cardName = Name!.Replace("/", "++").Replace(":", "-");
            return $"{OrderID.ToString("D4")}--{cardName}--{Set ?? "none"}--{SetType ?? "none"}--{BorderColor ?? "none"}--{Frame ?? "none"}--{ReleasedAt?.ToString("yyyy-MM-dd") ?? "none"}--{string.Join(",", FrameEffects ?? new string[] { "none" })}--{TypeLine?.Replace("/", "++") ?? "none"}.{FileExtension}";
        }

        public static CardImage FromImgFile(string path)
        {
            var res = new CardImage();
            res.FilePath = path;

            var filename = Path.GetFileNameWithoutExtension(path);
            var split = filename.Split("--");
            res.OrderID = int.Parse(split[0]);
            res.Name = split[1].Replace("++", "/");
            res.Set = split[2];
            res.SetType = split[3];
            res.BorderColor = split[4];
            res.Frame = split[5];
            res.ReleasedAt = DateTime.Parse(split[6]);
            res.FrameEffects = split[7].Split(",");
            res.TypeLine = split[8].Replace("/", "++");

            return res;
        }
    }
}
