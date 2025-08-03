using Newtonsoft.Json;

namespace MtgManager.ChooseImages
{
    /// <summary>
    /// Top level object representing a page of cards from scryfall.
    /// </summary>
    public class ScryfallCardList
    {
        // should be "list"
        [JsonProperty("object")]
        public string? Object = default;

        [JsonProperty("total_cards")]
        public int? TotalCards = default;

        [JsonProperty("has_more")]
        public bool? HasMore = default;

        [JsonProperty("data")]
        public List<ScryfallCard>? Data = default;
    }

    /// <summary>
    /// Represents a single card returned from scryfall
    /// </summary>
    public class ScryfallCard
    {
        // Should be "card"
        [JsonProperty("object")]
        public string? Object = default;

        [JsonProperty("id")]
        public string? Id = default;

        [JsonProperty("oracle_id")]
        public string? OracleId = default;

        [JsonProperty("multiverse_ids")]
        public int[]? MultiverseIds = default;

        [JsonProperty("mtgo_id")]
        public int? MtgoId = default;

        [JsonProperty("arena_id")]
        public int? ArenaId = default;

        [JsonProperty("tcgplayer_id")]
        public int? TcgPlayerId = default;

        [JsonProperty("cardmarket_id")]
        public int? CardMarketId = default;

        [JsonProperty("name")]
        public string? Name = default;

        // TODO WESD enum?
        [JsonProperty("lang")]
        public string? Lang = default;

        [JsonProperty("released_at")]
        public DateTime? ReleasedAt = default;

        [JsonProperty("uri")]
        public string? Uri = default;

        [JsonProperty("scryfall_uri")]
        public string? ScryfallUri = default;

        // TODO WESD enum?
        [JsonProperty("layout")]
        public string? Layout = default;

        [JsonProperty("highres_image")]
        public bool? HighresImage = default;

        // TODO WESD enum?
        [JsonProperty("image_status")]
        public string? ImageStatus = default;

        [JsonProperty("image_uris")]
        public ScryfallImageUris? ImageUris = default;

        [JsonProperty("mana_cost")]
        public string? ManaCost = default;

        [JsonProperty("cmc")]
        public decimal? Cmc = default;

        [JsonProperty("type_line")]
        public string? TypeLine = default;

        [JsonProperty("oracle_text")]
        public string? OracleText = default;

        // TODO WESD enum?
        [JsonProperty("colors")]
        public string[]? Colors = default;

        // TODO WESD enum?
        [JsonProperty("color_identity")]
        public string[]? ColorIdentity = default;

        // TODO WESD enum?
        [JsonProperty("keywords")]
        public string[]? Keywords = default;

        [JsonProperty("card_faces")]
        public ScryfallCardFace[]? CardFaces = default;

        // TODO WESD enum?
        [JsonProperty("produced_mana")]
        public string[]? ProducedMana = default;

        [JsonProperty("all_parts")]
        public ScryfallRelatedCard[]? AllParts = default;

        [JsonProperty("legalities")]
        public ScryfallLegalities? Legalities = default;

        // TODO WESD enum?
        [JsonProperty("games")]
        public string[]? Games = default;

        [JsonProperty("reserved")]
        public bool? Reserved = default;

        [JsonProperty("foil")]
        public bool? Foil = default;

        [JsonProperty("nonfoil")]
        public bool? NonFoil = default;

        // TODO WESD enum?
        [JsonProperty("finishes")]
        public string[]? Finishes = default;

        [JsonProperty("oversized")]
        public bool? Oversized = default;

        [JsonProperty("promo")]
        public bool? Promo = default;

        [JsonProperty("reprint")]
        public bool? Reprint = default;

        [JsonProperty("variation")]
        public bool? Variation = default;

        [JsonProperty("set_id")]
        public string? SetId = default;

        [JsonProperty("set")]
        public string? Set = default;

        [JsonProperty("set_name")]
        public string? SetName = default;

        [JsonProperty("set_type")]
        public string? SetType = default;

        [JsonProperty("set_uri")]
        public string? SetUri = default;

        [JsonProperty("set_search_uri")]
        public string? SetSearchUri = default;

        [JsonProperty("scryfall_set_uri")]
        public string? ScryfallSetUri = default;

        [JsonProperty("rullings_uri")]
        public string? RullingsUri = default;

        [JsonProperty("prints_search_uri")]
        public string? PrintsSearchUri = default;

        [JsonProperty("collector_number")]
        public string? CollectorNumber = default;

        [JsonProperty("digital")]
        public bool? Digital = default;

        // TODO WESD enum?
        [JsonProperty("rarity")]
        public string? Rarity = default;

        [JsonProperty("flavor_text")]
        public string? FlavorText = default;

        [JsonProperty("card_back_id")]
        public string? CardBackId = default;

        // string if single artist, but string[] if multiple artists (ex. double faced cards)
        //[JsonProperty("artist")]
        //public string? Artist = default;

        //[JsonProperty("artist_ids")]
        //public string[]? ArtistIds = default;

        [JsonProperty("illustration_id")]
        public string? IllustrationId = default;

        // TODO WESD enum?
        [JsonProperty("border_color")]
        public string? BorderColor = default;

