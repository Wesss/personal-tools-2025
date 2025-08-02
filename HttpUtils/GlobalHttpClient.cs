
namespace HttpUtils
{
    /// <summary>
    /// Holds a singleton client, doesn't ever dispose. We just release everything on program execution finish
    /// </summary>
    public static class GlobalHttpClient
    {
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private static HttpClient? client = null;

        public static HttpClient GetClient()
        {
            if (client == null)
            {
                client = new HttpClient();
            }
            return client;
        }
    }
}