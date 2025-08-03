using HttpUtils;
using SqliteUtils.Cache;
using Newtonsoft.Json;

namespace MtgManager.ChooseImages
{
    /// <summary>
    /// see https://scryfall.com/docs/api
    /// </summary>
    public class ScryfallApiClient : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        /*
        https://api.scryfall.com
Rate Limits and Good Citizenship
We kindly ask that you insert 50 – 100 milliseconds of delay between the requests you send to the server at api.scryfall.com.
(i.e., 10 requests per second on average).

Submitting excessive requests to the server may result in a HTTP 429 Too Many Requests status code. Continuing to overload the
API after this point may result in a temporary or permanent ban of your IP address.

The file origins used by the API, such as c1.scryfall.com, c2.scryfall.com, and c3.scryfall.com do not have these rate limits.

We encourage you to cache the data you download from Scryfall or process it locally in your own database, at least for 24 hours.
Scryfall provides our entire database compressed for download in bulk data files.

While we make incremental updates to card data daily, take note that:

We only update prices for cards once per day. Fetching card data more frequently than 24 hours will not yield new prices.

Updates to gameplay data (such as card names, Oracle text, mana costs, etc) are much less frequent. If you only need gameplay
information, downloading card data once per week or right after set releases would most likely be sufficient.
        */

        private const string CacheGroup = "MtgManager.GenerateProxies.ScryfallApiClient";
        private const string CachePath = @"D:/PersonalTools2025/MtgManager";
        /// <summary>
        /// 10 per second
        /// </summary>
        private const int ThrottleMilis = 100;
        private DateTime lastReq;
        /// <summary>
        /// 1 week
        /// </summary>
        private const int GameplayDataTTL = 60 * 60 * 24 * 7;
        private readonly PersistentCache cache;
        private readonly HttpClient httpClient;

        private const string APIBase = "https://api.scryfall.com";

        private bool disposedValue;

        public ScryfallApiClient()
        {
            cache = new PersistentCache(CachePath, CacheGroup);
            httpClient = GlobalHttpClient.GetClient();
            lastReq = DateTime.MinValue;

            // required headers, see https://scryfall.com/docs/api
            var headers = httpClient.DefaultRequestHeaders;
            headers.Add("User-Agent", "MtgManager/1.0");
            headers.Add("Accept", "*/*");
        }

        /// <summary>
        /// https://scryfall.com/docs/api/cards/search
        /// </summary>
        public ScryfallCardList CardsSearch(ScryfallSearchArgs args)
        {
            var url = APIBase + "/cards/search?" + args.ToQueryString();

            var resp = cache.Get(url, GameplayDataTTL);
            var newResp = false;
            if (resp == null)
            {
                Throttle();
                resp = httpClient.Get(url);
                newResp = true;
            }
            var res = JsonConvert.DeserializeObject<ScryfallCardList>(resp);
            if (res == null) throw new Exception("Unable to deserialize API response: " + resp);
            if (newResp) cache.Set(url, resp);
            return res;
        }

        /// <summary>
        /// Execution only passes this method at most 10 times per second. This aligns with Scryfall API's rate limiting.
        /// </summary>
        private void Throttle()
        {
            var targetTime = lastReq.AddMilliseconds(ThrottleMilis);
            while (targetTime > DateTime.Now)
            {
                Thread.Sleep((int)Math.Ceiling((targetTime - DateTime.Now).TotalMilliseconds));
            }

            lastReq = DateTime.Now;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                cache?.Dispose();
                // set large fields to null
                disposedValue = true;
            }
        }

        // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~ScryfallApiClient()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