        // TODO WESD enum?
        [JsonProperty("frame")]
        public string? Frame = default;

        // TODO WESD enum?
        [JsonProperty("frame_effects")]
        public string[]? FrameEffects = default;

        [JsonProperty("full_art")]
        public bool? FullArt = default;

        [JsonProperty("textless")]
        public bool? Textless = default;

        [JsonProperty("booster")]
        public bool? Booster = default;

        [JsonProperty("story_spotlight")]
        public bool? StorySpotlight = default;

        [JsonProperty("edhrec_rank")]
        public bool? EdhrecRank = default;

        [JsonProperty("penny_rank")]
        public bool? PennyRank = default;

        [JsonProperty("prices")]
        public ScryfallPrices? Prices = default;

        [JsonProperty("related_uris")]
        public ScryfallRelatedUris? RelatedUris = default;

        [JsonProperty("purchase_uris")]
        public ScryfallPurchaseUris? PurchaseUris = default;
    }

    public class ScryfallCardFace
    {
        [JsonProperty("object")]
        public string? Object = default;

        [JsonProperty("name")]
        public string? Name = default;

        [JsonProperty("type_line")]
        public string? TypeLine = default;

        [JsonProperty("oracle_text")]
        public string? OracleText = default;

        // TODO WESD enum?
        [JsonProperty("colors")]
        public string[]? Colors = default;

        //[JsonProperty("artist")]
        //public string[]? Artist = default;

        //[JsonProperty("artist_id")]
        //public string[]? ArtistIds = default;

        [JsonProperty("illustration_id")]
        public string? IllustrationId = default;

        [JsonProperty("image_uris")]
        public ScryfallImageUris? ImageUris = default;
    }

    public class ScryfallPurchaseUris
    {
        [JsonProperty("tcgplayer")]
        public string? Tcgplayer = default;

        [JsonProperty("cardmarket")]
        public string? Cardmarket = default;

        [JsonProperty("cardhoarder")]
        public string? Cardhoarder = default;
    }

    public class ScryfallRelatedUris
    {
        [JsonProperty("gatherer")]
        public string? Gatherer = default;

        [JsonProperty("tcgplayer_infinite_articles")]
        public string? TcgplayerInfiniteArticles = default;

        [JsonProperty("tcgplayer_infinite_decks")]
        public string? TcgplayerInfiniteDecks = default;

        [JsonProperty("edhrec")]
        public string? EdhRec = default;
    }

    public class ScryfallPrices
    {
        [JsonProperty("usd")]
        public decimal? Usd = default;

        [JsonProperty("usd_foil")]
        public decimal? UsdFoil = default;

        [JsonProperty("usd_etched")]
        public decimal? UsdEtched = default;

        [JsonProperty("eur")]
        public decimal? Eur = default;

        [JsonProperty("eur_foil")]
        public decimal? EurFoil = default;

        [JsonProperty("tix")]
        public decimal? Tix = default;
    }

    public class ScryfallLegalities
    {
        [JsonProperty("standard")]
        public string? Standard = default;

        [JsonProperty("future")]
        public string? Future = default;

        [JsonProperty("historic")]
        public string? Historic = default;

        [JsonProperty("gladiator")]
        public string? Gladiator = default;

        [JsonProperty("pioneer")]
        public string? Pioneer = default;

        [JsonProperty("explorer")]
        public string? Explorer = default;

        [JsonProperty("modern")]
        public string? Modern = default;

        [JsonProperty("legacy")]
        public string? Legacy = default;

        [JsonProperty("pauper")]
        public string? Pauper = default;

        [JsonProperty("vintage")]
        public string? Vintage = default;

        [JsonProperty("penny")]
        public string? Penny = default;

        [JsonProperty("commander")]
        public string? Commander = default;

        [JsonProperty("brawl")]
        public string? Brawl = default;

        [JsonProperty("historicbrawl")]
        public string? HistoricBrawl = default;

        [JsonProperty("alchemy")]
        public string? Alchemy = default;

        [JsonProperty("paupercommander")]
        public string? PauperCommander = default;

        [JsonProperty("duel")]
        public string? Duel = default;

        [JsonProperty("oldschool")]
        public string? OldSchool = default;

        [JsonProperty("premodern")]
        public string? Premodern = default;
    }

    public class ScryfallRelatedCard
    {
        [JsonProperty("object")]
        public string? Object = default;

        [JsonProperty("id")]
        public string? Id = default;

        [JsonProperty("component")]
        public string? Component = default;

        [JsonProperty("name")]
        public string? Name = default;

        [JsonProperty("type_line")]
        public string? TypeLine = default;

        [JsonProperty("uri")]
        public string? Uri = default;
    }

    public class ScryfallImageUris
    {
        [JsonProperty("small")]
        public string? Small = default;

        [JsonProperty("normal")]
        public string? Normal = default;

        [JsonProperty("large")]
        public string? Large = default;

        [JsonProperty("png")]
        public string? Png = default;

        [JsonProperty("art_crop")]
        public string? ArtCrop = default;

        [JsonProperty("border_crop")]
        public string? BorderCrop = default;
    }
}
