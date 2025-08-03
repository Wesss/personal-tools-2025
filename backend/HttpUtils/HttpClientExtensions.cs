using System.Reflection;
using WindowsUtils;

namespace HttpUtils
{
    public static class HttpClientExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public static string Get(this HttpClient client, string url)
        {
            using (var stream = client.GetStream(url))
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }

        public static Stream GetStream(this HttpClient client, string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            return client.Send(request).Content.ReadAsStream();
        }

        /// <summary>
        /// Sends a GET to the given url, and saves contents to given file.
        /// </summary>
        public static void SaveToFile(this HttpClient client, string url, string filepath)
        {
            if (FileSystemUtil.ContainsIllegalPathChars(filepath))
            {
                throw new ArgumentException($"filepath \"{filepath}\" contains illegal characters", nameof(filepath));
            }
            log.Debug($"wesd saving to file {filepath}");
            using var httpStream = client.GetStream(url);
            using var fileStream = File.Open(filepath, File.Exists(filepath) ? FileMode.Truncate : FileMode.OpenOrCreate);
            httpStream.CopyTo(fileStream);
        }
    }
}